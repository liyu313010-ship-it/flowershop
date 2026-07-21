using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Exceptions;
using HuanyuFlowerShop.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HuanyuFlowerShop.Services;

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _db;

    public ChatService(ApplicationDbContext db) => _db = db;

    public async Task<ConversationDto> GetOrCreateSupportConversationAsync(int userId)
    {
        var conversation = await ConversationQuery()
            .Where(c => c.UserId == userId && c.Status != "closed")
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync();

        if (conversation is not null) return MapConversation(conversation, false);

        var userExists = await _db.Users.AnyAsync(u => u.Id == userId && u.Status == "active");
        if (!userExists) throw new UserNotFoundException(userId);

        conversation = new SupportConversation
        {
            UserId = userId,
            Status = "waiting",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.SupportConversations.Add(conversation);
        await _db.SaveChangesAsync();
        conversation = await ConversationQuery().SingleAsync(c => c.Id == conversation.Id);
        return MapConversation(conversation, false);
    }

    public async Task<IReadOnlyList<ConversationDto>> GetConversationsAsync(int actorId, bool isAdmin, string? status = null)
    {
        var query = ConversationQuery();
        if (!isAdmin) query = query.Where(c => c.UserId == actorId);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(c => c.Status == status);

        return (await query.OrderByDescending(c => c.LastMessageAt ?? c.UpdatedAt).ToListAsync())
            .Select(c => MapConversation(c, isAdmin))
            .ToList();
    }

    public async Task<ConversationDto?> GetConversationAsync(int conversationId, int actorId, bool isAdmin)
    {
        var conversation = await ConversationQuery().SingleOrDefaultAsync(c => c.Id == conversationId);
        if (conversation is null || (!isAdmin && conversation.UserId != actorId)) return null;
        return MapConversation(conversation, isAdmin);
    }

    public async Task<IReadOnlyList<MessageDto>> GetMessagesAsync(int conversationId, int actorId, bool isAdmin, int page, int pageSize)
    {
        if (!await CanAccessConversationAsync(conversationId, actorId, isAdmin)) return Array.Empty<MessageDto>();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var conversation = await _db.SupportConversations.AsNoTracking().SingleAsync(c => c.Id == conversationId);
        var messages = await _db.SupportMessages.AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        messages.Reverse();
        return messages.Select(m => MapMessage(m, conversation)).ToList();
    }

    public async Task<MessageDto> SendMessageAsync(int actorId, bool isAdmin, SendMessageRequest request)
    {
        var content = (request.Content ?? string.Empty).Trim();
        if (content.Length == 0) throw new BusinessException("消息内容不能为空");
        if (content.Length > 2000) throw new BusinessException("消息内容不能超过2000个字符");

        var conversation = await _db.SupportConversations
            .Include(c => c.User)
            .Include(c => c.Admin)
            .SingleOrDefaultAsync(c => c.Id == request.ConversationId)
            ?? throw new BusinessException("客服会话不存在");

        if (!isAdmin && conversation.UserId != actorId) throw new UnauthorizedAccessException();
        if (conversation.Status == "closed") throw new BusinessException("该客服会话已结束，请重新发起会话");

        if (isAdmin)
        {
            if (conversation.AdminId.HasValue && conversation.AdminId != actorId)
                throw new BusinessException("该会话已由其他客服接待");
            if (!conversation.AdminId.HasValue)
            {
                conversation.AdminId = actorId;
                conversation.Admin = await _db.Users.SingleAsync(u => u.Id == actorId);
            }
            conversation.Status = "active";
        }

        var clientMessageId = string.IsNullOrWhiteSpace(request.ClientMessageId)
            ? null
            : request.ClientMessageId.Trim();
        if (clientMessageId is not null)
        {
            var existing = await _db.SupportMessages.AsNoTracking()
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.SenderId == actorId && m.ClientMessageId == clientMessageId);
            if (existing is not null) return MapMessage(existing, conversation);
        }

        var now = DateTime.UtcNow;
        var message = new SupportMessage
        {
            ConversationId = conversation.Id,
            SenderId = actorId,
            Content = content,
            MessageType = request.MessageType == "system" ? "system" : "text",
            ClientMessageId = clientMessageId,
            IsRead = false,
            CreatedAt = now
        };
        _db.SupportMessages.Add(message);
        conversation.LastMessage = content.Length <= 500 ? content : content[..500];
        conversation.LastMessageAt = now;
        conversation.UpdatedAt = now;
        if (isAdmin) conversation.UserUnreadCount++;
        else conversation.AdminUnreadCount++;

        await _db.SaveChangesAsync();
        var savedMessage = await _db.SupportMessages.AsNoTracking()
            .Include(m => m.Sender)
            .SingleAsync(m => m.Id == message.Id);
        return MapMessage(savedMessage, conversation);
    }

    public async Task<AttachmentMessageResult> SendAttachmentMessageAsync(
        int actorId,
        bool isAdmin,
        UploadChatAttachmentRequest request,
        StoredChatAttachment attachment)
    {
        var caption = (request.Caption ?? string.Empty).Trim();
        if (caption.Length > 500) throw new BusinessException("附件说明不能超过500个字符");

        var conversation = await _db.SupportConversations
            .Include(c => c.User)
            .Include(c => c.Admin)
            .SingleOrDefaultAsync(c => c.Id == request.ConversationId)
            ?? throw new BusinessException("客服会话不存在");

        if (!isAdmin && conversation.UserId != actorId) throw new UnauthorizedAccessException();
        if (conversation.Status == "closed") throw new BusinessException("该客服会话已结束，请重新发起会话");

        if (isAdmin)
        {
            if (conversation.AdminId.HasValue && conversation.AdminId != actorId)
                throw new BusinessException("该会话已由其他客服接待");
            if (!conversation.AdminId.HasValue)
            {
                conversation.AdminId = actorId;
                conversation.Admin = await _db.Users.SingleAsync(u => u.Id == actorId);
            }
            conversation.Status = "active";
        }

        var clientMessageId = string.IsNullOrWhiteSpace(request.ClientMessageId)
            ? null
            : request.ClientMessageId.Trim();
        if (clientMessageId is not null)
        {
            var existing = await _db.SupportMessages.AsNoTracking()
                .Include(m => m.Sender)
                .Include(m => m.Conversation)
                .FirstOrDefaultAsync(m => m.ConversationId == request.ConversationId
                    && m.SenderId == actorId
                    && m.ClientMessageId == clientMessageId);
            if (existing is not null)
                return new AttachmentMessageResult(MapMessage(existing, existing.Conversation), false);
        }

        var now = DateTime.UtcNow;
        var content = caption.Length > 0 ? caption : attachment.OriginalName;
        var preview = attachment.MessageType == "image"
            ? $"[图片] {content}"
            : $"[文件] {attachment.OriginalName}";
        var message = new SupportMessage
        {
            ConversationId = conversation.Id,
            SenderId = actorId,
            Content = content,
            MessageType = attachment.MessageType,
            ClientMessageId = clientMessageId,
            AttachmentName = attachment.OriginalName,
            AttachmentStorageName = attachment.StorageName,
            AttachmentContentType = attachment.ContentType,
            AttachmentSize = attachment.Size,
            IsRead = false,
            CreatedAt = now
        };
        _db.SupportMessages.Add(message);
        conversation.LastMessage = preview.Length <= 500 ? preview : preview[..500];
        conversation.LastMessageAt = now;
        conversation.UpdatedAt = now;
        if (isAdmin) conversation.UserUnreadCount++;
        else conversation.AdminUnreadCount++;

        await _db.SaveChangesAsync();
        var savedMessage = await _db.SupportMessages.AsNoTracking()
            .Include(m => m.Sender)
            .SingleAsync(m => m.Id == message.Id);
        return new AttachmentMessageResult(MapMessage(savedMessage, conversation), true);
    }

    public async Task<ChatAttachmentDownload?> GetAttachmentAsync(int messageId, int actorId, bool isAdmin)
    {
        var message = await _db.SupportMessages.AsNoTracking()
            .Include(m => m.Conversation)
            .SingleOrDefaultAsync(m => m.Id == messageId);
        if (message is null
            || (!isAdmin && message.Conversation.UserId != actorId)
            || string.IsNullOrWhiteSpace(message.AttachmentStorageName)
            || string.IsNullOrWhiteSpace(message.AttachmentName)
            || string.IsNullOrWhiteSpace(message.AttachmentContentType)
            || !message.AttachmentSize.HasValue)
            return null;

        return new ChatAttachmentDownload(
            message.Id,
            message.AttachmentName,
            message.AttachmentStorageName,
            message.AttachmentContentType,
            message.AttachmentSize.Value);
    }

    public async Task<bool> MarkMessageReadAsync(int messageId, int actorId, bool isAdmin)
    {
        var message = await _db.SupportMessages.Include(m => m.Conversation).SingleOrDefaultAsync(m => m.Id == messageId);
        if (message is null || (!isAdmin && message.Conversation.UserId != actorId)) return false;
        if (message.SenderId == actorId) return true;
        message.IsRead = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkConversationReadAsync(int conversationId, int actorId, bool isAdmin)
    {
        var conversation = await _db.SupportConversations.SingleOrDefaultAsync(c => c.Id == conversationId);
        if (conversation is null || (!isAdmin && conversation.UserId != actorId)) return false;

        var unread = await _db.SupportMessages
            .Where(m => m.ConversationId == conversationId && m.SenderId != actorId && !m.IsRead)
            .ToListAsync();
        unread.ForEach(m => m.IsRead = true);
        if (isAdmin) conversation.AdminUnreadCount = 0;
        else conversation.UserUnreadCount = 0;
        conversation.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<ConversationDto?> AssignConversationAsync(int conversationId, int adminId)
    {
        var conversation = await ConversationQuery(true).SingleOrDefaultAsync(c => c.Id == conversationId);
        if (conversation is null || conversation.Status == "closed"
            || (conversation.AdminId.HasValue && conversation.AdminId != adminId)) return null;
        conversation.AdminId = adminId;
        conversation.Status = "active";
        conversation.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        conversation = await ConversationQuery().SingleAsync(c => c.Id == conversationId);
        return MapConversation(conversation, true);
    }

    public async Task<ConversationDto?> CloseConversationAsync(int conversationId, int adminId)
    {
        var conversation = await ConversationQuery(true).SingleOrDefaultAsync(c => c.Id == conversationId);
        if (conversation is null || (conversation.AdminId.HasValue && conversation.AdminId != adminId)) return null;
        conversation.AdminId ??= adminId;
        conversation.Status = "closed";
        conversation.ClosedAt = DateTime.UtcNow;
        conversation.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        conversation = await ConversationQuery().SingleAsync(c => c.Id == conversationId);
        return MapConversation(conversation, true);
    }

    public Task<int> GetUnreadCountAsync(int actorId, bool isAdmin) => isAdmin
        ? _db.SupportConversations.SumAsync(c => c.AdminUnreadCount)
        : _db.SupportConversations.Where(c => c.UserId == actorId).SumAsync(c => c.UserUnreadCount);

    public Task<bool> CanAccessConversationAsync(int conversationId, int actorId, bool isAdmin) => isAdmin
        ? _db.SupportConversations.AnyAsync(c => c.Id == conversationId)
        : _db.SupportConversations.AnyAsync(c => c.Id == conversationId && c.UserId == actorId);

    private IQueryable<SupportConversation> ConversationQuery(bool tracking = false)
    {
        var query = _db.SupportConversations.Include(c => c.User).Include(c => c.Admin).AsQueryable();
        return tracking ? query : query.AsNoTracking();
    }

    private static ConversationDto MapConversation(SupportConversation c, bool viewerIsAdmin) => new()
    {
        Id = c.Id,
        UserId = c.UserId,
        UserName = c.User?.Username ?? string.Empty,
        UserAvatar = c.User?.Avatar ?? string.Empty,
        AdminId = c.AdminId,
        AdminName = c.Admin?.Username ?? "欢雨客服",
        LastMessage = c.LastMessage ?? "",
        LastMessageTime = c.LastMessageAt,
        UnreadCount = viewerIsAdmin ? c.AdminUnreadCount : c.UserUnreadCount,
        Status = c.Status,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };

    private static MessageDto MapMessage(SupportMessage message, SupportConversation conversation)
    {
        var receiverId = message.SenderId == conversation.UserId
            ? conversation.AdminId ?? 0
            : conversation.UserId;
        return new MessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = message.Sender?.Username ?? string.Empty,
            SenderAvatar = message.Sender?.Avatar ?? string.Empty,
            ReceiverId = receiverId,
            Content = message.Content,
            MessageType = message.MessageType,
            IsRead = message.IsRead,
            CreatedAt = message.CreatedAt,
            ConversationId = message.ConversationId,
            AttachmentName = message.AttachmentName,
            AttachmentContentType = message.AttachmentContentType,
            AttachmentSize = message.AttachmentSize,
            AttachmentAvailable = !string.IsNullOrWhiteSpace(message.AttachmentStorageName),
            AttachmentUrl = string.IsNullOrWhiteSpace(message.AttachmentStorageName)
                ? null
                : $"/api/chat/messages/{message.Id}/attachment"
        };
    }
}
