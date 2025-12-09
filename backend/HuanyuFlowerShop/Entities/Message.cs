using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuanyuFlowerShop.Entities
{
    /// <summary>
    /// 消息实体类
    /// </summary>
    public class Message
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 发送者ID（用户ID或管理员ID）
        /// </summary>
        [Required]
        public int SenderId { get; set; }

        /// <summary>
        /// 接收者ID（用户ID或管理员ID）
        /// </summary>
        [Required]
        public int ReceiverId { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        [Required]
        [StringLength(2000)]
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// 消息类型：text, image, file
        /// </summary>
        [Required]
        [StringLength(20)]
        [Column(TypeName = "varchar(20)")]
        public string MessageType { get; set; } = "text";

        /// <summary>
        /// 是否已读
        /// </summary>
        [Required]
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// 发送时间
        /// </summary>
        [Required]
        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 更新时间
        /// </summary>
        [Required]
        [Column(TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}