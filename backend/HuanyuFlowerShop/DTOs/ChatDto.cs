using System.ComponentModel.DataAnnotations;

namespace HuanyuFlowerShop.DTOs
{
    /// <summary>
    /// 消息DTO
    /// </summary>
    public class MessageDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderAvatar { get; set; } = string.Empty;
        public int ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "text";
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public int ConversationId { get; set; }
    }

    /// <summary>
    /// 发送消息请求
    /// </summary>
    public class SendMessageRequest
    {
        [Required]
        public int ReceiverId { get; set; }
        
        [Required(ErrorMessage = "消息内容不能为空")]
        [StringLength(2000, ErrorMessage = "消息内容不能超过2000个字符")]
        public string Content { get; set; } = string.Empty;
        
        [StringLength(20)]
        public string MessageType { get; set; } = "text";
    }

    /// <summary>
    /// 会话DTO
    /// </summary>
    public class ConversationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserAvatar { get; set; } = string.Empty;
        public int AdminId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime? LastMessageTime { get; set; }
        public int UnreadCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 标记消息已读请求
    /// </summary>
    public class MarkMessagesReadRequest
    {
        [Required]
        public int ConversationId { get; set; }
    }
}
