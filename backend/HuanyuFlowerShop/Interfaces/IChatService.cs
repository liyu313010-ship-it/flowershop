using HuanyuFlowerShop.DTOs;

namespace HuanyuFlowerShop.Interfaces;

public interface IChatService
{
    Task<ConversationDto> GetOrCreateSupportConversationAsync(int userId);
    Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(int actorId, bool isAdmin, string? status = null);
    Task<ConversationDto?> GetConversationAsync(int conversationId, int actorId, bool isAdmin);
    Task<IReadOnlyList<MessageDto>> GetMessagesAsync(int conversationId, int actorId, bool isAdmin, int page, int pageSize);
    Task<MessageDto> SendMessageAsync(int actorId, bool isAdmin, SendMessageRequest request);
    Task<AttachmentMessageResult> SendAttachmentMessageAsync(
        int actorId,
        bool isAdmin,
        UploadChatAttachmentRequest request,
        StoredChatAttachment attachment);
    Task<ChatAttachmentDownload?> GetAttachmentAsync(int messageId, int actorId, bool isAdmin);
    Task<bool> MarkMessageReadAsync(int messageId, int actorId, bool isAdmin);
    Task<bool> MarkConversationReadAsync(int conversationId, int actorId, bool isAdmin);
    Task<ConversationDto?> AssignConversationAsync(int conversationId, int adminId);
    Task<ConversationDto?> CloseConversationAsync(int conversationId, int adminId);
    Task<int> GetUnreadCountAsync(int actorId, bool isAdmin);
    Task<bool> CanAccessConversationAsync(int conversationId, int actorId, bool isAdmin);
}
