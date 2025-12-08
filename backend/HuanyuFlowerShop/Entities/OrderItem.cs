using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuanyuFlowerShop.Entities
{
    /// <summary>
    /// 订单项目实体类
    /// </summary>
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string? ProductImage { get; set; }

        [Required]
        [Range(1, 999)]
        public int Quantity { get; set; }

        /// <summary>
        /// 下单时的商品单价
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// 小计金额
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}