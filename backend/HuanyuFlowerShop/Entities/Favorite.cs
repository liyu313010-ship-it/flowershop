using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuanyuFlowerShop.Entities
{
    /// <summary>
    /// 用户收藏商品实体类
    /// </summary>
    [Table("userfavorites")]
    public class Favorite
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 商品ID
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// 收藏时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        public virtual User? User { get; set; }
        public virtual Product? Product { get; set; }
    }
}