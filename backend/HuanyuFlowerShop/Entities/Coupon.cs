using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuanyuFlowerShop.Entities
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string DiscountType { get; set; } = "amount"; // amount | percent

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Value { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal MinOrderAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaxDiscount { get; set; }

        public int? UsageLimit { get; set; }

        public int? UsageLimitPerUser { get; set; }

        public int UsedCount { get; set; } = 0;

        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = "active"; // active | inactive

        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
