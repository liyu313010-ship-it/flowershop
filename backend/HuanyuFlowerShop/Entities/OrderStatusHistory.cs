using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuanyuFlowerShop.Entities
{
    /// <summary>
    /// 订单状态历史实体类
    /// </summary>
    public class OrderStatusHistory
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 订单ID
        /// </summary>
        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// 变更前状态
        /// </summary>
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string? OldStatus { get; set; }

        /// <summary>
        /// 变更后状态
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string NewStatus { get; set; } = string.Empty;

        /// <summary>
        /// 操作人ID（系统管理员ID）
        /// </summary>
        public int? OperatorId { get; set; }

        /// <summary>
        /// 操作人名称
        /// </summary>
        [StringLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string OperatorName { get; set; } = string.Empty;

        /// <summary>
        /// 备注信息
        /// </summary>
        [StringLength(500)]
        [Column(TypeName = "varchar(500)")]
        public string? Note { get; set; }

        /// <summary>
        /// 状态变更时间
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 导航属性
        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}