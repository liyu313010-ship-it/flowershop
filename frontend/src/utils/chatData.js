const valueOf = (object, camel, pascal, fallback = undefined) =>
  object?.[camel] ?? object?.[pascal] ?? fallback

export const normalizeConversation = (conversation = {}) => ({
  ...conversation,
  id: Number(valueOf(conversation, 'id', 'Id', 0)),
  userId: Number(valueOf(conversation, 'userId', 'UserId', 0)),
  userName: valueOf(conversation, 'userName', 'UserName', ''),
  userAvatar: valueOf(conversation, 'userAvatar', 'UserAvatar', ''),
  adminId: valueOf(conversation, 'adminId', 'AdminId', null),
  adminName: valueOf(conversation, 'adminName', 'AdminName', '欢雨客服'),
  lastMessage: valueOf(conversation, 'lastMessage', 'LastMessage', ''),
  lastMessageTime: valueOf(conversation, 'lastMessageTime', 'LastMessageTime', null),
  unreadCount: Number(valueOf(conversation, 'unreadCount', 'UnreadCount', 0)),
  status: valueOf(conversation, 'status', 'Status', 'waiting'),
  createdAt: valueOf(conversation, 'createdAt', 'CreatedAt', null),
  updatedAt: valueOf(conversation, 'updatedAt', 'UpdatedAt', null)
})

export const normalizeMessage = (message = {}, conversationId = 0) => ({
  ...message,
  id: Number(valueOf(message, 'id', 'Id', 0)),
  senderId: Number(valueOf(message, 'senderId', 'SenderId', 0)),
  receiverId: Number(valueOf(message, 'receiverId', 'ReceiverId', 0)),
  conversationId: Number(valueOf(message, 'conversationId', 'ConversationId', conversationId || 0)),
  content: valueOf(message, 'content', 'Content', ''),
  messageType: valueOf(message, 'messageType', 'MessageType', 'text'),
  isRead: Boolean(valueOf(message, 'isRead', 'IsRead', false)),
  createdAt: valueOf(message, 'createdAt', 'CreatedAt', null)
})
