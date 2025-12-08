using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuanyuFlowerShop.Entities
{
    /// <summary>
    /// 用户实体类
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 用户电话号码
        /// </summary>
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string? Phone { get; set; }

        /// <summary>
        /// 用户评价集合
        /// </summary>
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? FullName { get; set; }

        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? Avatar { get; set; }

        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? Address { get; set; }

        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string? Gender { get; set; }

        /// <summary>
        /// 用户角色：admin(管理员) 或 user(普通用户)
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string Role { get; set; } = "user";

        /// <summary>
        /// 账户状态：active(活跃) 或 inactive(禁用)
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = "active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 上次登录时间
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        // 导航属性
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}