using System.Security.Claims;
using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using HuanyuFlowerShop.Hubs;

namespace HuanyuFlowerShop.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IHubContext<ChatHub> _hub;

    public ChatController(IChatService chatService, IHubContext<ChatHub> hub)
    {
        _chatService = chatService;
        _hub = hub;
    }

    [HttpPost("conversations/admin")]
    public async Task<ActionResult<ConversationDto>> GetOrCreateSupportConversation()
    {
        if (IsAdmin) return BadRequest(new { message = "管理员请从消息中心选择用户会话" });
        var conversation = await _chatService.GetOrCreateSupportConversationAsync(UserId);
        await _hub.Clients.Group("admins").SendAsync("ConversationUpdated", conversation);
        return Ok(conversation);
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> GetConversations([FromQuery] string? status = null) =>
        Ok(await _chatService.GetConversationsAsync(UserId, IsAdmin, status));

    [HttpGet("conversations/{conversationId:int}")]
    public async Task<ActionResult<ConversationDto>> GetConversation(int conversationId)
    {
        var conversation = await _chatService.GetConversationAsync(conversationId, UserId, IsAdmin);
        return conversation is null ? NotFound() : Ok(conversation);
    }

    [HttpGet("messages")]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> GetMessages(
        [FromQuery] int conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (!await _chatService.CanAccessConversationAsync(conversationId, UserId, IsAdmin)) return Forbid();
        return Ok(await _chatService.GetMessagesAsync(conversationId, UserId, IsAdmin, page, pageSize));
    }

    [HttpPost("messages")]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] SendMessageRequest request)
    {
        if (!await _chatService.CanAccessConversationAsync(request.ConversationId, UserId, IsAdmin)) return Forbid();
        var message = await _chatService.SendMessageAsync(UserId, IsAdmin, request);
        var conversation = await _chatService.GetConversationAsync(request.ConversationId, UserId, IsAdmin);

        await _hub.Clients.Group($"conversation:{request.ConversationId}").SendAsync("ReceiveMessage", message);
        if (conversation is not null)
        {
            if (IsAdmin)
                await _hub.Clients.Group($"user:{conversation.UserId}").SendAsync("ReceiveMessage", message);
            else
                await _hub.Clients.Group("admins").SendAsync("ReceiveMessage", message);
            await _hub.Clients.Group("admins").SendAsync("ConversationUpdated", conversation);
            await _hub.Clients.Group($"user:{conversation.UserId}").SendAsync("ConversationUpdated", conversation);
        }
        return Ok(message);
    }

    [HttpPut("messages/{messageId:int}/read")]
    public async Task<IActionResult> MarkMessageRead(int messageId) =>
        await _chatService.MarkMessageReadAsync(messageId, UserId, IsAdmin) ? NoContent() : NotFound();

    [HttpPut("conversations/{conversationId:int}/read")]
    public async Task<IActionResult> MarkConversationRead(int conversationId)
    {
        if (!await _chatService.CanAccessConversationAsync(conversationId, UserId, IsAdmin)) return Forbid();
        await _chatService.MarkConversationReadAsync(conversationId, UserId, IsAdmin);
        return NoContent();
    }

    [HttpPut("conversations/{conversationId:int}/assign")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ConversationDto>> AssignConversation(int conversationId)
    {
        var conversation = await _chatService.AssignConversationAsync(conversationId, UserId);
        if (conversation is null) return NotFound();
        await _hub.Clients.Group($"user:{conversation.UserId}").SendAsync("ConversationUpdated", conversation);
        await _hub.Clients.Group("admins").SendAsync("ConversationUpdated", conversation);
        return Ok(conversation);
    }

    [HttpPut("conversations/{conversationId:int}/close")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ConversationDto>> CloseConversation(int conversationId)
    {
        var conversation = await _chatService.CloseConversationAsync(conversationId, UserId);
        if (conversation is null) return NotFound();
        await _hub.Clients.Group($"user:{conversation.UserId}").SendAsync("ConversationUpdated", conversation);
        await _hub.Clients.Group("admins").SendAsync("ConversationUpdated", conversation);
        return Ok(conversation);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<object>> GetUnreadCount() =>
        Ok(new { unreadCount = await _chatService.GetUnreadCountAsync(UserId, IsAdmin) });

    private int UserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id
        : throw new UnauthorizedAccessException();
    private bool IsAdmin => User.IsInRole("admin");
}
