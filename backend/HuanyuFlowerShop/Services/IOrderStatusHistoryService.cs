using System.Threading.Tasks;
using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Services
{
    /// <summary>
    /// 订单状态历史服务接口
    /// </summary>
    public interface IOrderStatusHistoryService
    {
        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>是否更新成功</returns>
        Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request);

        /// <summary>
        /// 获取订单状态历史
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>订单状态历史列表</returns>
        Task<OrderStatusHistoryListResponse> GetOrderStatusHistoryAsync(int orderId, int pageIndex = 1, int pageSize = 20);

        /// <summary>
        /// 获取最新订单状态
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>最新的订单状态历史记录</returns>
        Task<OrderStatusHistoryDto?> GetLatestOrderStatusAsync(int orderId);
    }
}