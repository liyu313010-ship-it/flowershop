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
    private readonly ILogger<OrderStatusHistoryService> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">数据库上下文</param>
        /// <param name="logger">结构化日志记录器</param>
        public OrderStatusHistoryService(ApplicationDbContext context, ILogger<OrderStatusHistoryService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
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

            var oldStatus = order.Status;
            // 创建状态历史记录
            var statusHistory = new OrderStatusHistory
            {
                OrderId = orderId,
                OldStatus = oldStatus,
                NewStatus = request.Status,
                OperatorName = request.Operator,
                CreatedAt = DateTime.Now
            };

            // 更新订单状态
            order.Status = request.Status;
            order.UpdatedAt = DateTime.Now;

            if (request.Status == "delivered" && oldStatus != "delivered")
            {
                var user = await _context.Users.FindAsync(order.UserId);
                if (user is not null)
                {
                    user.Points += Math.Max(1, (int)Math.Floor(order.TotalAmount));
                }
            }

            // 添加状态历史记录
            _context.OrderStatusHistories.Add(statusHistory);

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新订单状态失败，订单ID: {OrderId}，目标状态: {Status}", orderId, request.Status);
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
