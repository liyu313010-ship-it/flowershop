using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Interfaces
{
    /// <summary>
    /// 支付服务接口
    /// 提供支付相关的核心功能，包括支付创建、验证、退款等
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// 创建支付
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="orderId">订单ID</param>
        /// <param name="paymentMethod">支付方式</param>
        /// <returns>支付结果</returns>
        Task<PaymentResult> CreatePaymentAsync(int userId, int orderId, string paymentMethod);

        /// <summary>
        /// 验证支付
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="orderId">订单ID</param>
        /// <param name="paymentReference">支付参考号</param>
        /// <returns>验证结果</returns>
        Task<PaymentResult> VerifyPaymentAsync(int userId, int orderId, string paymentReference);

        /// <summary>
        /// 取消支付
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="orderId">订单ID</param>
        /// <returns>取消结果</returns>
        Task<PaymentResult> CancelPaymentAsync(int userId, int orderId);

        /// <summary>
        /// 查询支付状态
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="orderId">订单ID</param>
        /// <returns>支付状态信息</returns>
        Task<PaymentResult> GetPaymentStatusAsync(int userId, int orderId);

        /// <summary>
        /// 生成支付链接
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="orderId">订单ID</param>
        /// <returns>支付链接和支付参考号</returns>
        Task<(string PaymentUrl, string PaymentReference)> GeneratePaymentLinkAsync(int userId, int orderId);
        
        Task<PaymentResult> ProcessCallbackAsync(string paymentReference, string tradeStatus, string? paymentMethod = null);
    }
}
