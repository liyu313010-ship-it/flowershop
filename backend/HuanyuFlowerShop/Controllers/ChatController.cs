using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HuanyuFlowerShop.Controllers
{
    /// <summary>
    /// 聊天控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        #region 消息相关API
        /// <summary>
        /// 获取会话消息
        /// </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>消息列表</returns>
        [HttpGet("messages/{conversationId}")]
        public async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                int userId = GetCurrentUserId();
                var messages = await _chatService.GetMessagesAsync(userId, conversationId, page, pageSize);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取消息失败，会话ID: {ConversationId}", conversationId);
                return StatusCode(500, new { message = "获取消息失败" });
            }
        }

        /// <summary>
        /// 获取会话消息（查询参数形式）
        /// </summary>
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessagesByQuery([FromQuery] int conversationId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                int userId = GetCurrentUserId();
                var messages = await _chatService.GetMessagesAsync(userId, conversationId, page, pageSize);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取消息失败，会话ID: {ConversationId}", conversationId);
                return StatusCode(500, new { message = "获取消息失败" });
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="request">发送消息请求</param>
        /// <returns>发送的消息</returns>
        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int userId = GetCurrentUserId();
                var message = await _chatService.SendMessageAsync(userId, request);
                return Ok(message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送消息失败");
                return StatusCode(500, new { message = "发送消息失败" });
            }
        }

        /// <summary>
        /// 标记消息已读
        /// </summary>
        /// <param name="conversationId">会话ID</param>
        /// <returns>操作结果</returns>
        [HttpPut("messages/mark-as-read/{conversationId}")]
        public async Task<IActionResult> MarkMessagesAsRead(int conversationId)
        {
            try
            {
                int userId = GetCurrentUserId();
                bool result = await _chatService.MarkMessagesAsReadAsync(userId, conversationId);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "标记消息已读失败，会话ID: {ConversationId}", conversationId);
                return StatusCode(500, new { message = "标记消息已读失败" });
            }
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="messageId">消息ID</param>
        /// <returns>操作结果</returns>
        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                int userId = GetCurrentUserId();
                bool result = await _chatService.DeleteMessageAsync(userId, messageId);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除消息失败，消息ID: {MessageId}", messageId);
                return StatusCode(500, new { message = "删除消息失败" });
            }
        }
        #endregion

        #region 会话相关API
        /// <summary>
        /// 获取用户会话列表
        /// </summary>
        /// <returns>会话列表</returns>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                int userId = GetCurrentUserId();
                var conversations = await _chatService.GetUserConversationsAsync(userId);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取会话列表失败");
                return StatusCode(500, new { message = "获取会话列表失败" });
            }
        }

        /// <summary>
        /// 获取会话详情
        /// </summary>
        /// <param name="conversationId">会话ID</param>
        /// <returns>会话详情</returns>
        [HttpGet("conversations/{conversationId}")]
        public async Task<IActionResult> GetConversation(int conversationId)
        {
            try
            {
                int userId = GetCurrentUserId();
                var conversation = await _chatService.GetConversationAsync(userId, conversationId);
                if (conversation == null)
                {
                    return NotFound(new { message = "会话不存在或无权访问" });
                }
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取会话详情失败，会话ID: {ConversationId}", conversationId);
                return StatusCode(500, new { message = "获取会话详情失败" });
            }
        }

        /// <summary>
        /// 创建或获取与管理员的会话
        /// </summary>
        /// <returns>会话详情</returns>
        [HttpPost("conversations/admin")]
        public async Task<IActionResult> CreateOrGetAdminConversation()
        {
            try
            {
                int userId = GetCurrentUserId();
                var conversation = await _chatService.CreateOrGetAdminConversationAsync(userId);
                if (conversation == null)
                {
                    return NotFound(new { message = "管理员会话不存在" });
                }
                return Ok(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建或获取管理员会话失败");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// 获取会话未读消息数
        /// </summary>
        /// <param name="conversationId">会话ID</param>
        /// <returns>未读消息数</returns>
        [HttpGet("conversations/{conversationId}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(int conversationId)
        {
            try
            {
                int userId = GetCurrentUserId();
                int count = await _chatService.GetUnreadCountAsync(userId, conversationId);
                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取未读消息数失败，会话ID: {ConversationId}", conversationId);
                return StatusCode(500, new { message = "获取未读消息数失败" });
            }
        }

        /// <summary>
        /// 标记会话消息已读（与前端路径兼容）
        /// </summary>
        [HttpPut("conversations/{conversationId}/read")]
        public async Task<IActionResult> MarkConversationAsRead(int conversationId)
        {
            try
            {
                int userId = GetCurrentUserId();
                bool result = await _chatService.MarkMessagesAsReadAsync(userId, conversationId);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "标记会话已读失败，会话ID: {ConversationId}", conversationId);
                return StatusCode(500, new { message = "标记会话已读失败" });
            }
        }

        /// <summary>
        /// 获取用户所有未读消息总数
        /// </summary>
        /// <returns>未读消息总数</returns>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetAllUnreadCount()
        {
            try
            {
                int userId = GetCurrentUserId();
                int count = await _chatService.GetAllUnreadMessagesCountAsync(userId);
                return Ok(new { unreadCount = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有未读消息数失败");
                return StatusCode(500, new { message = "获取所有未读消息数失败" });
            }
        }
        #endregion

        #region 辅助方法
        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        /// <returns>用户ID</returns>
        private int GetCurrentUserId()
        {
            // 从JWT中获取用户ID
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("无法获取用户ID");
            }
            return int.Parse(userIdClaim);
        }
        #endregion
    }
}
