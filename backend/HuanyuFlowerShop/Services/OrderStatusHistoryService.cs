using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Services
{
    /// <summary>
    /// 订单状态历史服务实现
    /// </summary>
    public class OrderStatusHistoryService : IOrderStatusHistoryService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">数据库上下文</param>
        public OrderStatusHistoryService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>是否更新成功</returns>
        public async Task<bool> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            // 创建状态历史记录
            var statusHistory = new OrderStatusHistory
            {
                OrderId = orderId,
                NewStatus = request.Status,
                OperatorName = request.Operator,
                CreatedAt = DateTime.Now
            };

            // 更新订单状态
            order.Status = request.Status;
            order.UpdatedAt = DateTime.Now;

            // 添加状态历史记录
            _context.OrderStatusHistories.Add(statusHistory);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取订单状态历史
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>订单状态历史列表</returns>
        public async Task<OrderStatusHistoryListResponse> GetOrderStatusHistoryAsync(int orderId, int pageIndex = 1, int pageSize = 20)
        {
            if (pageIndex < 1) pageIndex = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var query = _context.OrderStatusHistories
                .Where(h => h.OrderId == orderId)
                .Include(h => h.Order)
                .OrderByDescending(h => h.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new OrderStatusHistoryDto
                {
                    Id = h.Id,
                    OrderId = h.OrderId,
                    OrderNumber = (h.Order != null ? h.Order.OrderNumber : string.Empty),
                    Status = h.NewStatus,
                    Operator = h.OperatorName,
                    CreatedAt = h.CreatedAt
                })
                .ToListAsync();

            return new OrderStatusHistoryListResponse
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// 获取最新订单状态
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>最新的订单状态历史记录</returns>
        public async Task<OrderStatusHistoryDto?> GetLatestOrderStatusAsync(int orderId)
        {
            var latestHistory = await _context.OrderStatusHistories
                .Where(h => h.OrderId == orderId)
                .Include(h => h.Order)
                .OrderByDescending(h => h.CreatedAt)
                .FirstOrDefaultAsync();

            if (latestHistory == null)
            {
                return null;
            }

            return new OrderStatusHistoryDto
            {
                Id = latestHistory.Id,
                OrderId = latestHistory.OrderId,
                OrderNumber = (latestHistory.Order != null ? latestHistory.Order.OrderNumber : string.Empty),
                Status = latestHistory.NewStatus,
                Reason = (latestHistory.Note != null ? latestHistory.Note : string.Empty),
                Operator = latestHistory.OperatorName,
                CreatedAt = latestHistory.CreatedAt
            };
        }
    }
}