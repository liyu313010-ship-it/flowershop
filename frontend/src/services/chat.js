import api from './api'

class ChatService {
  // 获取消息列表
  async getMessages(conversationId, page = 1, pageSize = 20) {
    return api.get(`/chat/messages`, {
      params: {
        conversationId,
        page,
        pageSize
      },
      silent: true
    })
  }

  // 发送消息
  async sendMessage(messageData) {
    return api.post('/chat/messages', messageData, { silent: true })
  }

  // 标记消息为已读
  async markAsRead(messageId) {
    return api.put(`/chat/messages/${messageId}/read`, null, { silent: true })
  }

  // 删除消息
  async deleteMessage(messageId) {
    return api.delete(`/chat/messages/${messageId}`, { silent: true })
  }

  // 获取会话列表
  async getConversations() {
    return api.get('/chat/conversations', { silent: true })
  }

  // 获取会话详情
  async getConversation(conversationId) {
    return api.get(`/chat/conversations/${conversationId}`, { silent: true })
  }

  // 创建或获取与管理员的会话
  async createOrGetAdminConversation() {
    return api.post('/chat/conversations/admin', null, { silent: true })
  }

  // 获取未读消息数量
  async getUnreadCount() {
    return api.get('/chat/unread-count', { silent: true })
  }

  // 标记会话所有消息为已读
  async markConversationAsRead(conversationId) {
    return api.put(`/chat/conversations/${conversationId}/read`, null, { silent: true })
  }
}

export default new ChatService()
