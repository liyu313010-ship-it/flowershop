using System.Security.Claims;
using HuanyuFlowerShop.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HuanyuFlowerShop.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService) => _chatService = chatService;

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        if (Context.User?.IsInRole("admin") == true)
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        await base.OnConnectedAsync();
    }

    public async Task JoinConversation(int conversationId)
    {
        var userId = GetUserId();
        var isAdmin = Context.User?.IsInRole("admin") == true;
        if (!await _chatService.CanAccessConversationAsync(conversationId, userId, isAdmin))
            throw new HubException("无权加入该客服会话");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation:{conversationId}");
    }

    public Task LeaveConversation(int conversationId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation:{conversationId}");

    private int GetUserId()
    {
        var value = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(value, out var userId)) throw new HubException("登录状态无效");
        return userId;
    }
}
