using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuanyuFlowerShop.Entities
{
    /// <summary>
    /// 审计日志实体类
    /// </summary>
    [Table("audit_logs")]
    public class AuditLog
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Column("user_id")]
        public int? UserId { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        [Column("action")]
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// 资源类型
        /// </summary>
        [Column("resource")]
        [Required]
        [StringLength(50)]
        public string Resource { get; set; } = string.Empty;

        /// <summary>
        /// 资源ID
        /// </summary>
        [Column("resource_id")]
        [StringLength(100)]
        public string? ResourceId { get; set; }

        /// <summary>
        /// 详细信息
        /// </summary>
        [Column("details")]
        [StringLength(1000)]
        public string? Details { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        [Column("ip_address")]
        [StringLength(45)]
        public string? IPAddress { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [Column("created_at")]
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}