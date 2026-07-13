namespace HuanyuFlowerShop.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public DateTime? DeliveryDate { get; set; }
        public string ShippingMethod { get; set; } = "standard";
        public string? DeliveryTime { get; set; }
        public string? SenderName { get; set; }
        public string? CardMessage { get; set; }
        public bool IsAnonymous { get; set; }
        public string SubstitutionPreference { get; set; } = "contact_me";
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = new();
        
        // 支付相关字段
        public string PaymentStatus { get; set; } = "unpaid";
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public DateTime? PaidAt { get; set; }
        public decimal RefundedAmount { get; set; }
        public DateTime? PaymentExpiresAt { get; set; }
        
        // 物流信息
        public ShippingInfoDto? ShippingInfo { get; set; }
    }
    
    public class ShippingInfoDto
    {
        public string Company { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CreateOrderDto
    {
        public string RecipientName { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "cod"; // 未配置支付网关时默认货到付款
        public string? CouponCode { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string ShippingMethod { get; set; } = "standard";
        public string? DeliveryTime { get; set; }
        public string? SenderName { get; set; }
        public string? CardMessage { get; set; }
        public bool IsAnonymous { get; set; }
        public string SubstitutionPreference { get; set; } = "contact_me";
    }

    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
