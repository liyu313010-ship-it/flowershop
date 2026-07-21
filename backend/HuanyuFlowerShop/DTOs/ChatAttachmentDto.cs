using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HuanyuFlowerShop.DTOs;

public class UploadChatAttachmentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "会话编号无效")]
    public int ConversationId { get; set; }

    [Required(ErrorMessage = "请选择要发送的文件")]
    public IFormFile File { get; set; } = null!;

    [StringLength(500, ErrorMessage = "附件说明不能超过500个字符")]
    public string? Caption { get; set; }

    [StringLength(64)]
    public string? ClientMessageId { get; set; }
}

public record StoredChatAttachment(
    string OriginalName,
    string StorageName,
    string ContentType,
    long Size,
    string MessageType);

public record ChatAttachmentDownload(
    int MessageId,
    string OriginalName,
    string StorageName,
    string ContentType,
    long Size);

public record AttachmentMessageResult(MessageDto Message, bool Created);
