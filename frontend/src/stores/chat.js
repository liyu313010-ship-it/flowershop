import { defineStore } from 'pinia'
import chatService from '@/services/chat'
import { useUserStore } from './user'
import { notifyInfo } from '@/utils/notify'
import { normalizeConversation, normalizeMessage } from '@/utils/chatData'

export const useChatStore = defineStore('chat', {
  state: () => ({
    conversations: [],
    currentConversation: null,
    messages: [],
    unreadCount: 0,
    isLoading: false,
    error: null,
    hubConnection: null,
    isConnected: false,
    unreadCountRetryAt: 0
  }),

  getters: {
    currentMessages: (state) => state.currentConversation
      ? state.messages.filter(message => message.conversationId === state.currentConversation.id)
      : [],
    sortedMessages() {
      return [...this.currentMessages].sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt))
    },
    sortedConversations: (state) => [...state.conversations].sort((a, b) =>
      new Date(b.lastMessageTime || b.updatedAt || 0) - new Date(a.lastMessageTime || a.updatedAt || 0))
  },

  actions: {
    async initChat() {
      this.error = null
      await Promise.all([this.fetchConversations(), this.fetchUnreadCount()])
    },

    async fetchConversations() {
      try {
        const response = await chatService.getConversations()
        const list = response?.items ?? response?.Items ?? response
        this.conversations = (Array.isArray(list) ? list : []).map(normalizeConversation)
        if (this.currentConversation) {
          this.currentConversation = this.conversations.find(c => c.id === this.currentConversation.id)
            || this.currentConversation
        }
        return this.conversations
      } catch (error) {
        this.error = error?.response?.data?.message || error.message || '会话加载失败'
        throw error
      }
    },

    async fetchMessages(conversationId) {
      if (!conversationId) return []
      try {
        this.isLoading = true
        const all = []
        for (let page = 1; page <= 20; page += 1) {
          const response = await chatService.getMessages(conversationId, page, 50)
          const list = response?.items ?? response?.Items ?? response
          const messages = (Array.isArray(list) ? list : []).map(item => normalizeMessage(item, conversationId))
          all.push(...messages)
          if (messages.length < 50) break
        }
        this.messages = [
          ...this.messages.filter(message => message.conversationId !== Number(conversationId)),
          ...all
        ]
        await this.markConversationAsRead(conversationId)
        return all
      } catch (error) {
        this.error = error?.response?.data?.message || error.message || '消息加载失败'
        throw error
      } finally {
        this.isLoading = false
      }
    },

    async fetchAdminConversation() {
      try {
        this.isLoading = true
        const conversation = normalizeConversation(await chatService.createOrGetAdminConversation())
        this.upsertConversation(conversation)
        await this.setCurrentConversation(conversation)
        return conversation
      } catch (error) {
        this.error = error?.response?.data?.message || error.message || '暂时无法连接客服'
        throw error
      } finally {
        this.isLoading = false
      }
    },

    async sendMessage(content) {
      if (!this.currentConversation?.id) throw new Error('请先选择客服会话')
      const clientMessageId = globalThis.crypto?.randomUUID?.()
        || `${Date.now()}-${Math.random().toString(16).slice(2)}`
      const response = await chatService.sendMessage({
        conversationId: this.currentConversation.id,
        content: content.trim(),
        messageType: 'text',
        clientMessageId
      })
      const message = normalizeMessage(response, this.currentConversation.id)
      this.addNewMessage(message, false)
      return message
    },

    async sendAttachment(file, caption = '', onProgress) {
      if (!this.currentConversation?.id) throw new Error('请先选择客服会话')
      const clientMessageId = globalThis.crypto?.randomUUID?.()
        || `${Date.now()}-${Math.random().toString(16).slice(2)}`
      const formData = new FormData()
      formData.append('conversationId', String(this.currentConversation.id))
      formData.append('file', file)
      if (caption.trim()) formData.append('caption', caption.trim())
      formData.append('clientMessageId', clientMessageId)
      const response = await chatService.sendAttachment(formData, event => {
        if (event.total && onProgress) onProgress(Math.round((event.loaded * 100) / event.total))
      })
      const message = normalizeMessage(response, this.currentConversation.id)
      this.addNewMessage(message, false)
      return message
    },

    async markAsRead(messageId) {
      await chatService.markAsRead(messageId)
      const message = this.messages.find(item => item.id === messageId)
      if (message) message.isRead = true
      await this.fetchUnreadCount()
    },

    async markConversationAsRead(conversationId) {
      await chatService.markConversationAsRead(conversationId)
      const userId = Number(useUserStore().user?.id || 0)
      this.messages.forEach(message => {
        if (message.conversationId === Number(conversationId) && message.senderId !== userId) message.isRead = true
      })
      const conversation = this.conversations.find(item => item.id === Number(conversationId))
      if (conversation) conversation.unreadCount = 0
      await this.fetchUnreadCount()
    },

    async fetchUnreadCount() {
      if (Date.now() < this.unreadCountRetryAt) return this.unreadCount
      try {
        const response = await chatService.getUnreadCount()
        this.unreadCount = Number(response?.unreadCount ?? response?.UnreadCount ?? response ?? 0)
        this.unreadCountRetryAt = 0
      } catch (error) {
        if (error?.response?.status === 429) {
          const retryAfter = Number(error.response?.headers?.['x-ratelimit-retry-after'] || 30)
          this.unreadCountRetryAt = Date.now() + Math.min(Math.max(retryAfter, 30), 120) * 1000
        }
      }
      return this.unreadCount
    },

    async assignConversation(conversationId) {
      const conversation = normalizeConversation(await chatService.assignConversation(conversationId))
      this.upsertConversation(conversation)
      this.currentConversation = conversation
      return conversation
    },

    async closeConversation(conversationId) {
      const conversation = normalizeConversation(await chatService.closeConversation(conversationId))
      this.upsertConversation(conversation)
      this.currentConversation = conversation
      return conversation
    },

    async setCurrentConversation(conversation) {
      const previousId = this.currentConversation?.id
      const normalized = conversation ? normalizeConversation(conversation) : null
      this.currentConversation = normalized
      if (this.isConnected && previousId && previousId !== normalized?.id) {
        try { await this.hubConnection.invoke('LeaveConversation', previousId) } catch {}
      }
      if (normalized?.id) {
        if (this.isConnected) {
          try { await this.hubConnection.invoke('JoinConversation', normalized.id) } catch {}
        }
        await this.fetchMessages(normalized.id)
      }
    },

    upsertConversation(conversation) {
      const normalized = normalizeConversation(conversation)
      const index = this.conversations.findIndex(item => item.id === normalized.id)
      if (index === -1) this.conversations.unshift(normalized)
      else this.conversations[index] = { ...this.conversations[index], ...normalized }
      if (this.currentConversation?.id === normalized.id) {
        this.currentConversation = { ...this.currentConversation, ...normalized }
      }
    },

    addNewMessage(message, notify = true) {
      const normalized = normalizeMessage(message)
      if (this.messages.some(item => item.id === normalized.id)) return
      this.messages.push(normalized)
      const conversation = this.conversations.find(item => item.id === normalized.conversationId)
      if (conversation) {
        conversation.lastMessage = normalized.content
        conversation.lastMessageTime = normalized.createdAt
        conversation.updatedAt = normalized.createdAt
      } else {
        this.fetchConversations().catch(() => {})
      }

      const userId = Number(useUserStore().user?.id || 0)
      const isCurrent = this.currentConversation?.id === normalized.conversationId
      if (normalized.senderId !== userId && isCurrent) {
        this.markConversationAsRead(normalized.conversationId).catch(() => {})
      } else if (normalized.senderId !== userId) {
        this.unreadCount += 1
        if (conversation) conversation.unreadCount += 1
        if (notify) notifyInfo('收到一条新的客服消息')
      }
    },

    async connectHub() {
      if (!localStorage.getItem('token')) return
      const { HubConnectionBuilder, HubConnectionState, LogLevel } = await import('@microsoft/signalr')
      if (this.hubConnection?.state === HubConnectionState.Connected) return
      if (!this.hubConnection) {
        this.hubConnection = new HubConnectionBuilder()
          .withUrl('/hubs/chat', { accessTokenFactory: () => localStorage.getItem('token') || '' })
          .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
          .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.Warning)
          .build()

        this.hubConnection.on('ReceiveMessage', message => this.addNewMessage(message))
        this.hubConnection.on('ConversationUpdated', conversation => this.upsertConversation(conversation))
        this.hubConnection.on('ReceiveUnreadCount', count => { this.unreadCount = Number(count || 0) })
        this.hubConnection.onreconnecting(() => { this.isConnected = false })
        this.hubConnection.onreconnected(async () => {
          this.isConnected = true
          if (this.currentConversation?.id) {
            try { await this.hubConnection.invoke('JoinConversation', this.currentConversation.id) } catch {}
          }
          await this.initChat().catch(() => {})
        })
        this.hubConnection.onclose(() => { this.isConnected = false })
      }
      if (this.hubConnection.state === HubConnectionState.Disconnected) {
        await this.hubConnection.start()
        this.isConnected = true
      }
    },

    async disconnectHub() {
      if (this.hubConnection) await this.hubConnection.stop().catch(() => {})
      this.hubConnection = null
      this.isConnected = false
    },

    reset() {
      this.disconnectHub()
      this.conversations = []
      this.currentConversation = null
      this.messages = []
      this.unreadCount = 0
      this.unreadCountRetryAt = 0
      this.error = null
    }
  }
})
