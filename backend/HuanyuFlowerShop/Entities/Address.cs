using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuanyuFlowerShop.Entities
{
    /// <summary>
    /// 地址实体类
    /// </summary>
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "收件人姓名不能为空")]
        [StringLength(50, ErrorMessage = "收件人姓名不能超过50个字符")]
        [Column(TypeName = "varchar(50)")]
        public string RecipientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "联系电话不能为空")]
        [StringLength(20, ErrorMessage = "联系电话不能超过20个字符")]
        [Column("RecipientPhone", TypeName = "varchar(20)")]
        [RegularExpression("^1[3-9]\\d{9}$", ErrorMessage = "手机号格式不正确")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "省/直辖市不能为空")]
        [StringLength(50, ErrorMessage = "省/直辖市不能超过50个字符")]
        [Column(TypeName = "varchar(50)")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "市不能为空")]
        [StringLength(50, ErrorMessage = "市不能超过50个字符")]
        [Column(TypeName = "varchar(50)")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "区/县不能为空")]
        [StringLength(50, ErrorMessage = "区/县不能超过50个字符")]
        [Column(TypeName = "varchar(50)")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "详细地址不能为空")]
        [StringLength(200, ErrorMessage = "详细地址不能超过200个字符")]
        [Column(TypeName = "varchar(200)")]
        public string DetailAddress { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "邮政编码不能超过20个字符")]
        [Column(TypeName = "varchar(20)")]
        public string? PostalCode { get; set; }

        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 外键 - 与User关联
        [Required(ErrorMessage = "用户ID不能为空")]
        public int UserId { get; set; }

        // 导航属性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
    }
}
