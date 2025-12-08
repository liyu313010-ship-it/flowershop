using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HuanyuFlowerShop.Interfaces;
using HuanyuFlowerShop.DTOs;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using HuanyuFlowerShop.Entities;
// 使用System.UnauthorizedAccessException类型不需要using命名空间

namespace HuanyuFlowerShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, IPaymentService paymentService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _paymentService = paymentService;
            _logger = logger;
        }

        // 更新支付状态
        [HttpPut("{orderId}/payment-status")]
        public async Task<IActionResult> UpdatePaymentStatus(int orderId, [FromBody] PaymentStatusRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                _logger.LogInformation("接收到支付状态更新请求，订单ID: {OrderId}, 状态: {PaymentStatus}, 支付方式: {PaymentMethod}, 参考号: {PaymentReference}", 
                    orderId, request.PaymentStatus, request.PaymentMethod, request.PaymentReference);
                    
                var result = await _orderService.UpdatePaymentStatusAsync(userId, orderId, request.PaymentStatus, request.PaymentReference, request.PaymentMethod);
                
                if (result == null)
                {
                    return NotFound("订单不存在或无权访问");
                }
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新支付状态失败，订单ID: {OrderId}, 请求: {@Request}", orderId, request);
                return StatusCode(500, "更新支付状态时发生错误");
            }
        }

        // 处理支付
        [HttpPost("{orderId}/process-payment")]
        public async Task<IActionResult> ProcessPayment(int orderId, [FromBody] PaymentMethodRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                var result = await _orderService.ProcessPaymentAsync(userId, orderId, request.PaymentMethod);
                
                if (result == null)
                {
                    return NotFound("订单不存在或无权访问");
                }
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理支付失败，订单ID: {OrderId}", orderId);
                return StatusCode(500, "处理支付时发生错误");
            }
        }

        // 验证支付
        [HttpPost("{orderId}/verify-payment")]
        public async Task<IActionResult> VerifyPayment(int orderId, [FromBody] VerifyPaymentRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                bool isVerified = await _orderService.VerifyPaymentAsync(userId, orderId, request.PaymentReference);
                
                if (isVerified)
                {
                    return Ok(new { success = true, message = "支付验证成功" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "支付验证失败" });
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证支付失败，订单ID: {OrderId}", orderId);
                return StatusCode(500, "验证支付时发生错误");
            }
        }

        // 生成支付链接
        [HttpGet("{orderId}/payment-link")]
        public async Task<IActionResult> GeneratePaymentLink(int orderId)
        {
            try
            {
                int userId = GetCurrentUserId();
                _logger.LogInformation("生成支付链接，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                
                // 生成支付链接
                var (paymentUrl, paymentReference) = await _paymentService.GeneratePaymentLinkAsync(userId, orderId);
                
                // 由于元组直接返回数据，默认为成功状态
                return Ok(new { success = true, paymentLink = paymentUrl, paymentReference = paymentReference });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成支付链接失败，订单ID: {OrderId}", orderId);
                return StatusCode(500, "生成支付链接时发生错误");
            }
        }

        // 查询支付状态
        [HttpGet("{orderId}/payment-status")]
        public async Task<IActionResult> GetPaymentStatus(int orderId)
        {
            try
            {
                int userId = GetCurrentUserId();
                _logger.LogInformation("查询支付状态，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                
                var statusResult = await _paymentService.GetPaymentStatusAsync(userId, orderId);
                
                if (statusResult.Success)
                {
                    return Ok(new {
                        success = true,
                        paymentReference = statusResult.PaymentReference
                    });
                }
                else
                {
                    _logger.LogWarning("查询支付状态失败: {Message}", statusResult.Message);
                    return BadRequest(new { success = false, message = statusResult.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询支付状态失败，订单ID: {OrderId}", orderId);
                return StatusCode(500, "查询支付状态时发生错误");
            }
        }

        /// <summary>
        /// 获取当前用户的订单列表
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 获取订单列表", userId);
                
                var orders = await _orderService.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单列表失败");
                return StatusCode(500, "获取订单列表时发生错误");
            }
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 获取订单详情，订单ID: {OrderId}", userId, orderId);
                
                var order = await _orderService.GetOrderByIdAsync(userId, orderId);
                if (order == null)
                {
                    _logger.LogWarning("订单 {OrderId} 不存在或不属于用户 {UserId}", orderId, userId);
                    return NotFound("订单不存在或不属于当前用户");
                }
                
                return Ok(order);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单详情失败");
                return StatusCode(500, "获取订单详情时发生错误");
            }
        }

        /// <summary>
        /// 创建新订单
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 创建新订单", userId);
                
                var order = await _orderService.CreateOrderAsync(userId, createOrderDto);
                return CreatedAtAction(nameof(GetOrder), new { orderId = order.Id }, order);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "创建订单失败");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建订单时发生错误");
                var detail = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, new { message = "创建订单时发生错误: " + detail });
            }
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        [HttpPut("{orderId}/cancel")]
        public async Task<ActionResult<OrderDto>> CancelOrder(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 取消订单，订单ID: {OrderId}", userId, orderId);
                
                // 检查订单是否存在
                var existingOrder = await _orderService.GetOrderByIdAsync(userId, orderId);
                if (existingOrder == null)
                {
                    _logger.LogWarning("订单 {OrderId} 不存在或不属于用户 {UserId}", orderId, userId);
                    return NotFound("订单不存在或不属于当前用户");
                }

                // 检查订单状态是否可取消
                if (existingOrder.Status != "pending" && existingOrder.Status != "processing")
                {
                    return BadRequest("只有待处理或处理中的订单才能取消");
                }

                // 取消订单
                var success = await _orderService.CancelOrderAsync(userId, orderId);
                if (!success)
                {
                    return StatusCode(500, "取消订单失败");
                }

                // 返回更新后的订单
                var updatedOrder = await _orderService.GetOrderByIdAsync(userId, orderId);
                return Ok(updatedOrder);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消订单时发生错误");
                return StatusCode(500, "取消订单时发生错误");
            }
        }

        /// <summary>
        /// 获取用户订单统计信息
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetOrderStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("用户 {UserId} 获取订单统计信息", userId);
                
                var stats = await _orderService.GetUserOrderStatsAsync(userId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单统计信息失败");
                return StatusCode(500, "获取订单统计信息时发生错误");
            }
        }

        /// <summary>
        /// 获取订单状态历史
        /// </summary>
        [HttpGet("{orderId}/status-history")]
        public async Task<IActionResult> GetOrderStatusHistory(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("用户请求订单状态历史，用户ID: {UserId}，订单ID: {OrderId}", userId, orderId);
                
                var history = await _orderService.GetOrderStatusHistoryAsync(orderId);
                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "获取订单状态历史失败，订单ID: {OrderId}", orderId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单状态历史失败，用户ID: {UserId}，订单ID: {OrderId}", GetCurrentUserId(), orderId);
                return StatusCode(500, "获取订单状态历史时发生错误");
            }
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        [HttpPut("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                _logger.LogInformation("接收到订单状态更新请求，订单ID: {OrderId}, 新状态: {Status}", 
                    orderId, request.Status);
                
                var result = await _orderService.UpdateOrderStatusAsync(userId, orderId, request.Status);
                
                if (result == null)
                {
                    return NotFound("订单不存在或无权访问");
                }
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新订单状态失败，订单ID: {OrderId}, 请求: {@Request}", orderId, request);
                return StatusCode(500, "更新订单状态时发生错误");
            }
        }

        /// <summary>
        /// 获取当前认证用户的ID
        /// </summary>
        private int GetCurrentUserId()
        {
            // 从 JWT 中解析用户标识，优先 NameIdentifier，其次 "sub"
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("无法识别用户身份");
            }
            if (int.TryParse(userIdClaim, out var id))
            {
                return id;
            }
            // 非数字ID（如 GUID）暂不支持，返回未授权，避免 500
            throw new UnauthorizedAccessException("非法用户标识");
        }
    }
}
