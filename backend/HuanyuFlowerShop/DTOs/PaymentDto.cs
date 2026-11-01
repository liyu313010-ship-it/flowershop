namespace HuanyuFlowerShop.DTOs
{
    /// <summary>
    /// 支付状态请求模型
    /// </summary>
    public class PaymentStatusRequest
    {
        public required string PaymentStatus { get; set; }
        public string? PaymentReference { get; set; }
        public string? PaymentMethod { get; set; }
    }

    /// <summary>
    /// 支付方式请求模型
    /// </summary>
    public class PaymentMethodRequest
    {
        public required string PaymentMethod { get; set; }
    }

    /// <summary>
    /// 验证支付请求模型
    /// </summary>
    public class VerifyPaymentRequest
    {
        public required string PaymentReference { get; set; }
    }

    /// <summary>
    /// 创建支付请求模型
    /// </summary>
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "CNY";
        public required string PaymentMethod { get; set; }
        public required string ReturnUrl { get; set; }
    }

    /// <summary>
    /// 验证支付请求模型（服务层使用）
    /// </summary>
    public class PaymentVerificationRequest
    {
        public int OrderId { get; set; }
        public required string PaymentReference { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// 支付结果响应模型
    /// </summary>
    public class PaymentResult
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public string? PaymentReference { get; set; } = null;
        public string? ErrorMessage { get; set; } = null;
        public string? OrderStatus { get; set; } = null;
        public string? PaymentStatus { get; set; } = null;
    }
}