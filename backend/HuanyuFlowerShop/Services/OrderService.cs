using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.DTOs;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace HuanyuFlowerShop.Services
{
    public partial class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<CartItem> _cartRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<OrderStatusHistory> _orderStatusHistoryRepository;
        private readonly IRepository<Coupon> _couponRepository;
        private readonly IRepository<UserCoupon> _userCouponRepository;
        private readonly ICacheService _cacheService;
        private readonly ILogger<OrderService> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        private const string USER_ORDERS_CACHE_KEY_PREFIX = "user_orders_";
        private const string ORDER_CACHE_KEY_PREFIX = "order_";
        private const string ORDER_HISTORY_CACHE_KEY_PREFIX = "order_history_";
        private const string USER_ORDER_STATS_CACHE_KEY_PREFIX = "user_order_stats_";

        public OrderService(
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<CartItem> cartRepository,
            IRepository<Product> productRepository,
            IRepository<OrderStatusHistory> orderStatusHistoryRepository,
            IRepository<Coupon> couponRepository,
            IRepository<UserCoupon> userCouponRepository,
            ICacheService cacheService,
            ILogger<OrderService> logger,
            IPaymentService paymentService,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _orderStatusHistoryRepository = orderStatusHistoryRepository;
            _couponRepository = couponRepository;
            _userCouponRepository = userCouponRepository;
            _cacheService = cacheService;
            _logger = logger;
            _paymentService = paymentService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        [GeneratedRegex("(物流公司|快递公司)[：:]\\s*([^；,，\\n\\r]+)", RegexOptions.Compiled)]
        private static partial Regex CompanyRegex();

        [GeneratedRegex("(运单号|单号)[：:]\\s*([A-Za-z0-9-]+)", RegexOptions.Compiled)]
        private static partial Regex TrackingRegex();

        public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("用户ID必须大于0", nameof(userId));
            }

            _logger.LogInformation("开始获取用户订单列表，用户ID: {UserId}", userId);
            
            // 尝试从缓存获取
            string cacheKey = $"{USER_ORDERS_CACHE_KEY_PREFIX}{userId}";
            var cachedOrders = await _cacheService.GetAsync<IEnumerable<OrderDto>>(cacheKey);
            if (cachedOrders != null)
            {
                _logger.LogInformation("从缓存获取用户订单列表成功，用户ID: {UserId}", userId);
                return cachedOrders;
            }

            _logger.LogInformation("缓存未命中，从数据库获取用户订单列表，用户ID: {UserId}", userId);
            var orders = await _orderRepository.GetAllAsync();
            var userOrders = orders.Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt);

            var result = new List<OrderDto>();

            foreach (var order in userOrders)
            {
                var orderItems = await _orderItemRepository.GetAllAsync();
                var orderItemsForOrder = orderItems.Where(oi => oi.OrderId == order.Id);

                var orderItemDtos = new List<OrderItemDto>();

                foreach (var orderItem in orderItemsForOrder)
                {
                    var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                    if (product != null)
                    {
                        orderItemDtos.Add(new OrderItemDto
                        {
                            Id = orderItem.Id,
                            ProductId = orderItem.ProductId,
                            ProductName = product.Name,
                        ProductImage = product.ImageUrl ?? "",
                        Price = orderItem.UnitPrice,
                        Quantity = orderItem.Quantity,
                        TotalPrice = orderItem.Subtotal
                        });
                    }
                }

                // 查找物流相关的状态历史记录（假设物流信息存储在状态历史的备注中）
                var shippingHistory = await _orderStatusHistoryRepository.GetAllAsync();
                var shippingInfo = shippingHistory
                    .Where(h => h.OrderId == order.Id && h.NewStatus == "shipped")
                    .OrderByDescending(h => h.CreatedAt)
                    .FirstOrDefault();
                
                ShippingInfoDto? shippingInfoDto = null;
                if (shippingInfo != null && !string.IsNullOrEmpty(shippingInfo.Note))
                {
                    var note = shippingInfo.Note;
                    var companyMatch = CompanyRegex().Match(note);
                    var trackingMatch = TrackingRegex().Match(note);
                    if (companyMatch.Success && trackingMatch.Success)
                    {
                        shippingInfoDto = new ShippingInfoDto
                        {
                            Company = companyMatch.Groups[2].Value.Trim(),
                            TrackingNumber = trackingMatch.Groups[2].Value.Trim(),
                            Status = "shipped"
                        };
                    }
                }
                
                result.Add(new OrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber ?? "",
                    Status = order.Status ?? "unknown",
                    TotalAmount = order.TotalAmount,
                    ShippingAddress = order.DeliveryAddress ?? "",
                    Phone = order.RecipientPhone ?? "",
                    RecipientName = order.IsAnonymous ? "匿名收货人" : order.RecipientName,
                    DeliveryDate = order.DeliveryDate,
                    ShippingMethod = order.ShippingMethod,
                    DeliveryTime = order.DeliveryTime,
                    SenderName = order.IsAnonymous ? null : order.SenderName,
                    CardMessage = order.IsAnonymous ? null : order.CardMessage,
                    IsAnonymous = order.IsAnonymous,
                    SubstitutionPreference = order.SubstitutionPreference,
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt,
                    OrderItems = orderItemDtos,
                    // 支付相关字段
                    PaymentStatus = order.PaymentStatus ?? "unpaid",
                    PaymentMethod = order.PaymentMethod ?? "",
                    PaymentReference = order.PaymentReference ?? "",
                    PaidAmount = order.PaidAmount,
                    PaidAt = order.PaidAt,
                    RefundedAmount = order.RefundedAmount,
                    PaymentExpiresAt = order.PaymentExpiresAt,
                    // 物流信息
                    ShippingInfo = shippingInfoDto
                });
            }

            // 缓存结果，有效期30分钟
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30));
            _logger.LogInformation("获取用户订单列表完成，用户ID: {UserId}，订单数量: {Count}", userId, result.Count);
            
            return result;
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId)
        {
            if (userId <= 0 || orderId <= 0)
            {
                throw new ArgumentException("用户ID和订单ID必须大于0");
            }

            _logger.LogInformation("开始获取订单详情，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
            
            // 尝试从缓存获取
            string cacheKey = $"{ORDER_CACHE_KEY_PREFIX}{orderId}_{userId}";
            var cachedOrder = await _cacheService.GetAsync<OrderDto>(cacheKey);
            if (cachedOrder != null)
            {
                _logger.LogInformation("从缓存获取订单详情成功，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                return cachedOrder;
            }

            _logger.LogInformation("缓存未命中，从数据库获取订单详情，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                _logger.LogWarning("订单不存在或无权访问，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                return null;
            }

            var orderItems = await _orderItemRepository.GetAllAsync();
            var orderItemsForOrder = orderItems.Where(oi => oi.OrderId == order.Id);

            var orderItemDtos = new List<OrderItemDto>();

            foreach (var orderItem in orderItemsForOrder)
            {
                var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                if (product != null)
                {
                    orderItemDtos.Add(new OrderItemDto
                    {
                        Id = orderItem.Id,
                        ProductId = orderItem.ProductId,
                        ProductName = product.Name,
                        ProductImage = product.ImageUrl ?? "",
                        Price = orderItem.UnitPrice,
                        Quantity = orderItem.Quantity,
                        TotalPrice = orderItem.Subtotal
                    });
                }
            }

            // 查找物流相关的状态历史记录（假设物流信息存储在状态历史的备注中）
            var shippingHistory = await _orderStatusHistoryRepository.GetAllAsync();
            var shippingInfo = shippingHistory
                .Where(h => h.OrderId == order.Id && h.NewStatus == "shipped")
                .OrderByDescending(h => h.CreatedAt)
                .FirstOrDefault();
            
            ShippingInfoDto? shippingInfoDto = null;
            if (shippingInfo != null && !string.IsNullOrEmpty(shippingInfo.Note))
            {
                var note = shippingInfo.Note;
                var companyMatch = CompanyRegex().Match(note);
                var trackingMatch = TrackingRegex().Match(note);
                if (companyMatch.Success && trackingMatch.Success)
                {
                    shippingInfoDto = new ShippingInfoDto
                    {
                        Company = companyMatch.Groups[2].Value.Trim(),
                        TrackingNumber = trackingMatch.Groups[2].Value.Trim(),
                        Status = "shipped"
                    };
                }
            }
            
            var result = new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber ?? "",
                Status = order.Status ?? "unknown",
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.DeliveryAddress ?? "",
                Phone = order.RecipientPhone ?? "",
                RecipientName = order.IsAnonymous ? "匿名收货人" : order.RecipientName,
                DeliveryDate = order.DeliveryDate,
                ShippingMethod = order.ShippingMethod,
                DeliveryTime = order.DeliveryTime,
                SenderName = order.IsAnonymous ? null : order.SenderName,
                CardMessage = order.IsAnonymous ? null : order.CardMessage,
                IsAnonymous = order.IsAnonymous,
                SubstitutionPreference = order.SubstitutionPreference,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                OrderItems = orderItemDtos,
                // 支付相关字段
                PaymentStatus = order.PaymentStatus ?? "unpaid",
                PaymentMethod = order.PaymentMethod ?? "",
                PaymentReference = order.PaymentReference ?? "",
                PaidAmount = order.PaidAmount,
                PaidAt = order.PaidAt,
                RefundedAmount = order.RefundedAmount,
                PaymentExpiresAt = order.PaymentExpiresAt,
                // 物流信息
                ShippingInfo = shippingInfoDto
            };

            // 缓存结果，有效期15分钟
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15));
            _logger.LogInformation("获取订单详情完成，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
            
            return result;
        }

        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto createOrderDto)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await CreateOrderCoreAsync(userId, createOrderDto);
                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task<OrderDto> CreateOrderCoreAsync(int userId, CreateOrderDto createOrderDto)
        {
            if (userId <= 0) throw new ArgumentException("用户ID必须大于0", nameof(userId));
            if (createOrderDto == null) throw new ArgumentNullException(nameof(createOrderDto));

            // 订单字段由服务端统一校验，避免空地址/伪造支付方式进入订单。
            if (string.IsNullOrWhiteSpace(createOrderDto.RecipientName) || createOrderDto.RecipientName.Length > 50)
                throw new ArgumentException("收货人姓名无效");
            if (string.IsNullOrWhiteSpace(createOrderDto.ShippingAddress) || createOrderDto.ShippingAddress.Length > 255)
                throw new ArgumentException("收货地址无效");
            if (string.IsNullOrWhiteSpace(createOrderDto.Phone) || createOrderDto.Phone.Length > 20)
                throw new ArgumentException("收货电话无效");
            var deliveryDate = createOrderDto.DeliveryDate?.Date ?? DateTime.UtcNow.Date;
            if (deliveryDate < DateTime.UtcNow.Date || deliveryDate > DateTime.UtcNow.Date.AddDays(30))
                throw new ArgumentException("配送日期必须是今天至未来30天内");
            var shippingMethod = (createOrderDto.ShippingMethod ?? "standard").Trim().ToLowerInvariant();
            if (shippingMethod is not ("standard" or "same_day")) throw new ArgumentException("不支持的配送方式");
            if (shippingMethod == "same_day" && DateTime.Now.Hour >= _configuration.GetValue("Shipping:SameDayCutoffHour", 15) && deliveryDate == DateTime.UtcNow.Date)
                throw new ArgumentException("已超过当日配送截止时间，请选择其他日期");
            var substitution = (createOrderDto.SubstitutionPreference ?? "contact_me").Trim().ToLowerInvariant();
            if (substitution is not ("contact_me" or "allow_similar" or "no_substitution")) throw new ArgumentException("不支持的替换偏好");
            var requestedPaymentMethod = (createOrderDto.PaymentMethod ?? "cod").Trim().ToLowerInvariant() switch
            {
                "alipay" or "wechat_pay" or "wechat" or "online" => "online",
                "cash_on_delivery" or "cash" or "cod" => "cod",
                _ => string.Empty
            };
            if (requestedPaymentMethod is not ("cod" or "online"))
                throw new ArgumentException("暂不支持该支付方式");

            // 获取购物车项目
            var cartItems = await _cartRepository.GetAllAsync();
            var userCartItems = cartItems.Where(ci => ci.UserId == userId).ToList();

            if (!userCartItems.Any())
            {
                throw new ArgumentException("购物车为空");
            }

            // 计算总金额并检查库存
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var cartItem in userCartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product == null)
                {
                    throw new ArgumentException($"产品 {cartItem.ProductId} 不存在");
                }

                if (cartItem.Quantity <= 0 || cartItem.Quantity > 99)
                    throw new ArgumentException("商品数量必须在1到99之间");
                if (!product.IsActive)
                    throw new ArgumentException($"产品 {product.Name} 已下架");
                if (product.Stock < cartItem.Quantity)
                {
                    throw new ArgumentException($"产品 {product.Name} 库存不足");
                }

                totalAmount += product.Price * cartItem.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    ProductName = product.Name,
                    ProductImage = product.ImageUrl ?? string.Empty,
                    UnitPrice = product.Price,
                    Quantity = cartItem.Quantity,
                    Subtotal = product.Price * cartItem.Quantity
                });
            }

            var shippingFee = totalAmount >= _configuration.GetValue("Shipping:FreeThreshold", 299m)
                ? 0m
                : _configuration.GetValue("Shipping:BaseFee", 15m);
            if (shippingMethod == "same_day") shippingFee += _configuration.GetValue("Shipping:SameDayFee", 20m);

            // 创建订单
            var order = new Order
            {
                UserId = userId,
                OrderNumber = GenerateOrderNumber(),
                Status = "pending",
                PaymentStatus = "unpaid",
                PaymentMethod = requestedPaymentMethod,
                PaymentExpiresAt = DateTime.UtcNow.AddMinutes(30), // 支付超时时间30分钟
                TotalAmount = totalAmount + shippingFee,
                Subtotal = totalAmount,
                ShippingFee = shippingFee,
                DiscountAmount = 0,
                CouponCode = string.IsNullOrWhiteSpace(createOrderDto.CouponCode) ? null : createOrderDto.CouponCode?.Trim(),
                RecipientName = createOrderDto.RecipientName ?? "用户",
                RecipientPhone = createOrderDto.Phone ?? "",
                DeliveryAddress = createOrderDto.ShippingAddress ?? "",
                DeliveryDate = deliveryDate,
                ShippingMethod = shippingMethod,
                DeliveryTime = string.IsNullOrWhiteSpace(createOrderDto.DeliveryTime) ? null : createOrderDto.DeliveryTime.Trim(),
                SenderName = string.IsNullOrWhiteSpace(createOrderDto.SenderName) ? null : createOrderDto.SenderName.Trim(),
                CardMessage = string.IsNullOrWhiteSpace(createOrderDto.CardMessage) ? null : createOrderDto.CardMessage.Trim(),
                IsAnonymous = createOrderDto.IsAnonymous,
                SubstitutionPreference = substitution,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            _logger.LogInformation("创建订单，用户ID: {UserId}，支付方式: {PaymentMethod}，支付超时时间: {PaymentExpiresAt}", 
                userId, createOrderDto.PaymentMethod, order.PaymentExpiresAt);

            // 在保存订单前处理优惠券
            if (!string.IsNullOrWhiteSpace(order.CouponCode))
            {
                try
                {
                    var coupons = await _couponRepository.GetAllAsync();
                    var coupon = coupons.FirstOrDefault(c => string.Equals(c.Code, order.CouponCode, StringComparison.OrdinalIgnoreCase) && c.Status == "active" &&
                        (!c.StartAt.HasValue || c.StartAt <= DateTime.UtcNow) && (!c.EndAt.HasValue || c.EndAt >= DateTime.UtcNow));
                    var userCoupons = await _userCouponRepository.GetAllAsync();
                    var userCoupon = coupon == null ? null : userCoupons.FirstOrDefault(x => x.UserId == userId && x.CouponId == coupon.Id);
                    var alreadyUsed = userCoupon?.Status == "used";
                    var exhausted = coupon?.UsageLimit is > 0 && coupon.UsedCount >= coupon.UsageLimit;
                    if (coupon != null && !alreadyUsed && !exhausted && totalAmount >= coupon.MinOrderAmount)
                    {
                        decimal discount = 0;
                if (string.Equals(coupon.DiscountType ?? "amount", "percent", StringComparison.OrdinalIgnoreCase))
                        {
                            discount = Math.Round(totalAmount * (coupon.Value / 100m), 2);
                            if (coupon.MaxDiscount.HasValue) discount = Math.Min(discount, coupon.MaxDiscount.Value);
                        }
                        else
                        {
                            discount = coupon.Value;
                        }
                        if (discount > 0)
                        {
                            order.DiscountAmount = discount;
                            order.TotalAmount = Math.Max(0, order.Subtotal + order.ShippingFee - discount);
                            // 标记用户优惠券使用
                            var uc = userCoupon;
                            if (uc == null)
                            {
                                await _userCouponRepository.AddAsync(new UserCoupon { UserId = userId, CouponId = coupon.Id, ClaimedAt = DateTime.UtcNow, UsedAt = DateTime.UtcNow, Status = "used" });
                            }
                            else if (uc.Status != "used")
                            {
                                uc.UsedAt = DateTime.UtcNow;
                                uc.Status = "used";
                                await _userCouponRepository.UpdateAsync(uc);
                            }
                            // 增加优惠券使用计数
                            coupon.UsedCount += 1;
                            await _couponRepository.UpdateAsync(coupon);
                        }
                    }
                }
                catch (ArgumentException) { throw; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "应用优惠券失败，订单创建已回滚");
                    throw new InvalidOperationException("优惠券校验失败，请稍后重试", ex);
                }
            }

            var createdOrder = await _orderRepository.AddAsync(order);

            if (createdOrder == null)
            {
                throw new InvalidOperationException("无法创建订单");
            }
            
            await _orderStatusHistoryRepository.AddAsync(new OrderStatusHistory
            {
                OrderId = createdOrder.Id,
                OldStatus = null,
                NewStatus = "pending",
                OperatorId = userId,
                OperatorName = "用户",
                Note = "订单创建",
                CreatedAt = DateTime.UtcNow
            });

            // 创建订单项
            foreach (var orderItem in orderItems)
            {
                orderItem.OrderId = createdOrder.Id;
                await _orderItemRepository.AddAsync(orderItem);

                // 更新产品库存
                var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                if (product != null)
                {
                    product.Stock -= orderItem.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }

            // 清空购物车
            foreach (var cartItem in userCartItems)
            {
                await _cartRepository.DeleteAsync(cartItem.Id);
            }

            // 清除相关缓存
            await ClearUserOrderCache(userId);
            
            // 返回创建的订单详情
            var createdOrderDto = await GetOrderByIdAsync(userId, createdOrder.Id);
            if (createdOrderDto == null)
            {
                throw new InvalidOperationException("无法获取创建的订单详情");
            }
            
            _logger.LogInformation("订单创建成功，订单ID: {OrderId}", createdOrder.Id);
            return createdOrderDto;
        }

        public async Task<OrderDto?> UpdateOrderStatusAsync(int userId, int orderId, string status)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                return null;
            }

            status = status?.Trim().ToLowerInvariant() ?? string.Empty;
            var validStatuses = new[] { "pending", "processing", "shipped", "delivered", "cancelled" };
            if (!validStatuses.Contains(status))
            {
                throw new ArgumentException("无效的订单状态");
            }

            // 仅允许单向状态流转，避免管理员误操作回退已完成订单。
            var allowedTransitions = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["pending"] = new[] { "processing", "cancelled" },
                ["processing"] = new[] { "shipped", "cancelled" },
                ["shipped"] = new[] { "delivered" },
                ["delivered"] = Array.Empty<string>(),
                ["cancelled"] = Array.Empty<string>()
            };
            if (!string.Equals(order.Status, status, StringComparison.OrdinalIgnoreCase) &&
                (!allowedTransitions.TryGetValue(order.Status ?? string.Empty, out var next) || !next.Contains(status)))
                throw new InvalidOperationException($"订单不能从{order.Status}变更为{status}");

            // 记录旧状态
            string oldStatus = order.Status ?? "unknown";
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            
            // 创建订单状态历史记录
            var history = new OrderStatusHistory
            {
                OrderId = orderId,
                OldStatus = oldStatus,
                NewStatus = status,
                OperatorId = userId,
                OperatorName = "用户",
                Note = $"订单状态从{oldStatus}更新为{status}",
                CreatedAt = DateTime.UtcNow
            };
            await _orderStatusHistoryRepository.CreateAsync(history);

            // 清除相关缓存
            await ClearOrderCache(userId, orderId);
            await ClearUserOrderCache(userId);
            await ClearUserOrderStatsCache(userId);
            
            _logger.LogInformation("订单状态更新成功，订单ID: {OrderId}，状态: {Status}", orderId, status);
            return await GetOrderByIdAsync(userId, orderId);
        }

        public async Task<bool> CancelOrderAsync(int userId, int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                return false;
            }

            // 只有待处理和已确认的订单可以取消
            if (order.Status != "pending" && order.Status != "processing")
            {
                return false;
            }

            // 恢复产品库存
            var orderItems = await _orderItemRepository.GetAllAsync();
            var orderItemsForOrder = orderItems.Where(oi => oi.OrderId == order.Id);

            foreach (var orderItem in orderItemsForOrder)
            {
                var product = await _productRepository.GetByIdAsync(orderItem.ProductId);
                if (product != null)
                {
                    product.Stock += orderItem.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }

            // 更新订单状态
            order.Status = "cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            // 清除相关缓存
            await ClearOrderCache(userId, orderId);
            await ClearUserOrderCache(userId);
            await ClearUserOrderStatsCache(userId);
            
            _logger.LogInformation("订单取消成功，订单ID: {OrderId}", orderId);
            return true;
        }

        // 支付相关方法实现
        public async Task<OrderDto?> UpdatePaymentStatusAsync(int userId, int orderId, string paymentStatus, string? paymentReference = null, string? paymentMethod = null)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null || order.UserId != userId)
                {
                    _logger.LogWarning("订单不存在或不属于当前用户，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                    return null;
                }

                // 验证支付状态
                var validPaymentStatuses = new[] { "unpaid", "paid", "refunded", "partial_refunded" };
                if (!validPaymentStatuses.Contains(paymentStatus))
                {
                    throw new ArgumentException("无效的支付状态");
                }

                // 记录旧状态
                string oldPaymentStatus = order.PaymentStatus;
                string oldOrderStatus = order.Status;
                
                // 更新支付状态和其他支付相关信息
                order.PaymentStatus = paymentStatus;
                order.UpdatedAt = DateTime.UtcNow;
                
                // 如果提供了支付方式，更新支付方式
                if (!string.IsNullOrEmpty(paymentMethod))
                {
                    order.PaymentMethod = paymentMethod;
                }
                
                // 根据支付状态更新相应字段
                if (paymentStatus == "paid")
                {
                    // 如果是支付成功
                    order.PaidAt = DateTime.UtcNow;
                    order.PaidAmount = order.TotalAmount;
                    if (!string.IsNullOrEmpty(paymentReference))
                    {
                        order.PaymentReference = paymentReference;
                    }
                    
                    if (string.IsNullOrWhiteSpace(order.PaymentReference))
                    {
                        throw new InvalidOperationException("支付成功必须提供真实支付凭证号");
                    }
                    
                    // 详细的支付日志信息
                    _logger.LogInformation("支付成功: 订单号={OrderNumber}, 金额={TotalAmount}, 支付方式={PaymentMethod}, 参考号={PaymentReference}",
                        order.OrderNumber,
                        order.TotalAmount,
                        order.PaymentMethod,
                        order.PaymentReference);
                    
                    // 如果支付成功，同时更新订单状态为处理中
                    if (order.Status == "pending")
                    {
                        order.Status = "processing";
                        
                        // 创建订单状态历史记录 - 订单状态变更
                        var statusHistory = new OrderStatusHistory
                        {
                            OrderId = orderId,
                            OldStatus = "pending",
                            NewStatus = "processing",
                            OperatorId = userId,
                            OperatorName = "系统",
                            Note = $"订单支付成功，自动更新为处理中。支付成功: 订单号={order.OrderNumber}, 金额={order.TotalAmount}, 支付方式={order.PaymentMethod}, 参考号={order.PaymentReference}",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _orderStatusHistoryRepository.CreateAsync(statusHistory);
                    }
                    
                    // 创建支付状态历史记录 - 支付状态变更
                    if (oldPaymentStatus != "paid")
                    {
                        var paymentStatusHistory = new OrderStatusHistory
                        {
                            OrderId = orderId,
                            OldStatus = oldPaymentStatus,
                            NewStatus = "paid",
                            OperatorId = userId,
                            OperatorName = "系统",
                            Note = $"支付成功: 订单号={order.OrderNumber}, 金额={order.TotalAmount}, 支付方式={order.PaymentMethod}, 参考号={order.PaymentReference}",
                            CreatedAt = DateTime.UtcNow
                        };
                        await _orderStatusHistoryRepository.CreateAsync(paymentStatusHistory);
                    }
                }
                else if (paymentStatus == "refunded" || paymentStatus == "partial_refunded")
                {
                    // 退款状态必须同步退款金额，避免订单统计继续把全额计为收入。
                    order.RefundedAmount = paymentStatus == "refunded"
                        ? order.PaidAmount
                        : Math.Min(order.PaidAmount, Math.Max(0m, order.RefundedAmount));
                    if (oldPaymentStatus != paymentStatus)
                    {
                        _logger.LogInformation("支付状态变更: 从 {OldPaymentStatus} 变更为 {NewPaymentStatus}, 参考号={PaymentReference}",
                            oldPaymentStatus, paymentStatus, paymentReference ?? "未提供");
                    }
                }
                else if (oldPaymentStatus != paymentStatus)
                {
                    // 记录其他支付状态变更
                    _logger.LogInformation("支付状态变更: 从 {OldPaymentStatus} 变更为 {NewPaymentStatus}, 参考号={PaymentReference}",
                        oldPaymentStatus,
                        paymentStatus,
                        paymentReference ?? "未提供");
                    
                    var paymentStatusHistory = new OrderStatusHistory
                    {
                        OrderId = orderId,
                        OldStatus = oldPaymentStatus,
                        NewStatus = paymentStatus,
                        OperatorId = userId,
                        OperatorName = "系统",
                        Note = $"支付状态变更: 从 {oldPaymentStatus} 变更为 {paymentStatus}, 参考号={paymentReference ?? "未提供"}",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _orderStatusHistoryRepository.CreateAsync(paymentStatusHistory);
                }
                await _orderRepository.UpdateAsync(order);
                
                // 创建支付状态变更的历史记录
                  var finalPaymentStatusHistory = new OrderStatusHistory
                  {
                      OrderId = orderId,
                      OldStatus = order.Status ?? "unknown",
                      NewStatus = order.Status ?? "unknown", // 状态可能没变，但支付状态变了
                      OperatorId = userId,
                      OperatorName = "系统",
                      Note = $"支付状态从{(oldPaymentStatus ?? "unknown")}更新为{paymentStatus}",
                      CreatedAt = DateTime.UtcNow
                  };
                await _orderStatusHistoryRepository.CreateAsync(finalPaymentStatusHistory);
                
                // 清除相关缓存
                await ClearOrderCache(userId, orderId);
                await ClearUserOrderCache(userId);
                await ClearUserOrderStatsCache(userId);
                
                _logger.LogInformation("订单支付状态更新成功，订单ID: {OrderId}，支付状态: {PaymentStatus}", orderId, paymentStatus);
                return await GetOrderByIdAsync(userId, orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新订单支付状态失败，订单ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<OrderDto?> ProcessPaymentAsync(int userId, int orderId, string paymentMethod)
        {
            try
            {
                // 获取订单
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null || order.UserId != userId)
                {
                    _logger.LogWarning("订单不存在或不属于当前用户，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                    return null;
                }

                // 验证订单状态
                if (order.Status != "pending")
                {
                    _logger.LogWarning("订单状态不允许支付，订单ID: {OrderId}，当前状态: {OrderStatus}", orderId, order.Status);
                    throw new InvalidOperationException("只有待处理的订单可以进行支付");
                }

                // 验证支付状态
                if (order.PaymentStatus == "paid")
                {
                    _logger.LogWarning("订单已经支付，无需重复支付，订单ID: {OrderId}", orderId);
                    throw new InvalidOperationException("订单已经支付，无需重复支付");
                }

                // 验证支付方式
                paymentMethod = paymentMethod?.Trim().ToLowerInvariant() ?? string.Empty;
                var validPaymentMethods = new[] { "cod", "online" };
                if (!validPaymentMethods.Contains(paymentMethod))
                {
                    throw new ArgumentException("无效的支付方式");
                }

                _logger.LogInformation("处理订单支付请求，订单ID: {OrderId}，支付方式: {PaymentMethod}", orderId, paymentMethod);

                // 设置支付方式和过期时间
                order.PaymentMethod = paymentMethod;
                order.PaymentExpiresAt = DateTime.UtcNow.AddMinutes(30); // 设置30分钟支付超时
                await _orderRepository.UpdateAsync(order);

                // 调用支付服务进行支付处理
                // 这里应该集成实际的支付API，现在只是模拟
                var paymentResult = await _paymentService.CreatePaymentAsync(
                    userId, // 用户ID
                    orderId, // 订单ID
                    paymentMethod // 支付方式
                );

                // 支付处理失败
                if (!paymentResult.Success)
                {
                    _logger.LogError("支付处理失败，订单ID: {OrderId}，错误信息: {ErrorMessage}", orderId, paymentResult.ErrorMessage);
                    throw new InvalidOperationException(paymentResult.ErrorMessage);
                }

                // 货到付款仅登记支付方式，签收前保持未支付；在线支付必须等待受信任网关回调。
                order.PaymentStatus = "unpaid";
                order.PaymentReference = paymentResult.PaymentReference;
                order.PaidAmount = 0;
                order.PaidAt = null;
                order.Status = "pending";
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(order);
                
                // 创建支付成功记录
                 var statusHistory = new OrderStatusHistory
                 {
                     OrderId = orderId,
                     OldStatus = "pending",
                     NewStatus = "pending",
                     OperatorId = userId,
                     OperatorName = "系统",
                     Note = "已登记货到付款，待收货时完成支付",
                     CreatedAt = DateTime.UtcNow
                 };
                await _orderStatusHistoryRepository.CreateAsync(statusHistory);

                // 清除缓存
                await ClearOrderCache(userId, orderId);
                await ClearUserOrderCache(userId);
                await ClearUserOrderStatsCache(userId);

                _logger.LogInformation("订单支付成功，订单ID: {OrderId}，支付方式: {PaymentMethod}，支付参考号: {PaymentReference}", 
                    orderId, paymentMethod, paymentResult.PaymentReference);

                // 返回更新后的订单信息
                return await GetOrderByIdAsync(userId, orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理订单支付失败，订单ID: {OrderId}", orderId);
                throw;
            }
        }

        public async Task<bool> VerifyPaymentAsync(int userId, int orderId, string paymentReference)
        {
            try
            {
                // 获取订单
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null || order.UserId != userId)
                {
                    _logger.LogWarning("订单不存在或不属于当前用户，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                    return false;
                }

                // 验证订单状态
                if (order.Status != "pending")
                {
                    _logger.LogWarning("订单状态不允许验证支付，订单ID: {OrderId}，当前状态: {OrderStatus}", orderId, order.Status);
                    throw new InvalidOperationException("只有待处理的订单可以验证支付");
                }

                // 检查支付是否超时
                if (order.PaymentExpiresAt.HasValue && DateTime.UtcNow > order.PaymentExpiresAt.Value)
                {
                    _logger.LogWarning("订单支付已超时，订单ID: {OrderId}，过期时间: {ExpiresAt}", orderId, order.PaymentExpiresAt.Value);
                    throw new InvalidOperationException("订单支付已超时，请重新下单");
                }

                _logger.LogInformation("验证订单支付请求，订单ID: {OrderId}，支付参考号: {PaymentReference}", orderId, paymentReference);

                // 调用支付服务验证支付
                var verificationResult = await _paymentService.VerifyPaymentAsync(
                    userId, // 用户ID
                    orderId, // 订单ID
                    paymentReference // 支付参考号
                );

                if (verificationResult.Success)
                {
                    // 支付验证成功，更新订单状态
                    order.PaymentStatus = "paid";
                    order.PaymentReference = paymentReference;
                    order.PaidAmount = order.TotalAmount;
                    order.PaidAt = DateTime.UtcNow;
                    order.Status = "processing";
                    order.UpdatedAt = DateTime.UtcNow;

                    await _orderRepository.UpdateAsync(order);
                    
                    // 创建状态变更记录
                    var statusHistory = new OrderStatusHistory
                    {
                        OrderId = orderId,
                        OldStatus = "pending",
                        NewStatus = "processing",
                        OperatorId = userId,
                        OperatorName = "系统",
                        Note = "支付验证成功，订单状态更新为处理中",
                        CreatedAt = DateTime.UtcNow
                    };
                    await _orderStatusHistoryRepository.CreateAsync(statusHistory);

                    // 清除缓存
                    await ClearOrderCache(userId, orderId);
                    await ClearUserOrderCache(userId);
                    await ClearUserOrderStatsCache(userId);

                    _logger.LogInformation("订单支付验证成功，订单ID: {OrderId}，支付方式: {PaymentMethod}，支付参考号: {PaymentReference}", 
                        orderId, order.PaymentMethod, paymentReference);
                    return true;
                }
                else
                {
                    _logger.LogWarning("订单支付验证失败，订单ID: {OrderId}，支付参考号: {PaymentReference}，原因: {Reason}", 
                        orderId, paymentReference, verificationResult.ErrorMessage);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证订单支付失败，订单ID: {OrderId}，支付参考号: {PaymentReference}", orderId, paymentReference);
                throw;
            }
        }

        private static string GenerateOrderNumber()
        {
            // Guid 后缀避免高并发下 Random/时间戳产生重复订单号。
            return $"ORD{DateTime.UtcNow:yyyyMMddHHmmssfff}{Guid.NewGuid():N}"[..50];
        }

        // 这个方法已经被PaymentService中的同名方法取代，但保留用于兼容性

        private async Task ClearOrderCache(int userId, int orderId)
        {
            string orderCacheKey = $"{ORDER_CACHE_KEY_PREFIX}{orderId}_{userId}";
            string historyCacheKey = $"{ORDER_HISTORY_CACHE_KEY_PREFIX}{userId}_{orderId}";
            
            await _cacheService.RemoveAsync(orderCacheKey);
            await _cacheService.RemoveAsync(historyCacheKey);
            
            _logger.LogDebug("清除订单相关缓存，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
        }

        private async Task ClearUserOrderCache(int userId)
        {
            string userOrdersCacheKey = $"{USER_ORDERS_CACHE_KEY_PREFIX}{userId}";
            await _cacheService.RemoveAsync(userOrdersCacheKey);
            
            _logger.LogDebug("清除用户订单列表缓存，用户ID: {UserId}", userId);
        }

        private async Task ClearUserOrderStatsCache(int userId)
        {
            string statsCacheKey = $"{USER_ORDER_STATS_CACHE_KEY_PREFIX}{userId}";
            await _cacheService.RemoveAsync(statsCacheKey);
            
            _logger.LogDebug("清除用户订单统计缓存，用户ID: {UserId}", userId);
        }

        public async Task<object> GetUserOrderStatsAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("用户ID必须大于0", nameof(userId));
            }

            _logger.LogInformation("开始获取用户订单统计信息，用户ID: {UserId}", userId);
            
            // 尝试从缓存获取
            string cacheKey = $"{USER_ORDER_STATS_CACHE_KEY_PREFIX}{userId}";
            var cachedStats = await _cacheService.GetAsync<object>(cacheKey);
            if (cachedStats != null)
            {
                _logger.LogInformation("从缓存获取用户订单统计信息成功，用户ID: {UserId}", userId);
                return cachedStats;
            }

            _logger.LogInformation("缓存未命中，从数据库获取用户订单统计信息，用户ID: {UserId}", userId);
            var orders = await _orderRepository.GetAllAsync();
            var userOrders = orders.Where(o => o.UserId == userId).ToList();

            var totalOrders = userOrders.Count;
            var totalAmount = userOrders.Sum(o => o.TotalAmount);
            var pendingOrders = userOrders.Count(o => o.Status == "pending");
            var completedOrders = userOrders.Count(o => o.Status == "delivered");

            var result = new
            {
                totalOrders = totalOrders,
                totalAmount = totalAmount,
                pendingOrders = pendingOrders,
                completedOrders = completedOrders
            };

            // 缓存结果，有效期10分钟
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
            _logger.LogInformation("获取用户订单统计信息完成，用户ID: {UserId}", userId);
            
            return result;
        }

        public async Task<IEnumerable<OrderStatusHistoryDto>> GetOrderStatusHistoryAsync(int userId, int orderId)
        {
            if (userId <= 0 || orderId <= 0)
            {
                throw new ArgumentException("用户或订单ID无效");
            }

            _logger.LogInformation("开始获取订单状态历史，订单ID: {OrderId}", orderId);
            
            // 尝试从缓存获取
            string cacheKey = $"{ORDER_HISTORY_CACHE_KEY_PREFIX}{userId}_{orderId}";
            var cachedHistory = await _cacheService.GetAsync<IEnumerable<OrderStatusHistoryDto>>(cacheKey);
            if (cachedHistory != null)
            {
                _logger.LogInformation("从缓存获取订单状态历史成功，订单ID: {OrderId}", orderId);
                return cachedHistory;
            }

            _logger.LogInformation("缓存未命中，从数据库获取订单状态历史，订单ID: {OrderId}", orderId);
            // 验证订单是否存在
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.UserId != userId)
            {
                throw new ArgumentException("订单不存在");
            }

            // 获取订单状态历史记录
            var histories = await _orderStatusHistoryRepository.GetAllAsync();
            var orderHistories = histories.Where(h => h.OrderId == orderId).OrderByDescending(h => h.CreatedAt);

            // 转换为DTO
            var historyDtos = orderHistories.Select(h => new OrderStatusHistoryDto
            {
                Id = h.Id,
                OrderId = h.OrderId,
                OrderNumber = order.OrderNumber ?? "",
                Status = h.NewStatus ?? "unknown",
                Reason = h.Note ?? "",
                Operator = h.OperatorName ?? "系统",
                CreatedAt = h.CreatedAt
            }).ToList();

            // 缓存结果，有效期5分钟
            await _cacheService.SetAsync(cacheKey, historyDtos, TimeSpan.FromMinutes(5));
            _logger.LogInformation("获取订单状态历史成功，订单ID: {OrderId}，记录数: {Count}", orderId, historyDtos.Count);
            
            return historyDtos;
        }
    }
}
