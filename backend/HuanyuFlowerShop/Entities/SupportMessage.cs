using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuanyuFlowerShop.Entities;

[Table("SupportMessages")]
public class SupportMessage
{
    [Key]
    public int Id { get; set; }

    public int ConversationId { get; set; }
    public int SenderId { get; set; }

    [Required, StringLength(2000)]
    public string Content { get; set; } = string.Empty;

    [Required, StringLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string MessageType { get; set; } = "text";

    [StringLength(64)]
    [Column(TypeName = "varchar(64)")]
    public string? ClientMessageId { get; set; }

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SupportConversation Conversation { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
