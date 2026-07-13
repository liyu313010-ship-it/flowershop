using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuanyuFlowerShop.Entities
{
    /// <summary>
    /// 订单实体类
    /// </summary>
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 订单状态：pending(待处理), processing(处理中), shipped(已发货), delivered(已送达), cancelled(已取消)
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = "pending";

        /// <summary>
        /// 订单总金额
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 商品总金额
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// 配送费用
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ShippingFee { get; set; } = 0;

        /// <summary>
        /// 订单优惠金额
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        /// <summary>
        /// 应用的优惠码
        /// </summary>
        [StringLength(50)]
        [Column("UsedCouponCode", TypeName = "varchar(50)")]
        public string? CouponCode { get; set; }

        /// <summary>
        /// 收货人姓名
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string RecipientName { get; set; } = string.Empty;

        /// <summary>
        /// 收货人电话
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string RecipientPhone { get; set; } = string.Empty;

        /// <summary>
        /// 收货地址
        /// </summary>
        [Required]
        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string DeliveryAddress { get; set; } = string.Empty;

        /// <summary>
        /// 配送时间要求
        /// </summary>
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string? DeliveryTime { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [StringLength(30)]
        public string ShippingMethod { get; set; } = "standard";

        [StringLength(50)]
        public string? SenderName { get; set; }

        [StringLength(500)]
        public string? CardMessage { get; set; }

        public bool IsAnonymous { get; set; }

        [StringLength(30)]
        public string SubstitutionPreference { get; set; } = "contact_me";

        /// <summary>
        /// 留言
        /// </summary>
        [StringLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string? Message { get; set; }

        /// <summary>
        /// 支付方式：cod(货到付款), online(在线支付)
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string PaymentMethod { get; set; } = "cod";

        /// <summary>
        /// 支付状态：unpaid(未支付), paid(已支付), refunded(已退款), partial_refunded(部分退款)
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string PaymentStatus { get; set; } = "unpaid";
        
        /// <summary>
        /// 支付参考号
        /// </summary>
        public string? PaymentReference { get; set; }
        
        /// <summary>
        /// 已支付金额
        /// </summary>
        public decimal PaidAmount { get; set; } = 0;
        
        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime? PaidAt { get; set; }
        
        /// <summary>
        /// 退款金额
        /// </summary>
        public decimal RefundedAmount { get; set; } = 0;
        
        /// <summary>
        /// 支付过期时间
        /// </summary>
        public DateTime? PaymentExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ShippedAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        // 导航属性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
        public virtual ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
    }
}
