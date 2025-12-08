using System.ComponentModel.DataAnnotations;

namespace HuanyuFlowerShop.DTOs
{
    /// <summary>
    /// 地址数据传输对象
    /// </summary>
    public class AddressDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string RecipientName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Province { get; set; } = null!;
        public string City { get; set; } = null!;
        public string District { get; set; } = null!;
        public string DetailAddress { get; set; } = null!;
        public string? PostalCode { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // 完整地址拼接
        public string FullAddress => $"{Province}{City}{District}{DetailAddress}";
    }
    
    /// <summary>
    /// 创建地址的请求对象
    /// </summary>
    public class CreateAddressDto
    {
        [Required(ErrorMessage = "收件人姓名不能为空")]
        [MaxLength(50, ErrorMessage = "收件人姓名不能超过50个字符")]
        public string RecipientName { get; set; } = null!;
        
        [Required(ErrorMessage = "联系电话不能为空")]
        [MaxLength(20, ErrorMessage = "联系电话不能超过20个字符")]
        [RegularExpression("^1[3-9]\\d{9}$", ErrorMessage = "手机号格式不正确")]
        public string PhoneNumber { get; set; } = null!;
        
        [Required(ErrorMessage = "省/直辖市不能为空")]
        [MaxLength(50, ErrorMessage = "省/直辖市不能超过50个字符")]
        public string Province { get; set; } = null!;
        
        [Required(ErrorMessage = "市不能为空")]
        [MaxLength(50, ErrorMessage = "市不能超过50个字符")]
        public string City { get; set; } = null!;
        
        [Required(ErrorMessage = "区/县不能为空")]
        [MaxLength(50, ErrorMessage = "区/县不能超过50个字符")]
        public string District { get; set; } = null!;
        
        [Required(ErrorMessage = "详细地址不能为空")]
        [MaxLength(200, ErrorMessage = "详细地址不能超过200个字符")]
        public string DetailAddress { get; set; } = null!;
        
        [MaxLength(20, ErrorMessage = "邮政编码不能超过20个字符")]
        public string? PostalCode { get; set; }
        
        public bool IsDefault { get; set; } = false;
    }
    
    /// <summary>
    /// 更新地址的请求对象
    /// </summary>
    public class UpdateAddressDto
    {
        [Required(ErrorMessage = "收件人姓名不能为空")]
        [MaxLength(50, ErrorMessage = "收件人姓名不能超过50个字符")]
        public string RecipientName { get; set; } = null!;
        
        [Required(ErrorMessage = "联系电话不能为空")]
        [MaxLength(20, ErrorMessage = "联系电话不能超过20个字符")]
        [RegularExpression("^1[3-9]\\d{9}$", ErrorMessage = "手机号格式不正确")]
        public string PhoneNumber { get; set; } = null!;
        
        [Required(ErrorMessage = "省/直辖市不能为空")]
        [MaxLength(50, ErrorMessage = "省/直辖市不能超过50个字符")]
        public string Province { get; set; } = null!;
        
        [Required(ErrorMessage = "市不能为空")]
        [MaxLength(50, ErrorMessage = "市不能超过50个字符")]
        public string City { get; set; } = null!;
        
        [Required(ErrorMessage = "区/县不能为空")]
        [MaxLength(50, ErrorMessage = "区/县不能超过50个字符")]
        public string District { get; set; } = null!;
        
        [Required(ErrorMessage = "详细地址不能为空")]
        [MaxLength(200, ErrorMessage = "详细地址不能超过200个字符")]
        public string DetailAddress { get; set; } = null!;
        
        [MaxLength(20, ErrorMessage = "邮政编码不能超过20个字符")]
        public string? PostalCode { get; set; }
        
        public bool IsDefault { get; set; }
    }
}
