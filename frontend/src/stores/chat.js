import { defineStore } from 'pinia'
import chatService from '@/services/chat'
import { useUserStore } from './user'
import { notifyInfo } from '@/utils/notify'

export const useChatStore = defineStore('chat', {
  state: () => ({
    conversations: [],
    currentConversation: null,
    messages: [],
    unreadCount: 0,
    isLoading: false,
    error: null,
    hubConnection: null,
    isConnected: false
  }),

  getters: {
    // 获取当前会话的消息
    currentMessages: (state) => {
      if (!state.currentConversation) return []
      return state.messages.filter(msg => msg.conversationId === state.currentConversation.id)
    },

    // 按时间排序的消息
    sortedMessages: (state) => {
      return [...state.messages].sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt))
    },

    // 按时间排序的会话
    sortedConversations: (state) => {
      return [...state.conversations].sort((a, b) => new Date(b.updatedAt) - new Date(a.updatedAt))
    }
  },

  actions: {
    // 初始化聊天状态
    async initChat() {
      try {
        this.isLoading = true
        await this.fetchConversations()
        await this.fetchUnreadCount()
      } catch (error) {
        console.warn('初始化聊天失败:', error?.message || error)
      } finally {
        this.isLoading = false
      }
    },

    // 获取会话列表
    async fetchConversations() {
      try {
        this.isLoading = true
        const conversations = await chatService.getConversations()
        const list = conversations?.items || conversations?.Items || conversations
        this.conversations = Array.isArray(list) ? list : []
      } catch (error) {
        console.warn('获取会话列表失败:', error?.message || error)
        this.error = error.message
      } finally {
        this.isLoading = false
      }
    },

    // 获取消息列表
    async fetchMessages(conversationId) {
      try {
        this.isLoading = true
        const pageSize = 50
        let page = 1
        const all = []
        while (true) {
          const resp = await chatService.getMessages(conversationId, page, pageSize)
          const list = resp?.items || resp?.Items || resp || []
          const arr = Array.isArray(list) ? list : []
          if (arr.length === 0) break
          for (const m of arr) { all.push({ ...m, conversationId }) }
          if (arr.length < pageSize) break
          page += 1
          if (page > 20) break
        }
        this.messages = all
        await this.markConversationAsRead(conversationId)
      } catch (error) {
        console.warn('获取消息列表失败:', error?.message || error)
        this.error = error.message
      } finally {
        this.isLoading = false
      }
    },

    // 获取与管理员的会话
    async fetchAdminConversation() {
      try {
        this.isLoading = true
        const conversation = await chatService.createOrGetAdminConversation()
        this.currentConversation = conversation
        await this.fetchMessages(conversation.id)
        return conversation
      } catch (error) {
        console.warn('获取管理员会话失败:', error?.message || error)
        this.error = error.message
        // 前端回退：创建本地占位会话，避免界面卡住
        const userStore = useUserStore()
        const placeholder = {
          id: 0,
          title: '管理员客服',
          lastMessage: '您好，请留言，我们会尽快联系您',
          updatedAt: new Date().toISOString(),
          adminId: 1, // 默认管理员ID
          userId: userStore.user?.id || 0
        }
        this.currentConversation = placeholder
        // 消息列表为空
        this.messages = []
        // 尝试从会话列表中找到已存在的管理员会话并加载历史
        try {
          await this.fetchConversations()
          const conv = (this.conversations || []).find(c => {
            const a = c.adminId ?? c.AdminId
            return Number(a) === 1
          })
          if (conv && conv.id) {
            this.currentConversation = conv
            await this.fetchMessages(conv.id)
          }
        } catch {}
        return placeholder
      } finally {
        this.isLoading = false
      }
    },

    // 发送消息
    async sendMessage(content, receiverId) {
      try {
        this.isLoading = true
        const messageData = {
          content,
          receiverId,
          conversationId: this.currentConversation?.id
        }

        const message = await chatService.sendMessage(messageData)
        // 使用后端返回的完整数据（包含正确的 conversationId）
        const data = { ...message }
        // 如果本地消息列表还是空的或者ID不匹配，确保我们使用后端返回的ConversationId
        if (this.currentConversation && (this.currentConversation.id === 0 || !this.currentConversation.id)) {
           // 更新当前会话ID，避免后续消息仍然使用 0
           this.currentConversation.id = data.conversationId
        }
        
        this.messages.push(data)
        
        // 更新会话的最后消息
        const convId = this.currentConversation?.id || data.conversationId
        const conversationIndex = this.conversations.findIndex(c => c.id === convId)
        if (conversationIndex !== -1) {
          this.conversations[conversationIndex].lastMessage = data.content
          this.conversations[conversationIndex].updatedAt = data.createdAt
        }

        return data
      } catch (error) {
        console.warn('发送消息失败:', error?.message || error)
        this.error = error.message
        throw error
      } finally {
        this.isLoading = false
      }
    },

    // 标记消息为已读
    async markAsRead(messageId) {
      try {
        await chatService.markAsRead(messageId)
        const messageIndex = this.messages.findIndex(m => m.id === messageId)
        if (messageIndex !== -1) {
          this.messages[messageIndex].isRead = true
        }
        await this.fetchUnreadCount()
      } catch (error) {
        console.warn('标记消息为已读失败:', error?.message || error)
      }
    },

    // 标记会话所有消息为已读
    async markConversationAsRead(conversationId) {
      try {
        await chatService.markConversationAsRead(conversationId)
        this.messages.forEach(msg => {
          if (msg.conversationId === conversationId && !msg.isRead) {
            msg.isRead = true
          }
        })
        await this.fetchUnreadCount()
      } catch (error) {
        console.warn('标记会话为已读失败:', error?.message || error)
      }
    },

    // 获取未读消息数量
    async fetchUnreadCount() {
      try {
        const count = await chatService.getUnreadCount()
        this.unreadCount = typeof count?.unreadCount === 'number' ? count.unreadCount : (count || 0)
      } catch (error) {
        console.warn('获取未读消息数量失败:', error?.message || error)
      }
    },

    // 设置当前会话
    setCurrentConversation(conversation) {
      this.currentConversation = conversation
      if (conversation) {
        this.fetchMessages(conversation.id)
      }
    },

    // 添加新消息
    addNewMessage(message) {
      // 检查消息是否已存在
      const existingMessage = this.messages.find(m => m.id === message.id)
      if (existingMessage) return

      this.messages.push(message)

      // 更新会话
      const conversationIndex = this.conversations.findIndex(c => c.id === message.conversationId)
      if (conversationIndex !== -1) {
        this.conversations[conversationIndex].lastMessage = message.content
        this.conversations[conversationIndex].updatedAt = message.createdAt
        this.conversations[conversationIndex].unreadCount = message.isRead ? 0 : (this.conversations[conversationIndex].unreadCount || 0) + 1
      } else {
        // 如果是新会话，添加到会话列表
        this.fetchConversations()
      }

      // 更新未读计数
      if (!message.isRead && message.receiverId === useUserStore().user.id) {
        this.unreadCount += 1
        try { notifyInfo('你有一条消息未读') } catch {}
      }
    },

    // 清除错误
    clearError() {
      this.error = null
    },

    // 重置聊天状态
    reset() {
      this.conversations = []
      this.currentConversation = null
      this.messages = []
      this.unreadCount = 0
      this.error = null
      this.disconnectHub()
    },

    // 连接SignalR Hub
    async connectHub() {
      try {
        if (this.hubConnection) {
          return
        }

        const { HubConnectionBuilder } = await import('@microsoft/signalr')
        const userStore = useUserStore()

        const hubUrl = `/hubs/chat`
        this.hubConnection = new HubConnectionBuilder()
          .withUrl(hubUrl, {
            accessTokenFactory: () => localStorage.getItem('token') || ''
          })
          .withAutomaticReconnect()
          .build()

        // 注册接收消息事件
        this.hubConnection.on('ReceiveMessage', (message) => {
          this.addNewMessage(message)
        })

        // 注册接收未读计数事件
        this.hubConnection.on('ReceiveUnreadCount', (count) => {
          this.unreadCount = count
        })

        // 连接状态变化
        this.hubConnection.onreconnected(() => {
          this.isConnected = true
          console.log('SignalR 重新连接成功')
        })

        this.hubConnection.onreconnecting(() => {
          this.isConnected = false
          console.log('SignalR 正在重新连接...')
        })

        this.hubConnection.onclose(() => {
          this.isConnected = false
          console.log('SignalR 连接已关闭')
        })

        // 启动连接
        await this.hubConnection.start()
        this.isConnected = true
        console.log('SignalR 连接成功')

        // 获取未读消息数量
        await this.fetchUnreadCount()
      } catch (error) {
        console.warn('SignalR 连接失败，进入静默模式:', error?.message || error)
        this.isConnected = false
      }
    },

    // 断开SignalR Hub连接
    disconnectHub() {
      if (this.hubConnection) {
        this.hubConnection.stop()
        this.hubConnection = null
        this.isConnected = false
      }
    }
  }
})
