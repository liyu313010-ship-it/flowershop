using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace HuanyuFlowerShop.Entities
{
    /// <summary>
    /// 产品评价实体类
    /// </summary>
    public class ProductReview
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "产品ID不能为空")]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        [Required(ErrorMessage = "用户ID不能为空")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required(ErrorMessage = "评分不能为空")]
        [Range(1, 5, ErrorMessage = "评分必须在1-5之间")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "评价内容不能超过500个字符")]
        [Column(TypeName = "varchar(500)")]
        public string? Comment { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// 是否已删除
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}
