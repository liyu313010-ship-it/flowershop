using System.ComponentModel.DataAnnotations;

namespace HuanyuFlowerShop.Entities
{
    public class UserCoupon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int CouponId { get; set; }

        public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UsedAt { get; set; }

        public string Status { get; set; } = "claimed"; // claimed | used | expired
    }
}
