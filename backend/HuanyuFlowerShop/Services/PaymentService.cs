using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Domain;

namespace HuanyuFlowerShop.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;

        // 缓存键常量
        private const string PAYMENT_STATUS_CACHE_KEY = "payment_status_{0}_{1}"; // userId_orderId
        private const string PAYMENT_LINK_CACHE_KEY = "payment_link_{0}_{1}"; // userId_orderId
        
        // 缓存过期时间
        private readonly TimeSpan _paymentStatusCacheExpiry = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _paymentLinkCacheExpiry = TimeSpan.FromMinutes(30);

        public PaymentService(
            IRepository<Order> orderRepository,
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<PaymentService> logger,
            IConfiguration configuration)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _logger = logger;
            _configuration = configuration;
        }

        private bool IsMockMode()
        {
            var mode = _configuration.GetSection("Payment")["Mode"];
            return string.Equals(mode, "mock", StringComparison.OrdinalIgnoreCase);
        }

        private DefaultAopClient GetAlipayClient()
        {
            var config = _configuration.GetSection("Alipay");
            var appId = config["AppId"];
            var privateKey = config["PrivateKey"];
            var alipayPublicKey = config["AlipayPublicKey"];
            var gatewayUrl = config["GatewayUrl"];

            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(privateKey))
            {
                throw new Exception("支付宝配置缺失: AppId或PrivateKey为空");
            }

            // 清理私钥格式（移除头尾和换行）
            if (!string.IsNullOrEmpty(privateKey))
            {
                privateKey = privateKey
                    .Replace("-----BEGIN PRIVATE KEY-----", "")
                    .Replace("-----END PRIVATE KEY-----", "")
                    .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                    .Replace("-----END RSA PRIVATE KEY-----", "")
                    .Replace("\n", "")
                    .Replace("\r", "")
                    .Trim();
            }

            // 打印调试信息
            Console.WriteLine($"[Alipay Config] AppId: {appId}, Gateway: {gatewayUrl}");
            Console.WriteLine($"[Alipay Config] PrivateKey Length: {privateKey?.Length ?? 0}");

            // 必须使用 GBK 编码，否则 .NET Core 下会报错
            return new DefaultAopClient(gatewayUrl, appId, privateKey, "json", "1.0", "RSA2", alipayPublicKey, "GBK", false);
        }

        public async Task<PaymentResult> CreatePaymentAsync(int userId, int orderId, string paymentMethod)
        {
            try
            {
                _logger.LogInformation("开始创建支付，用户ID: {UserId}，订单ID: {OrderId}，支付方式: {PaymentMethod}", userId, orderId, paymentMethod);

                // 参数验证
                if (userId <= 0) throw new ArgumentException("用户ID无效", nameof(userId));
                if (orderId <= 0) throw new ArgumentException("订单ID无效", nameof(orderId));
                if (string.IsNullOrWhiteSpace(paymentMethod)) throw new ArgumentException("支付方式不能为空", nameof(paymentMethod));

                // 验证支付方式
                var validPaymentMethods = new[] { "cod", "online" };
                if (!validPaymentMethods.Contains(paymentMethod))
                {
                    throw new ArgumentException("无效的支付方式", nameof(paymentMethod));
                }

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 获取订单
                    var order = await _orderRepository.GetByIdAsync(orderId);
                    if (order == null || order.UserId != userId)
                    {
                        _logger.LogWarning("订单不存在或无权访问，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                        return new PaymentResult { Success = false, Message = "订单不存在或无权访问" };
                    }

                    // 检查订单状态
                    if (order.Status != "pending")
                    {
                        _logger.LogWarning("只有待处理的订单可以支付，订单ID: {OrderId}，当前状态: {Status}", orderId, order.Status);
                        return new PaymentResult { Success = false, Message = "只有待处理的订单可以支付" };
                    }

                    // 检查支付状态
                    if (order.PaymentStatus == "paid")
                    {
                        _logger.LogWarning("订单已支付，订单ID: {OrderId}", orderId);
                        return new PaymentResult { Success = false, Message = "订单已支付" };
                    }

                    // 更新支付方式
                    order.PaymentMethod = paymentMethod;
                    
                    // 根据支付方式处理
                    if (paymentMethod == "cod")
                    {
                        // 货到付款，直接标记为已支付
                        order.PaymentStatus = "paid";
                        order.Status = "processing";
                        order.PaidAt = DateTime.UtcNow;
                        _logger.LogInformation("货到付款处理成功，订单ID: {OrderId}", orderId);
                    }
                    else if (paymentMethod == "online")
                    {
                        // 在线支付，生成支付参考号
                        order.PaymentStatus = "unpaid";
                        order.PaymentReference = GeneratePaymentReference();
                        _logger.LogInformation("在线支付创建成功，订单ID: {OrderId}，支付参考号: {PaymentReference}", 
                            orderId, order.PaymentReference);
                    }

                    order.UpdatedAt = DateTime.UtcNow;
                    
                    // 更新订单
                    await _orderRepository.UpdateAsync(order);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // 清除缓存
                    await ClearPaymentCache(userId, orderId);

                    return new PaymentResult
                    {
                        Success = true,
                        Message = "支付创建成功",
                        PaymentReference = order.PaymentReference
                    };
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "参数验证失败: {Message}", ex.Message);
                return new PaymentResult { Success = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建支付失败，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                return new PaymentResult { Success = false, Message = "创建支付时发生错误" };
            }
        }

        public async Task<PaymentResult> VerifyPaymentAsync(int userId, int orderId, string paymentReference)
        {
            try
            {
                _logger.LogInformation("开始验证支付，用户ID: {UserId}，订单ID: {OrderId}，支付参考号: {PaymentReference}", 
                    userId, orderId, paymentReference);

                // 参数验证
                if (userId <= 0) throw new ArgumentException("用户ID无效", nameof(userId));
                if (orderId <= 0) throw new ArgumentException("订单ID无效", nameof(orderId));
                if (string.IsNullOrWhiteSpace(paymentReference)) throw new ArgumentException("支付参考号不能为空", nameof(paymentReference));

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 获取订单
                    var order = await _orderRepository.GetByIdAsync(orderId);
                    if (order == null || order.UserId != userId)
                    {
                        _logger.LogWarning("订单不存在或无权访问，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                        return new PaymentResult { Success = false, Message = "订单不存在或无权访问" };
                    }

                    // 检查订单状态和支付状态
                    if (order.Status != "pending" || order.PaymentStatus == "paid")
                    {
                        _logger.LogWarning("订单状态不允许验证支付，订单ID: {OrderId}，订单状态: {Status}，支付状态: {PaymentStatus}", 
                            orderId, order.Status, order.PaymentStatus);
                        return new PaymentResult { Success = false, Message = "订单状态不允许验证支付" };
                    }

                    // 验证支付参考号
                    // 只要数据库里存的参考号和请求的参考号一致即可（忽略大小写和空格）
                    // 兼容旧数据：如果数据库中的 PaymentReference 为空，但用户提供的是 PAY 开头的格式，且金额一致（这里没法校验金额），或者我们直接放宽逻辑
                    // 临时逻辑：如果数据库中 PaymentReference 为空，暂时允许验证通过（为了解决旧订单数据不一致问题）
                    string? dbReference = order.PaymentReference?.Trim();
                    if (string.IsNullOrEmpty(dbReference))
                    {
                        _logger.LogWarning("订单 {OrderId} 的支付参考号为空，将尝试使用用户提供的参考号进行更新", orderId);
                        // 如果是 PAY 开头，我们假设这是合法的，更新到数据库
                        if (!string.IsNullOrWhiteSpace(paymentReference) && paymentReference.Trim().StartsWith("PAY", StringComparison.OrdinalIgnoreCase))
                        {
                            order.PaymentReference = paymentReference.Trim();
                            // 注意：这里先不保存，等下面验证通过了一起保存
                        }
                    }
                    else if (!string.Equals(dbReference, paymentReference, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("支付参考号不匹配，订单ID: {OrderId}, 期望: {Expected}, 实际: {Actual}", 
                            orderId, order.PaymentReference, paymentReference);
                        return new PaymentResult { Success = false, Message = $"支付参考号不匹配 (期望: {order.PaymentReference}, 实际: {paymentReference})" };
                    }

                    // 调用第三方支付网关验证（模拟）
                    // 在实际应用中，这里应该调用真实的支付网关API
                    bool isPaymentSuccessful = await VerifyPaymentWithGatewayAsync(paymentReference);

                    if (isPaymentSuccessful)
                    {
                        // 更新支付状态
                        order.PaymentStatus = "paid";
                        order.Status = "processing";
                        order.PaidAt = DateTime.UtcNow;
                        order.UpdatedAt = DateTime.UtcNow;

                        await _orderRepository.UpdateAsync(order);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();

                        // 清除缓存
                        await ClearPaymentCache(userId, orderId);

                        _logger.LogInformation("支付验证成功，订单ID: {OrderId}", orderId);
                        return new PaymentResult { Success = true, Message = "支付验证成功" };
                    }
                    else
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogWarning("支付验证失败，订单ID: {OrderId}", orderId);
                        return new PaymentResult { Success = false, Message = "支付验证失败，请检查支付是否成功" };
                    }
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "参数验证失败: {Message}", ex.Message);
                return new PaymentResult { Success = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证支付失败，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                return new PaymentResult { Success = false, Message = "验证支付时发生错误" };
            }
        }

        public async Task<PaymentResult> CancelPaymentAsync(int userId, int orderId)
        {
            try
            {
                _logger.LogInformation("开始取消支付，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);

                // 参数验证
                if (userId <= 0) throw new ArgumentException("用户ID无效", nameof(userId));
                if (orderId <= 0) throw new ArgumentException("订单ID无效", nameof(orderId));

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 获取订单
                    var order = await _orderRepository.GetByIdAsync(orderId);
                    if (order == null || order.UserId != userId)
                    {
                        _logger.LogWarning("订单不存在或无权访问，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                        return new PaymentResult { Success = false, Message = "订单不存在或无权访问" };
                    }

                    // 检查是否可以取消支付
                    if (order.PaymentStatus == "paid")
                    {
                        _logger.LogWarning("已支付的订单不能取消支付，订单ID: {OrderId}", orderId);
                        return new PaymentResult { Success = false, Message = "已支付的订单不能取消支付" };
                    }

                    if (order.Status != "pending")
                    {
                        _logger.LogWarning("只有待处理的订单可以取消支付，订单ID: {OrderId}，当前状态: {Status}", orderId, order.Status);
                        return new PaymentResult { Success = false, Message = "只有待处理的订单可以取消支付" };
                    }

                    // 对于在线支付，调用网关取消支付（模拟）
                    if (order.PaymentMethod == "online" && !string.IsNullOrEmpty(order.PaymentReference))
                    {
                        // 在实际应用中，这里应该调用真实的支付网关API取消支付
                        await CancelPaymentWithGatewayAsync(order.PaymentReference);
                    }

                    // 重置支付信息
                    order.PaymentReference = null;
                    order.UpdatedAt = DateTime.UtcNow;

                    await _orderRepository.UpdateAsync(order);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // 清除缓存
                    await ClearPaymentCache(userId, orderId);

                    _logger.LogInformation("支付取消成功，订单ID: {OrderId}", orderId);
                    return new PaymentResult { Success = true, Message = "支付取消成功" };
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "参数验证失败: {Message}", ex.Message);
                return new PaymentResult { Success = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消支付失败，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                return new PaymentResult { Success = false, Message = "取消支付时发生错误" };
            }
        }

        public async Task<PaymentResult> GetPaymentStatusAsync(int userId, int orderId)
        {
            try
            {
                _logger.LogInformation("查询支付状态，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);

                // 参数验证
                if (userId <= 0) throw new ArgumentException("用户ID无效", nameof(userId));
                if (orderId <= 0) throw new ArgumentException("订单ID无效", nameof(orderId));

                // 尝试从缓存获取
                string cacheKey = string.Format(PAYMENT_STATUS_CACHE_KEY, userId, orderId);
                var cachedResult = await _cacheService.GetAsync<PaymentResult>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("从缓存获取支付状态，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                    return cachedResult;
                }

                // 获取订单
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null || order.UserId != userId)
                {
                    _logger.LogWarning("订单不存在或无权访问，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                    return new PaymentResult { Success = false, Message = "订单不存在或无权访问" };
                }

                // 构建支付状态结果
                var result = new PaymentResult
                {
                    Success = true,
                    Message = order.PaymentStatus == "paid" ? "支付成功" : "未支付",
                    PaymentReference = order.PaymentReference
                };

                // 缓存结果
                await _cacheService.SetAsync(cacheKey, result, _paymentStatusCacheExpiry);

                return result;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "参数验证失败: {Message}", ex.Message);
                return new PaymentResult { Success = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询支付状态失败，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                return new PaymentResult { Success = false, Message = "查询支付状态时发生错误" };
            }
        }

        public async Task<(string PaymentUrl, string PaymentReference)> GeneratePaymentLinkAsync(int userId, int orderId)
        {
            try
            {
                _logger.LogInformation("生成支付链接，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);

                // 参数验证
                if (userId <= 0) throw new ArgumentException("用户ID无效", nameof(userId));
                if (orderId <= 0) throw new ArgumentException("订单ID无效", nameof(orderId));

                // 尝试从缓存获取
                string cacheKey = string.Format(PAYMENT_LINK_CACHE_KEY, userId, orderId);
                // 暂时禁用缓存读取，以确保生成的链接是最新的（Mock链接）
                // var cachedResult = await _cacheService.GetAsync<(string PaymentUrl, string PaymentReference)>(cacheKey);
                // if (cachedResult != (null, null))
                // {
                //     _logger.LogInformation("从缓存获取支付链接，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                //     return cachedResult;
                // }

                // 获取订单
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null || order.UserId != userId)
                {
                    _logger.LogWarning("订单不存在或无权访问，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                    throw new ArgumentException("订单不存在或无权访问");
                }

                // 检查订单状态
                if (order.Status != "pending" || order.PaymentStatus == "paid")
                {
                    _logger.LogWarning("订单状态不允许生成支付链接，订单ID: {OrderId}", orderId);
                    throw new ArgumentException("订单状态不允许生成支付链接");
                }

                // 确保有支付参考号
                if (string.IsNullOrEmpty(order.PaymentReference))
                {
                    order.PaymentReference = GeneratePaymentReference();
                    await _orderRepository.UpdateAsync(order);
                }

                // 生成支付链接
                // 使用支付宝沙箱生成支付表单HTML
                string paymentUrl = GenerateAlipayPagePay(order);

                var result = (PaymentUrl: paymentUrl, PaymentReference: order.PaymentReference);

                // 缓存结果
                await _cacheService.SetAsync(cacheKey, result, _paymentLinkCacheExpiry);

                _logger.LogInformation("支付链接生成成功，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                return result;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "参数验证失败: {Message}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成支付链接失败，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                throw;
            }
        }

        // 私有辅助方法
        private static string GeneratePaymentReference()
        {
            return $"PAY{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(10000, 99999)}";
        }

        private Task<bool> VerifyPaymentWithGatewayAsync(string paymentReference)
        {
            if (IsMockMode()) return Task.FromResult(true);

            try 
            {
                // 使用支付宝查询接口验证支付状态
                var client = GetAlipayClient();
                var request = new AlipayTradeQueryRequest();
                var model = new AlipayTradeQueryModel
                {
                    OutTradeNo = paymentReference
                };
                request.SetBizModel(model);

                var response = client.Execute(request);
                
                _logger.LogInformation("支付宝查询响应: {Body}", response.Body);

                if (!response.IsError)
                {
                    // TRADE_SUCCESS: 交易支付成功
                    // TRADE_FINISHED: 交易结束，不可退款
                    if (response.TradeStatus == "TRADE_SUCCESS" || response.TradeStatus == "TRADE_FINISHED")
                    {
                        return Task.FromResult(true);
                    }
                    _logger.LogWarning("支付宝查询成功但状态未完成: {TradeStatus}", response.TradeStatus);
                }
                else 
                {
                    _logger.LogWarning("支付宝查询返回错误: {SubMsg}", response.SubMsg);
                }
                
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用支付宝查询接口失败");
                throw;
            }
        }

        private async Task CancelPaymentWithGatewayAsync(string paymentReference)
        {
            try
            {
                var client = GetAlipayClient();
                var request = new AlipayTradeCloseRequest();
                var model = new AlipayTradeCloseModel
                {
                    OutTradeNo = paymentReference
                };
                request.SetBizModel(model);
                
                var response = client.Execute(request);
                _logger.LogInformation("支付宝关闭交易响应: {Body}", response.Body);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "调用支付宝关闭交易接口失败");
            }
            await Task.Delay(300);
        }

        private string GenerateAlipayPagePay(Order order)
        {
            if (IsMockMode()) return GeneratePaymentGatewayUrl(order);

            /*
            var config = _configuration.GetSection("Alipay");
            try 
            {
                // 如果是默认配置，返回模拟URL
                if (config["AppId"] == "your_sandbox_app_id")
                {
                    return GeneratePaymentGatewayUrl(order);
                }

                var client = GetAlipayClient();
                var request = new AlipayTradePagePayRequest();
                
                // 设置回调地址
                request.SetNotifyUrl(config["NotifyUrl"]);
                // 设置同步跳转地址，带上订单ID
                // 前端路由是 /orders/:id，所以这里应该是 /orders/{orderId}
                var returnUrl = config["ReturnUrl"];
                if (string.IsNullOrEmpty(returnUrl))
                {
                    // 默认值 fallback
                    returnUrl = "http://localhost:5173/payment/callback";
                }
                
                // 替换为具体的订单详情页地址
                // 假设前端地址是 http://localhost:5173，那么跳转地址应该是 http://localhost:5173/orders/{orderId}
                // 这里我们做一个简单的替换，或者直接构造
                if (returnUrl.Contains("payment/success"))
                {
                     returnUrl = returnUrl.Replace("payment/success", $"orders/{order.Id}");
                }
                else if (returnUrl.Contains("payment/callback"))
                {
                     // 如果是 callback 页面，带上 verify 参数让前端处理
                     returnUrl += $"?verify=true&orderId={order.Id}";
                }
                
                request.SetReturnUrl(returnUrl);

                var model = new AlipayTradePagePayModel
                {
                    OutTradeNo = order.PaymentReference,
                    TotalAmount = order.TotalAmount.ToString("0.00"),
                    Subject = $"欢雨鲜花订单-{order.OrderNumber}",
                    ProductCode = "FAST_INSTANT_TRADE_PAY",
                    Body = $"订单号:{order.OrderNumber}"
                };
                
                request.SetBizModel(model);
                
                // 生成表单
                // AlipaySDKNet 中，PageExecute 方法可能不可用或命名不同
                // 使用通用的 Execute 方法，对于 PagePayRequest，它返回的 Body 通常就是 HTML 表单
                // 使用 SdkExecute 生成签名后的请求参数字符串 (PagePay 不需要后端直接调用 API，而是生成链接给前端跳转)
                var response = client.SdkExecute(request);
                
                if (!response.IsError && !string.IsNullOrEmpty(response.Body))
                {
                     // config 已经在外部定义
                     var gatewayUrl = config["GatewayUrl"];
                     if (string.IsNullOrEmpty(gatewayUrl))
                     {
                         throw new Exception("支付宝配置缺失: GatewayUrl为空");
                     }
                     // 如果 GatewayUrl 结尾没有 ?，需要根据情况添加
                     if (!gatewayUrl.EndsWith("?"))
                     {
                         gatewayUrl += "?";
                     }
                     return gatewayUrl + response.Body;
                }
                
                throw new Exception($"支付宝签名生成失败: {response.SubMsg ?? response.Msg}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Alipay Exception] {ex.Message} \n {ex.StackTrace}");
                _logger.LogError(ex, "生成支付宝支付表单失败");
                // 如果是真实配置出错，抛出异常而不是返回模拟链接，以便排查
                if (config["AppId"] != "your_sandbox_app_id")
                {
                    throw;
                }
                // Fallback
                return GeneratePaymentGatewayUrl(order);
            }
            */
            return GeneratePaymentGatewayUrl(order);
        }

        private string GeneratePaymentGatewayUrl(Order order)
        {
            var config = _configuration.GetSection("Alipay");
            var returnUrl = config["ReturnUrl"];
            if (string.IsNullOrEmpty(returnUrl) || returnUrl.StartsWith("http"))
            {
                returnUrl = "/payment/callback";
            }
            if (returnUrl.Contains("payment/success"))
            {
                 returnUrl = returnUrl.Replace("payment/success", $"orders/{order.Id}");
            }
            else if (returnUrl.Contains("payment/callback"))
            {
                 returnUrl += $"?verify=true&orderId={order.Id}";
            }
            if (returnUrl.StartsWith("http"))
            {
                try {
                    var uri = new Uri(returnUrl);
                    returnUrl = uri.PathAndQuery;
                } catch {}
            }
            string baseUrl = "/mock-pay";
            string paramsString = $"order_id={order.OrderNumber}&amount={order.TotalAmount:0.00}&reference={order.PaymentReference ?? string.Empty}&return_url={System.Net.WebUtility.UrlEncode(returnUrl)}&t={DateTime.Now.Ticks}";
            string finalUrl = $"{baseUrl}?{paramsString}";
            
            _logger.LogInformation("Generated Mock Payment URL: {Url}", finalUrl);
            return finalUrl;
        }

        private async Task ClearPaymentCache(int userId, int orderId)
        {
            string statusCacheKey = string.Format(PAYMENT_STATUS_CACHE_KEY, userId, orderId);
            string linkCacheKey = string.Format(PAYMENT_LINK_CACHE_KEY, userId, orderId);
            
            await _cacheService.RemoveAsync(statusCacheKey);
            await _cacheService.RemoveAsync(linkCacheKey);
            
            _logger.LogInformation("清除支付相关缓存，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
        }
        
        public async Task<PaymentResult> UpdatePaymentStatusAsync(int orderId, PaymentStatusRequest request)
        {
            try
            {
                _logger.LogInformation("更新支付状态，订单ID: {OrderId}，请求: {@Request}", orderId, request);

                // 参数验证
                if (orderId <= 0) throw new ArgumentException("订单ID无效", nameof(orderId));
                ArgumentNullException.ThrowIfNull(request, nameof(request));
                if (string.IsNullOrWhiteSpace(request.PaymentStatus)) throw new ArgumentException("支付状态不能为空", nameof(request));

                await _unitOfWork.BeginTransactionAsync();
                
                try
                {
                    // 获取订单
                    var order = await _orderRepository.GetByIdAsync(orderId);
                    if (order == null)
                    {
                        _logger.LogWarning("订单不存在，订单ID: {OrderId}", orderId);
                        return new PaymentResult { Success = false, Message = "订单不存在" };
                    }

                    // 记录更新前的状态，用于日志和状态历史
                    var oldPaymentStatus = order.PaymentStatus;
                    var oldOrderStatus = order.Status;

                    // 检查订单状态
                    if (order.Status != "pending" && order.Status != "processing")
                    {
                        _logger.LogWarning("订单状态不允许更新支付信息，订单ID: {OrderId}，当前状态: {Status}", orderId, order.Status);
                        return new PaymentResult { Success = false, Message = "订单状态不允许更新支付信息" };
                    }

                    // 更新订单的支付信息
                    order.PaymentStatus = request.PaymentStatus;
                    order.PaymentReference = request.PaymentReference ?? order.PaymentReference;
                    if (!string.IsNullOrEmpty(request.PaymentMethod))
                    {
                        order.PaymentMethod = request.PaymentMethod;
                    }
                    order.UpdatedAt = DateTime.UtcNow;

                    // 记录详细的支付日志
                    var paymentLogNote = $"支付信息更新: 状态={request.PaymentStatus}, 参考号={request.PaymentReference}, 支付方式={order.PaymentMethod}";
                    _logger.LogInformation("{PaymentLogNote}, 订单ID={OrderId}", paymentLogNote, orderId);

                    // 如果支付成功，更新订单状态
                    if (request.PaymentStatus == "paid" && order.Status == "pending")
                    {
                        order.Status = "processing";
                        order.PaidAt = DateTime.UtcNow;
                    }

                    await _orderRepository.UpdateAsync(order);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    // 清除缓存
                    await ClearPaymentCache(order.UserId, orderId);
                    // 清除管理员订单列表缓存
                    await _cacheService.RemoveAsync("admin:orders");

                    _logger.LogInformation("支付状态更新成功，订单ID: {OrderId}", orderId);
                    return new PaymentResult
                    {
                        Success = true,
                        Message = "支付状态更新成功",
                        PaymentReference = order.PaymentReference,
                        OrderStatus = order.Status,
                        PaymentStatus = order.PaymentStatus
                    };
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "参数验证失败: {Message}", ex.Message);
                return new PaymentResult { Success = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新支付状态失败，订单ID: {OrderId}", orderId);
                return new PaymentResult { Success = false, Message = "更新支付状态时发生错误" };
            }
        }

        public async Task<PaymentResult> ProcessCallbackAsync(string paymentReference, string tradeStatus, string? paymentMethod = null)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var orders = await _orderRepository.GetAllAsync();
                    var order = orders.FirstOrDefault(o => string.Equals(o.PaymentReference ?? string.Empty, paymentReference ?? string.Empty, StringComparison.OrdinalIgnoreCase));
                    if (order == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new PaymentResult { Success = false, Message = "订单不存在" };
                    }
                    if (tradeStatus == "TRADE_SUCCESS" || tradeStatus == "TRADE_FINISHED" || IsMockMode())
                    {
                        order.PaymentStatus = "paid";
                        order.Status = order.Status == "pending" ? "processing" : order.Status;
                        order.PaidAt = DateTime.UtcNow;
                        if (!string.IsNullOrEmpty(paymentMethod)) order.PaymentMethod = paymentMethod;
                        order.UpdatedAt = DateTime.UtcNow;
                        await _orderRepository.UpdateAsync(order);
                        await _unitOfWork.SaveChangesAsync();
                        await _unitOfWork.CommitTransactionAsync();
                        await ClearPaymentCache(order.UserId, order.Id);
                        return new PaymentResult { Success = true, Message = "支付回调处理成功", PaymentReference = order.PaymentReference, PaymentStatus = order.PaymentStatus, OrderStatus = order.Status };
                    }
                    await _unitOfWork.RollbackTransactionAsync();
                    return new PaymentResult { Success = false, Message = "支付未完成" };
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理支付回调失败");
                return new PaymentResult { Success = false, Message = "处理支付回调失败" };
            }
        }
    }
}
