using System.ComponentModel.DataAnnotations;

namespace HuanyuFlowerShop.Entities;

public sealed class PasswordResetToken
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    [Required, MaxLength(128)]
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAt { get; set; }
    public User? User { get; set; }
}
