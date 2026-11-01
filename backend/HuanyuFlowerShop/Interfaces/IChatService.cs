using HuanyuFlowerShop.DTOs;
using HuanyuFlowerShop.Entities;

namespace HuanyuFlowerShop.Interfaces
{
    /// <summary>
    /// 聊天服务接口
    /// </summary>
    public interface IChatService
    {
        #region 消息相关
        /// <summary>
        /// 获取会话消息
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="conversationId">会话ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>消息列表</returns>
        Task<PagedResult<MessageDto>> GetMessagesAsync(int userId, int conversationId, int page = 1, int pageSize = 50);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="senderId">发送者ID</param>
        /// <param name="request">发送消息请求</param>
        /// <returns>发送的消息</returns>
        Task<MessageDto> SendMessageAsync(int senderId, SendMessageRequest request);

        /// <summary>
        /// 标记消息已读
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="conversationId">会话ID</param>
        /// <returns>是否成功</returns>
        Task<bool> MarkMessagesAsReadAsync(int userId, int conversationId);

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="messageId">消息ID</param>
        /// <returns>是否成功</returns>
        Task<bool> DeleteMessageAsync(int userId, int messageId);
        #endregion

        #region 会话相关
        /// <summary>
        /// 获取用户的会话列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>会话列表</returns>
        Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(int userId);

        /// <summary>
        /// 获取会话详情
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="conversationId">会话ID</param>
        /// <returns>会话详情</returns>
        Task<ConversationDto?> GetConversationAsync(int userId, int conversationId);

        /// <summary>
        /// 创建或获取与管理员的会话
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>会话详情</returns>
        Task<ConversationDto?> CreateOrGetAdminConversationAsync(int userId);

        /// <summary>
        /// 获取会话未读消息数
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="conversationId">会话ID</param>
        /// <returns>未读消息数</returns>
        Task<int> GetUnreadCountAsync(int userId, int conversationId);

        /// <summary>
        /// 获取用户所有未读消息总数
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>未读消息总数</returns>
        Task<int> GetAllUnreadMessagesCountAsync(int userId);
        #endregion
    }
}
