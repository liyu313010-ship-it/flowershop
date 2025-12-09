using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuanyuFlowerShop.Entities
{
    /// <summary>
    /// 会话实体类
    /// </summary>
    public class Conversation
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// 管理员ID
        /// </summary>
        [Required]
        public int AdminId { get; set; }

        /// <summary>
        /// 最后一条消息
        /// </summary>
        [StringLength(2000)]
        [Column(TypeName = "text")]
        public string LastMessage { get; set; } = string.Empty;

        /// <summary>
        /// 最后一条消息时间
        /// </summary>
        [Column(TypeName = "datetime")]
        public DateTime? LastMessageTime { get; set; }

        /// <summary>
        /// 未读消息数
        /// </summary>
        [Required]
        public int UnreadCount { get; set; } = 0;

        /// <summary>
        /// 会话创建时间
        /// </summary>
        [Required]
        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 会话更新时间
        /// </summary>
        [Required]
        [Column(TypeName = "datetime")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}