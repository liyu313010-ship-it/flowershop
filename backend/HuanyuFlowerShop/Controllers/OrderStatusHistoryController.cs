using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Services;

namespace HuanyuFlowerShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderStatusHistoryController : ControllerBase
    {
        private readonly IOrderStatusHistoryService _orderStatusHistoryService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="orderStatusHistoryService">订单状态历史服务</param>
        public OrderStatusHistoryController(IOrderStatusHistoryService orderStatusHistoryService)
        {
            _orderStatusHistoryService = orderStatusHistoryService;
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("order/{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] HuanyuFlowerShop.DTOs.UpdateOrderStatusRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _orderStatusHistoryService.UpdateOrderStatusAsync(orderId, request);
            if (!result)
            {
                return NotFound(new { message = "订单不存在或更新失败" });
            }

            return Ok(new { message = "订单状态更新成功" });
        }

        /// <summary>
        /// 获取订单状态历史
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>订单状态历史列表</returns>
        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderStatusHistory(int orderId, int pageIndex = 1, int pageSize = 20)
        {
            var result = await _orderStatusHistoryService.GetOrderStatusHistoryAsync(orderId, pageIndex, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// 获取最新订单状态
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>最新的订单状态历史记录</returns>
        [HttpGet("order/{orderId}/latest")]
        public async Task<IActionResult> GetLatestOrderStatus(int orderId)
        {
            var result = await _orderStatusHistoryService.GetLatestOrderStatusAsync(orderId);
            if (result == null)
            {
                return NotFound(new { message = "未找到订单状态历史记录" });
            }

            return Ok(result);
        }
    }
}