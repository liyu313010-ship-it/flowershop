using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HuanyuFlowerShop.Entities;

[Table("SupportConversations")]
public class SupportConversation
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }
    public int? AdminId { get; set; }

    [Required, StringLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string Status { get; set; } = "waiting";

    [StringLength(500)]
    public string? LastMessage { get; set; }

    public DateTime? LastMessageAt { get; set; }
    public int UserUnreadCount { get; set; }
    public int AdminUnreadCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public User User { get; set; } = null!;
    public User? Admin { get; set; }
    public ICollection<SupportMessage> Messages { get; set; } = new List<SupportMessage>();
}
