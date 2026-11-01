using Microsoft.AspNetCore.SignalR;
using HuanyuFlowerShop.Services;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;
using HuanyuFlowerShop.Interfaces;
using System.Security.Claims;

namespace HuanyuFlowerShop.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// 连接到聊天
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                if (role == "Admin")
                {
                    // 管理员加入所有用户的聊天组
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                }
                else
                {
                    // 普通用户加入自己的聊天组
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                }
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                if (role == "Admin")
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
                }
                else
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="messageDto">消息DTO</param>
        public async Task SendMessage(MessageDto messageDto)
        {
            // 保存消息到数据库
            var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId))
            {
                throw new UnauthorizedAccessException("无法获取发送者ID");
            }

            var request = new SendMessageRequest
            {
                ReceiverId = messageDto.ReceiverId,
                Content = messageDto.Content,
                MessageType = messageDto.MessageType
            };

            var message = await _chatService.SendMessageAsync(int.Parse(senderId), request);

            // 发送消息给接收者
            if (message.ReceiverId == 1) // 管理员ID固定为1
            {
                await Clients.Group("Admins").SendAsync("ReceiveMessage", message);
            }
            else
            {
                await Clients.Group($"User_{message.ReceiverId}").SendAsync("ReceiveMessage", message);
            }

            // 发送消息给发送者（确认）
            await Clients.Caller.SendAsync("ReceiveMessage", message);
        }

        /// <summary>
        /// 获取未读消息数量
        /// </summary>
        public async Task GetUnreadCount()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var count = await _chatService.GetAllUnreadMessagesCountAsync(int.Parse(userId));
                await Clients.Caller.SendAsync("ReceiveUnreadCount", count);
            }
        }

        /// <summary>
        /// 标记消息为已读
        /// </summary>
        public async Task MarkAsRead(int conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _chatService.MarkMessagesAsReadAsync(int.Parse(userId), conversationId);
            }
        }
    }
}