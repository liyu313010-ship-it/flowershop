import { normalizeConversation, normalizeMessage } from '@/utils/chatData'

describe('客服数据兼容处理', () => {
  it('兼容后端 PascalCase 会话响应', () => {
    expect(normalizeConversation({
      Id: 12,
      UserId: 3,
      UserName: '小雨',
      AdminId: 1,
      UnreadCount: 2,
      Status: 'active'
    })).toMatchObject({
      id: 12,
      userId: 3,
      userName: '小雨',
      adminId: 1,
      unreadCount: 2,
      status: 'active'
    })
  })

  it('保留 camelCase 消息并补充会话编号', () => {
    expect(normalizeMessage({
      id: 8,
      senderId: 5,
      content: '请问今天可以送到吗？',
      isRead: false
    }, 16)).toMatchObject({
      id: 8,
      senderId: 5,
      conversationId: 16,
      content: '请问今天可以送到吗？',
      isRead: false,
      messageType: 'text'
    })
  })

  it('空数据返回安全默认值', () => {
    expect(normalizeConversation()).toMatchObject({ id: 0, unreadCount: 0, status: 'waiting' })
    expect(normalizeMessage()).toMatchObject({ id: 0, conversationId: 0, content: '' })
  })

  it('规范化聊天附件元数据', () => {
    expect(normalizeMessage({
      Id: 9,
      MessageType: 'image',
      AttachmentName: '花束.jpg',
      AttachmentContentType: 'image/jpeg',
      AttachmentSize: 2048,
      AttachmentAvailable: true,
      AttachmentUrl: '/api/chat/messages/9/attachment'
    })).toMatchObject({
      id: 9,
      messageType: 'image',
      attachmentName: '花束.jpg',
      attachmentSize: 2048,
      attachmentAvailable: true
    })
  })
})
