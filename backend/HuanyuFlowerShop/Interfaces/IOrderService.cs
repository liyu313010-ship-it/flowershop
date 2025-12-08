using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetUserOrdersAsync(int userId);
        Task<OrderDto?> GetOrderByIdAsync(int userId, int orderId);
        Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto createOrderDto);
        Task<OrderDto?> UpdateOrderStatusAsync(int userId, int orderId, string status);
        Task<bool> CancelOrderAsync(int userId, int orderId);
        Task<object> GetUserOrderStatsAsync(int userId);
        
        // 支付相关方法
        Task<OrderDto?> UpdatePaymentStatusAsync(int userId, int orderId, string paymentStatus, string? paymentReference = null, string? paymentMethod = null);
        Task<OrderDto?> ProcessPaymentAsync(int userId, int orderId, string paymentMethod);
        Task<bool> VerifyPaymentAsync(int userId, int orderId, string paymentReference);
        Task<IEnumerable<OrderStatusHistoryDto>> GetOrderStatusHistoryAsync(int orderId);
    }
}