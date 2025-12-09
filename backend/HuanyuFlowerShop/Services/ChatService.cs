using HuanyuFlowerShop.Data;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HuanyuFlowerShop.Services
{
    /// <summary>
    /// 聊天服务实现
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ChatService> _logger;

        public ChatService(ApplicationDbContext dbContext, ILogger<ChatService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        #region 消息相关
        /// <inheritdoc/>
        public async Task<PagedResult<MessageDto>> GetMessagesAsync(int userId, int conversationId, int page = 1, int pageSize = 50)
        {
            // 验证会话是否存在且用户有权访问
            var conversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && (c.UserId == userId || c.AdminId == userId));

            if (conversation == null)
            {
                return new PagedResult<MessageDto>();
            }

            // 查询消息
            var query = _dbContext.Messages
                .Where(m => 
                    (m.SenderId == userId && m.ReceiverId == conversation.UserId) ||
                    (m.SenderId == userId && m.ReceiverId == conversation.AdminId) ||
                    (m.SenderId == conversation.UserId && m.ReceiverId == userId) ||
                    (m.SenderId == conversation.AdminId && m.ReceiverId == userId))
                .OrderByDescending(m => m.CreatedAt);

            // 计算总数
            var totalCount = await query.CountAsync();

            // 分页查询
            var messages = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(m => m.CreatedAt) // 最终按时间正序返回
                .ToListAsync();

            // 转换为DTO
            var messageDtos = await ConvertMessagesToDtos(messages);

            return new PagedResult<MessageDto>
            {
                Items = messageDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <inheritdoc/>
        public async Task<MessageDto> SendMessageAsync(int senderId, SendMessageRequest request)
        {
            // 验证接收者是否存在
            var receiver = await _dbContext.Users.FindAsync(request.ReceiverId);
            if (receiver == null)
            {
                throw new ArgumentException("接收者不存在");
            }

            // 验证发送者是否存在
            var sender = await _dbContext.Users.FindAsync(senderId);
            if (sender == null)
            {
                throw new ArgumentException("发送者不存在");
            }

            // 创建消息
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                MessageType = request.MessageType,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();

            // 创建或更新会话
            await CreateOrUpdateConversationAsync(senderId, request.ReceiverId, request.Content);

            // 转换为DTO
            var messageDto = await ConvertMessageToDto(message);

            return messageDto;
        }

        /// <inheritdoc/>
        public async Task<bool> MarkMessagesAsReadAsync(int userId, int conversationId)
        {
            // 验证会话是否存在且用户有权访问
            var conversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && (c.UserId == userId || c.AdminId == userId));

            if (conversation == null)
            {
                return false;
            }

            // 查询未读消息
            var unreadMessages = await _dbContext.Messages
                .Where(m => m.ReceiverId == userId && 
                    ((m.SenderId == conversation.UserId && m.ReceiverId == conversation.AdminId) ||
                     (m.SenderId == conversation.AdminId && m.ReceiverId == conversation.UserId)))
                .ToListAsync();

            // 标记为已读
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.UpdatedAt = DateTime.UtcNow;
            }

            // 重置未读计数
            conversation.UnreadCount = 0;
            conversation.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteMessageAsync(int userId, int messageId)
        {
            // 查找消息
            var message = await _dbContext.Messages.FindAsync(messageId);
            if (message == null || (message.SenderId != userId && message.ReceiverId != userId))
            {
                return false;
            }

            // 删除消息
            _dbContext.Messages.Remove(message);
            await _dbContext.SaveChangesAsync();

            return true;
        }
        #endregion

        #region 会话相关
        /// <inheritdoc/>
        public async Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(int userId)
        {
            // 获取当前用户的角色
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null)
            {
                return Enumerable.Empty<ConversationDto>();
            }

            IQueryable<Conversation> query;
            if (string.Equals(user.Role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                // 管理员获取所有会话
                query = _dbContext.Conversations.OrderByDescending(c => c.UpdatedAt);
            }
            else
            {
                // 普通用户获取与管理员的会话
                query = _dbContext.Conversations
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.UpdatedAt);
            }

            var conversations = await query.ToListAsync();
            return await ConvertConversationsToDtos(conversations, userId);
        }

        /// <inheritdoc/>
        public async Task<ConversationDto?> GetConversationAsync(int userId, int conversationId)
        {
            // 查找会话
            var conversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && (c.UserId == userId || c.AdminId == userId));

            if (conversation == null)
            {
                return null;
            }

            // 转换为DTO
            var conversationDtos = await ConvertConversationsToDtos(new List<Conversation> { conversation }, userId);
            if (conversationDtos.Count == 0) { throw new Exception("会话创建失败"); }
            return conversationDtos[0];
        }

        /// <inheritdoc/>
        public async Task<ConversationDto?> CreateOrGetAdminConversationAsync(int userId)
        {
            // 查找管理员
            var admin = await _dbContext.Users.FirstOrDefaultAsync(u => string.Equals(u.Role, "admin", StringComparison.OrdinalIgnoreCase));
            if (admin == null)
            {
                throw new Exception("管理员不存在");
            }

            // 查找是否已存在会话
            var conversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c => c.UserId == userId && c.AdminId == admin.Id);

            if (conversation == null)
            {
                // 创建新会话
                conversation = new Conversation
                {
                    UserId = userId,
                    AdminId = admin.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Conversations.Add(conversation);
                await _dbContext.SaveChangesAsync();
            }

            // 转换为DTO
            var conversationDtos = await ConvertConversationsToDtos(new List<Conversation> { conversation }, userId);
            if (conversationDtos.Count == 0) { return null; }
            return conversationDtos[0];
        }

        /// <inheritdoc/>
        public async Task<int> GetUnreadCountAsync(int userId, int conversationId)
        {
            // 查找会话
            var conversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && (c.UserId == userId || c.AdminId == userId));

            if (conversation == null)
            {
                return 0;
            }

            // 计算未读消息数
            var unreadCount = await _dbContext.Messages
                .Where(m => m.ReceiverId == userId && 
                    ((m.SenderId == conversation.UserId && m.ReceiverId == conversation.AdminId) ||
                     (m.SenderId == conversation.AdminId && m.ReceiverId == conversation.UserId)) &&
                    !m.IsRead)
                .CountAsync();

            return unreadCount;
        }

        /// <inheritdoc/>
        public async Task<int> GetAllUnreadMessagesCountAsync(int userId)
        {
            // 计算用户的所有未读消息总数
            var unreadCount = await _dbContext.Messages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .CountAsync();

            return unreadCount;
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 创建或更新会话
        /// </summary>
        private async Task CreateOrUpdateConversationAsync(int senderId, int receiverId, string lastMessage)
        {
            // 获取当前用户和接收者的角色
            var sender = await _dbContext.Users.FindAsync(senderId);
            var receiver = await _dbContext.Users.FindAsync(receiverId);

            if (sender == null || receiver == null)
            {
                return;
            }

            // 确定用户ID和管理员ID
            int userId, adminId;
            if (string.Equals(sender.Role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                adminId = senderId;
                userId = receiverId;
            }
            else if (string.Equals(receiver.Role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                userId = senderId;
                adminId = receiverId;
            }
            else
            {
                // 如果双方都不是管理员，不创建会话
                return;
            }

            // 查找会话
            var conversation = await _dbContext.Conversations
                .FirstOrDefaultAsync(c => c.UserId == userId && c.AdminId == adminId);

            if (conversation == null)
            {
                // 创建新会话
                conversation = new Conversation
                {
                    UserId = userId,
                    AdminId = adminId,
                    LastMessage = lastMessage,
                    LastMessageTime = DateTime.UtcNow,
                    UnreadCount = 1, // 新消息，未读计数为1
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Conversations.Add(conversation);
            }
            else
            {
                // 更新现有会话
                conversation.LastMessage = lastMessage;
                conversation.LastMessageTime = DateTime.UtcNow;
                conversation.UpdatedAt = DateTime.UtcNow;

                // 如果接收者是管理员，增加未读计数
                if (receiverId == adminId)
                {
                    conversation.UnreadCount += 1;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 将消息转换为DTO
        /// </summary>
        private async Task<MessageDto> ConvertMessageToDto(Message message)
        {
            var sender = await _dbContext.Users.FindAsync(message.SenderId);
            var receiver = await _dbContext.Users.FindAsync(message.ReceiverId);
            var conversationId = await GetConversationIdAsync(message.SenderId, message.ReceiverId);

            return new MessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = sender?.Username ?? "未知用户",
                SenderAvatar = sender?.Avatar ?? string.Empty,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                MessageType = message.MessageType,
                IsRead = message.IsRead,
                CreatedAt = message.CreatedAt,
                ConversationId = conversationId
            };
        }

        /// <summary>
        /// 将消息列表转换为DTO列表
        /// </summary>
        private async Task<List<MessageDto>> ConvertMessagesToDtos(List<Message> messages)
        {
            var dtos = new List<MessageDto>();
            foreach (var message in messages)
            {
                var dto = await ConvertMessageToDto(message);
                dtos.Add(dto);
            }
            return dtos;
        }

        /// <summary>
        /// 将会话转换为DTO
        /// </summary>
        private async Task<List<ConversationDto>> ConvertConversationsToDtos(List<Conversation> conversations, int currentUserId)
        {
            var dtos = new List<ConversationDto>();

            foreach (var conversation in conversations)
            {
                var user = await _dbContext.Users.FindAsync(conversation.UserId);
                var admin = await _dbContext.Users.FindAsync(conversation.AdminId);

                var dto = new ConversationDto
                {
                    Id = conversation.Id,
                    UserId = conversation.UserId,
                    UserName = user?.Username ?? "未知用户",
                    UserAvatar = user?.Avatar ?? string.Empty,
                    AdminId = conversation.AdminId,
                    AdminName = admin?.Username ?? "未知管理员",
                    LastMessage = conversation.LastMessage,
                    LastMessageTime = conversation.LastMessageTime,
                    UnreadCount = conversation.UnreadCount,
                    CreatedAt = conversation.CreatedAt,
                    UpdatedAt = conversation.UpdatedAt
                };

                dtos.Add(dto);
            }

            return dtos;
        }

        /// <summary>
        /// 获取会话ID（基于发送者与接收者的角色）
        /// </summary>
        private async Task<int> GetConversationIdAsync(int senderId, int receiverId)
        {
            var sender = await _dbContext.Users.FindAsync(senderId);
            var receiver = await _dbContext.Users.FindAsync(receiverId);
            if (sender == null || receiver == null)
            {
                return 0;
            }

            int userId, adminId;
            if (string.Equals(sender.Role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                adminId = senderId;
                userId = receiverId;
            }
            else if (string.Equals(receiver.Role, "admin", StringComparison.OrdinalIgnoreCase))
            {
                userId = senderId;
                adminId = receiverId;
            }
            else
            {
                return 0;
            }

            var conv = await _dbContext.Conversations
                .FirstOrDefaultAsync(c => c.UserId == userId && c.AdminId == adminId);
            return conv?.Id ?? 0;
        }
        #endregion
    }
}
