using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using Microsoft.Extensions.Logging;

namespace HuanyuFlowerShop.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<PaymentService> _logger;

        // 缓存键常量
        private const string PAYMENT_STATUS_CACHE_KEY = "payment_status_{0}_{1}"; // userId_orderId
        
        // 缓存过期时间
        private readonly TimeSpan _paymentStatusCacheExpiry = TimeSpan.FromMinutes(15);

        public PaymentService(
            IRepository<Order> orderRepository,
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<PaymentService> logger)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _logger = logger;
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
                        // 货到付款在签收前不能视为已支付，订单仍由商家正常处理。
                        order.PaymentStatus = "unpaid";
                        order.Status = "pending";
                        order.PaymentReference = null;
                        _logger.LogInformation("货到付款方式已登记，订单ID: {OrderId}", orderId);
                    }
                    else if (paymentMethod == "online")
                    {
                        // 未配置真实支付网关时不能生成可被伪造的支付单。
                        await _unitOfWork.RollbackTransactionAsync();
                        return new PaymentResult { Success = false, Message = "在线支付暂未开通，请选择货到付款" };
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
                    if (order.PaymentReference != paymentReference)
                    {
                        _logger.LogWarning("支付参考号不匹配，订单ID: {OrderId}", orderId);
                        return new PaymentResult { Success = false, Message = "支付参考号不匹配" };
                    }

                    // 调用第三方支付网关验证（模拟）
                    // 在实际应用中，这里应该调用真实的支付网关API
                    bool isPaymentSuccessful = await VerifyPaymentWithGatewayAsync(order, paymentReference);

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

                // 该部署未接入真实支付网关，不返回伪造的第三方地址。
                throw new InvalidOperationException("在线支付暂未开通，请选择货到付款");

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
        private string GeneratePaymentReference()
        {
            return $"PAY{DateTime.UtcNow:yyyyMMddHHmmssfff}{Guid.NewGuid():N}";
        }

        private async Task<bool> VerifyPaymentWithGatewayAsync(Order order, string paymentReference)
        {
            // 模拟调用第三方支付网关验证支付
            // 在实际应用中，这里应该调用真实的支付网关API
            _logger.LogInformation("调用支付网关验证支付，订单ID: {OrderId}，支付参考号: {PaymentReference}", 
                order.Id, paymentReference);
            
            // 模拟验证延迟
            await Task.Delay(500);
            
            // 没有真实网关签名/回调时必须拒绝验证，禁止客户端伪造支付成功。
            return false;
        }

        private async Task CancelPaymentWithGatewayAsync(string paymentReference)
        {
            // 模拟调用第三方支付网关取消支付
            // 在实际应用中，这里应该调用真实的支付网关API
            _logger.LogInformation("调用支付网关取消支付，支付参考号: {PaymentReference}", paymentReference);
            await Task.Delay(300);
        }

        private async Task ClearPaymentCache(int userId, int orderId)
        {
            string statusCacheKey = string.Format(PAYMENT_STATUS_CACHE_KEY, userId, orderId);
            await _cacheService.RemoveAsync(statusCacheKey);
            
            _logger.LogInformation("清除支付相关缓存，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
        }
        
        public async Task<PaymentResult> UpdatePaymentStatusAsync(int orderId, PaymentStatusRequest request)
        {
            try
            {
                _logger.LogInformation("更新支付状态，订单ID: {OrderId}，请求: {@Request}", orderId, request);

                // 参数验证
                if (orderId <= 0) throw new ArgumentException("订单ID无效", nameof(orderId));
                if (request == null) throw new ArgumentNullException(nameof(request));
                if (string.IsNullOrWhiteSpace(request.PaymentStatus)) throw new ArgumentException("支付状态不能为空", nameof(request.PaymentStatus));
                request.PaymentStatus = request.PaymentStatus.Trim().ToLowerInvariant();
                if (request.PaymentStatus == "paid" && string.IsNullOrWhiteSpace(request.PaymentReference))
                    throw new ArgumentException("支付成功必须提供支付凭证号", nameof(request.PaymentReference));

                await _unitOfWork.BeginTransactionAsync();
                
                try
                {
                    // 获取订单
                    var order = await _orderRepository.GetByIdAsync(orderId);
                    if (order == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogWarning("订单不存在，订单ID: {OrderId}", orderId);
                        return new PaymentResult { Success = false, Message = "订单不存在" };
                    }

                    if (request.Amount.HasValue && Math.Abs(request.Amount.Value - order.TotalAmount) > 0.01m)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new PaymentResult { Success = false, Message = "支付金额与订单金额不一致" };
                    }

                    if (order.PaymentStatus == "paid")
                    {
                        var sameReference = string.Equals(order.PaymentReference, request.PaymentReference, StringComparison.Ordinal);
                        await _unitOfWork.RollbackTransactionAsync();
                        return sameReference
                            ? new PaymentResult { Success = true, Message = "支付回调已处理", PaymentReference = order.PaymentReference, OrderStatus = order.Status, PaymentStatus = order.PaymentStatus }
                            : new PaymentResult { Success = false, Message = "订单已使用其他支付凭证完成支付" };
                    }

                    // 记录更新前的状态，用于日志和状态历史
                    var oldPaymentStatus = order.PaymentStatus;
                    var oldOrderStatus = order.Status;

                    // 检查订单状态
                    if (order.Status != "pending" && order.Status != "processing")
                    {
                        await _unitOfWork.RollbackTransactionAsync();
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
    }
}
