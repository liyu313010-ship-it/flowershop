using System;

namespace HuanyuFlowerShop.DTOs
{
    /// <summary>
    /// 订单状态历史DTO
    /// </summary>
    public class OrderStatusHistoryDto
    {
        /// <summary>
        /// 历史记录ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string? OrderNumber { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// 状态变更原因
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string? Operator { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// 更新订单状态请求DTO
    /// </summary>
    public class UpdateOrderStatusRequest
    {
        /// <summary>
        /// 订单状态
        /// </summary>
        public string Status { get; set; } = null!;

        /// <summary>
        /// 状态变更原因
        /// </summary>
        public string? Reason { get; set; }
        public string? Note { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string? Operator { get; set; }
    }

    /// <summary>
    /// 订单状态历史列表响应DTO
    /// </summary>
    public class OrderStatusHistoryListResponse
    {
        /// <summary>
        /// 订单状态历史列表
        /// </summary>
        public List<OrderStatusHistoryDto> Items { get; set; } = new List<OrderStatusHistoryDto>();

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }
    }
}
