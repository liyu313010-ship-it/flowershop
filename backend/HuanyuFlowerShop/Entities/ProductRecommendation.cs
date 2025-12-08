using System.ComponentModel.DataAnnotations;

namespace HuanyuFlowerShop.Entities
{
    public class ProductRecommendation
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ForUserId { get; set; }
        public decimal Score { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
