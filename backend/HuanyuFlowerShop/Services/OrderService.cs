using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.DTOs;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HuanyuFlowerShop.Data;
using Microsoft.EntityFrameworkCore;

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
        private readonly ApplicationDbContext _db;

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
            IPaymentService paymentService, IConfiguration configuration, IUnitOfWork unitOfWork, ApplicationDbContext db)
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
            _configuration = configuration; _unitOfWork = unitOfWork; _db = db;
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
                    if (orderItem != null)
                    {
                        orderItemDtos.Add(new OrderItemDto
                        {
                            Id = orderItem.Id,
                            ProductId = orderItem.ProductId,
                            ProductName = string.IsNullOrWhiteSpace(orderItem.ProductName) ? product?.Name ?? "已下架商品" : orderItem.ProductName,
                        ProductImage = string.IsNullOrWhiteSpace(orderItem.ProductImage) ? product?.ImageUrl ?? "" : orderItem.ProductImage,
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
                    Subtotal = order.Subtotal, ShippingFee = order.ShippingFee,
                    RecipientName = order.RecipientName, DeliveryDate = order.DeliveryDate,
                    ShippingMethod = order.ShippingMethod, DeliveryTime = order.DeliveryTime,
                    ShippedAt = order.ShippedAt, DeliveredAt = order.DeliveredAt,
                    DiscountAmount = order.DiscountAmount,
                    CouponCode = order.CouponCode,
                    ShippingAddress = order.DeliveryAddress ?? "",
                    Phone = order.RecipientPhone ?? "",
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
                if (orderItem != null)
                {
                    orderItemDtos.Add(new OrderItemDto
                    {
                        Id = orderItem.Id,
                        ProductId = orderItem.ProductId,
                        ProductName = string.IsNullOrWhiteSpace(orderItem.ProductName) ? product?.Name ?? "已下架商品" : orderItem.ProductName,
                        ProductImage = string.IsNullOrWhiteSpace(orderItem.ProductImage) ? product?.ImageUrl ?? "" : orderItem.ProductImage,
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
                    Subtotal = order.Subtotal, ShippingFee = order.ShippingFee,
                    RecipientName = order.RecipientName, DeliveryDate = order.DeliveryDate,
                    ShippingMethod = order.ShippingMethod, DeliveryTime = order.DeliveryTime,
                    ShippedAt = order.ShippedAt, DeliveredAt = order.DeliveredAt,
                DiscountAmount = order.DiscountAmount,
                CouponCode = order.CouponCode,
                ShippingAddress = order.DeliveryAddress ?? "",
                Phone = order.RecipientPhone ?? "",
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

        public async Task<OrderQuoteDto> GetQuoteAsync(int userId, OrderQuoteRequest request)
        {
            var quote = await BuildQuoteAsync(userId, request, false);
            return quote.Quote;
        }

        private async Task<(List<CartItem> Cart, List<OrderItem> Items, Coupon? Coupon, OrderQuoteDto Quote)> BuildQuoteAsync(
            int userId, OrderQuoteRequest request, bool reserve)
        {
            var method = (request.ShippingMethod ?? "standard").Trim().ToLowerInvariant();
            var cart = reserve && _db.Database.IsRelational()
                ? await _db.CartItems.FromSqlInterpolated($"SELECT * FROM `CartItems` WHERE `UserId` = {userId} FOR UPDATE").AsNoTracking().ToListAsync()
                : await _db.CartItems.AsNoTracking().Where(c => c.UserId == userId).ToListAsync();
            if (cart.Count == 0) throw new ArgumentException("购物车为空");
            var items = new List<OrderItem>();
            foreach (var item in cart.OrderBy(c => c.ProductId))
            {
                Product? product;
                if (reserve && _db.Database.IsRelational())
                    product = (await _db.Products.FromSqlInterpolated($"SELECT * FROM `Products` WHERE `Id` = {item.ProductId} FOR UPDATE").AsNoTracking().ToListAsync()).SingleOrDefault();
                else product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == item.ProductId);
                if (product is null || !product.IsActive) throw new ArgumentException("购物车包含不存在或已下架的商品");
                if (item.Quantity is < 1 or > 99) throw new ArgumentException("商品数量必须在1到99之间");
                if (product.Stock < item.Quantity) throw new ArgumentException($"产品 {product.Name} 库存不足");
                items.Add(new OrderItem { ProductId = product.Id, ProductName = product.Name,
                    ProductImage = product.ImageUrl ?? "", UnitPrice = product.Price,
                    Quantity = item.Quantity, Subtotal = product.Price * item.Quantity });
            }
            var subtotal = items.Sum(i => i.Subtotal);
            var shipping = OrderBusinessRules.ShippingFee(subtotal, method, _configuration);
            Coupon? coupon = null;
            decimal discount = 0;
            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var code = request.CouponCode.Trim().ToUpperInvariant();
                coupon = await _db.Coupons.AsNoTracking().FirstOrDefaultAsync(c => c.Code.ToUpper() == code);
                if (reserve && coupon is not null && _db.Database.IsRelational())
                    coupon = (await _db.Coupons.FromSqlInterpolated($"SELECT * FROM `Coupons` WHERE `Id` = {coupon.Id} FOR UPDATE").AsNoTracking().ToListAsync()).SingleOrDefault();
                var userCoupon = coupon is null ? null : await _db.UserCoupons.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == userId && u.CouponId == coupon.Id);
                discount = OrderBusinessRules.CouponDiscount(coupon, userCoupon, subtotal, DateTime.UtcNow);
            }
            return (cart, items, coupon, new OrderQuoteDto { Subtotal = subtotal, ShippingFee = shipping,
                DiscountAmount = discount, TotalAmount = subtotal + shipping - discount });
        }

        public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
        {
            if (userId <= 0) throw new ArgumentException("用户ID必须大于0");
            ArgumentNullException.ThrowIfNull(dto);
            if (string.IsNullOrWhiteSpace(dto.RecipientName) || dto.RecipientName.Length > 50) throw new ArgumentException("收货人姓名无效");
            if (string.IsNullOrWhiteSpace(dto.ShippingAddress) || dto.ShippingAddress.Length > 200) throw new ArgumentException("收货地址无效");
            if (string.IsNullOrWhiteSpace(dto.Phone) || !Regex.IsMatch(dto.Phone, @"^1[3-9]\d{9}$")) throw new ArgumentException("收货电话无效");
            var today = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).Date;
            var deliveryDate = dto.DeliveryDate?.Date ?? today;
            if (deliveryDate < today || deliveryDate > today.AddDays(30)) throw new ArgumentException("配送日期必须是今天至未来30天内");
            var method = (dto.ShippingMethod ?? "standard").Trim().ToLowerInvariant();
            if (method == "same_day" && deliveryDate == today && DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).Hour >= _configuration.GetValue("Shipping:SameDayCutoffHour", 15))
                throw new ArgumentException("已超过当日配送截止时间，请选择其他日期");
            var substitution = (dto.SubstitutionPreference ?? "contact_me").Trim().ToLowerInvariant();
            if (substitution is not ("contact_me" or "allow_similar" or "no_substitution")) throw new ArgumentException("不支持的替换偏好");
            var paymentMethod = (dto.PaymentMethod ?? "cod").Trim().ToLowerInvariant() switch
            {
                "cash_on_delivery" or "cash" or "cod" => "cod",
                "alipay" or "wechat_pay" or "wechat" or "online" => "online",
                _ => throw new ArgumentException("暂不支持该支付方式")
            };
            var requestId = string.IsNullOrWhiteSpace(dto.ClientRequestId) ? null : dto.ClientRequestId.Trim();
            if (requestId?.Length > 64) throw new ArgumentException("请求编号过长");
            var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto))));
            int orderId;
            await using (var transaction = _db.Database.IsRelational()
                ? await _db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted) : null)
            {
                // Serializes checkout requests for this user across app instances.
                if (_db.Database.IsRelational())
                {
                    var users = await _db.Users.FromSqlInterpolated($"SELECT * FROM `Users` WHERE `Id` = {userId} FOR UPDATE").AsNoTracking().ToListAsync();
                    if (users.SingleOrDefault()?.Status != "active") throw new UnauthorizedAccessException("账户不可用");
                }
                var existing = requestId is null ? null : await _db.Orders.AsNoTracking()
                    .FirstOrDefaultAsync(o => o.UserId == userId && o.ClientRequestId == requestId);
                if (existing is not null)
                {
                    if (existing.RequestFingerprint != fingerprint) throw new ArgumentException("请求编号已用于其他订单，请刷新后重试");
                    orderId = existing.Id;
                }
                else
                {
                    var quoted = await BuildQuoteAsync(userId, new OrderQuoteRequest { ShippingMethod = method, CouponCode = dto.CouponCode }, true);
                    if (dto.ExpectedTotal.HasValue && dto.ExpectedTotal.Value != quoted.Quote.TotalAmount)
                        throw new ArgumentException("订单金额已变化，请刷新报价并确认后重新提交");
                    var now = DateTime.UtcNow;
                    var order = new Order { UserId = userId, OrderNumber = GenerateOrderNumber(),
                        ClientRequestId = requestId, RequestFingerprint = fingerprint,
                        Status = "pending", PaymentStatus = "unpaid", PaymentMethod = paymentMethod,
                        PaymentExpiresAt = paymentMethod == "online" ? now.AddMinutes(30) : null,
                        Subtotal = quoted.Quote.Subtotal, ShippingFee = quoted.Quote.ShippingFee,
                        DiscountAmount = quoted.Quote.DiscountAmount, TotalAmount = quoted.Quote.TotalAmount,
                        CouponId = quoted.Coupon?.Id, CouponCode = quoted.Coupon?.Code,
                        RecipientName = dto.RecipientName.Trim(), RecipientPhone = dto.Phone.Trim(), DeliveryAddress = dto.ShippingAddress.Trim(),
                        DeliveryDate = deliveryDate, DeliveryTime = dto.DeliveryTime?.Trim(), ShippingMethod = method,
                        SenderName = dto.SenderName?.Trim(), CardMessage = dto.CardMessage?.Trim(),
                        IsAnonymous = dto.IsAnonymous, SubstitutionPreference = substitution, CreatedAt = now, UpdatedAt = now,
                        OrderItems = quoted.Items };
                    foreach (var item in quoted.Items)
                    {
                        if (_db.Database.IsRelational())
                        {
                            var changed = await _db.Products.Where(p => p.Id == item.ProductId && p.IsActive && p.Stock >= item.Quantity)
                                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - item.Quantity));
                            if (changed != 1) throw new ArgumentException($"产品 {item.ProductName} 库存不足");
                        }
                        else (await _db.Products.FindAsync(item.ProductId))!.Stock -= item.Quantity;
                    }
                    if (quoted.Coupon is not null && quoted.Quote.DiscountAmount > 0)
                    {
                        var couponId = quoted.Coupon.Id;
                        var userCoupon = await _db.UserCoupons.FirstOrDefaultAsync(u => u.UserId == userId && u.CouponId == couponId);
                        if (userCoupon is null)
                            _db.UserCoupons.Add(new UserCoupon { UserId = userId, CouponId = couponId, Status = "used", ClaimedAt = now, UsedAt = now });
                        else { userCoupon.Status = "used"; userCoupon.UsedAt = now; }
                        if (_db.Database.IsRelational())
                            await _db.Coupons.Where(c => c.Id == couponId).ExecuteUpdateAsync(s => s.SetProperty(c => c.UsedCount, c => c.UsedCount + 1));
                        else (await _db.Coupons.FindAsync(couponId))!.UsedCount += 1;
                    }
                    _db.Orders.Add(order);
                    _db.OrderStatusHistories.Add(new OrderStatusHistory { Order = order, NewStatus = "pending", OperatorId = userId, OperatorName = "用户", Note = "订单创建", CreatedAt = now });
                    foreach (var item in quoted.Cart)
                    {
                        var tracked = _db.CartItems.Local.FirstOrDefault(c => c.Id == item.Id);
                        _db.CartItems.Remove(tracked ?? item);
                    }
                    await _db.SaveChangesAsync();
                    orderId = order.Id;
                }
                if (transaction is not null) await transaction.CommitAsync();
            }
            await OrderCacheInvalidation.InvalidateAsync(_cacheService, userId, orderId);
            return await GetOrderByIdAsync(userId, orderId) ?? throw new InvalidOperationException("无法获取创建的订单详情");
        }

        public async Task<OrderDto?> UpdateOrderStatusAsync(int userId, int orderId, string status)
        {
            var order = await OrderWorkflow.ChangeStatusAsync(_db, _cacheService, orderId, status, userId, "用户");
            return order is null ? null : await GetOrderByIdAsync(userId, orderId);
        }

        public async Task<OrderDto?> UpdateOrderStatusAsAdminAsync(int orderId, string status, string operatorName)
        {
            var order = await OrderWorkflow.ChangeStatusAsync(_db, _cacheService, orderId, status, null, operatorName);
            return order is null ? null : await GetOrderByIdAsync(order.UserId, orderId);
        }

        public async Task<bool> CancelOrderAsync(int userId, int orderId)
        {
            try
            {
                var order = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
                if (order is null || order.Status is not ("pending" or "processing")) return false;
                return await OrderWorkflow.ChangeStatusAsync(_db, _cacheService, orderId, "cancelled", userId, "用户", "用户取消订单") is not null;
            }
            catch (ArgumentException) { return false; }
        }

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
                    
                    // 生成虚拟支付参考号（如果未提供）
                    if (string.IsNullOrEmpty(order.PaymentReference))
                    {
                        order.PaymentReference = "VIRTUAL_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + orderId;
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
                else if (paymentStatus == "refunded" || paymentStatus == "partial_refunded")
                {
                    // 如果是退款状态，记录退款金额
                    if (paymentStatus == "refunded")
                    {
                        order.RefundedAmount = order.PaidAmount;
                    }
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
                var validPaymentMethods = new[] { "credit_card", "alipay", "wechat_pay", "bank_transfer", "cash_on_delivery" };
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

                // 更新订单支付相关信息
                order.PaymentStatus = "paid";
                order.PaymentReference = paymentResult.PaymentReference;
                order.PaidAmount = order.TotalAmount;
                order.PaidAt = DateTime.UtcNow;
                order.Status = "processing"; // 支付成功后更新为处理中
                order.UpdatedAt = DateTime.UtcNow;

                await _orderRepository.UpdateAsync(order);
                
                // 创建支付成功记录
                 var statusHistory = new OrderStatusHistory
                 {
                     OrderId = orderId,
                     OldStatus = "pending",
                     NewStatus = "processing",
                     OperatorId = userId,
                     OperatorName = "系统",
                     Note = "订单支付成功，自动更新为处理中",
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
            return $"ORD{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }

        // 这个方法已经被PaymentService中的同名方法取代，但保留用于兼容性

        private async Task ClearOrderCache(int userId, int orderId)
        {
            string orderCacheKey = $"{ORDER_CACHE_KEY_PREFIX}{orderId}_{userId}";
            string historyCacheKey = $"{ORDER_HISTORY_CACHE_KEY_PREFIX}{orderId}_{userId}";
            
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
