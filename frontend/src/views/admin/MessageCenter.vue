<template>
  <div class="admin-message-center">
    <!-- 页面标题 -->
    <div class="page-header">
      <h1 class="text-2xl font-bold">消息中心</h1>
      <p class="text-gray-600">查看和回复用户消息</p>
    </div>

    <!-- 主内容区域 -->
    <div class="main-content">
      <!-- 左侧会话列表 -->
      <div class="conversation-list">
        <div class="conversation-list-header">
          <h2 class="text-lg font-semibold">会话列表</h2>
          <span v-if="chatStore.unreadCount > 0" class="unread-count-badge">
            {{ chatStore.unreadCount }} 条未读
          </span>
        </div>

        <!-- 会话列表内容 -->
        <div class="conversation-items">
          <div v-if="chatStore.isLoading" class="loading-state">
            <div class="spinner-border animate-spin inline-block w-4 h-4 border-4 rounded-full border-blue-500 border-t-transparent"></div>
            <span class="ml-2">加载中...</span>
          </div>
          <div v-else-if="chatStore.conversations.length === 0" class="empty-state">
            <svg class="w-16 h-16 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"></path>
            </svg>
            <p class="mt-4 text-gray-500">暂无会话</p>
          </div>
          <div v-else>
            <div
              v-for="conversation in chatStore.sortedConversations"
              :key="conversation.id"
              class="conversation-item"
              :class="{ 'active': currentConversationId === conversation.id, 'has-unread': conversation.unreadCount > 0 }"
              @click="selectConversation(conversation)"
            >
              <div class="conversation-avatar">
                <img v-if="getAvatar(conversation)"
                     :src="getAvatar(conversation)"
                     @error="onAvatarError"
                     alt="头像"
                     class="w-10 h-10 rounded-full object-cover bg-gray-100" />
                <svg v-else class="w-10 h-10 bg-gray-100 text-gray-500 rounded-full" fill="currentColor" viewBox="0 0 24 24">
                  <path d="M24 20.993V24H0v-2.996A14.977 14.977 0 0112.004 15c4.904 0 9.26 2.354 11.996 5.993zM16.002 8.999a4 4 0 11-8 0 4 4 0 018 0z"></path>
                </svg>
              </div>
              <div class="conversation-info">
                <div class="conversation-header">
                  <h3 class="conversation-name">{{ conversation.UserName || conversation.userName }}</h3>
                  <span class="conversation-time">{{ formatTime(conversation.LastMessageTime || conversation.UpdatedAt || conversation.updatedAt || conversation.CreatedAt) }}</span>
                </div>
                <div class="conversation-preview">
                  <p class="conversation-last-message">{{ conversation.lastMessage }}</p>
                  <span v-if="conversation.unreadCount > 0" class="unread-badge">{{ conversation.unreadCount }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧聊天区域 -->
      <div class="chat-area">
        <div v-if="!currentConversation" class="empty-chat">
          <svg class="w-24 h-24 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"></path>
          </svg>
          <p class="mt-4 text-gray-500">选择一个会话开始聊天</p>
        </div>
        <div v-else>
          <div class="chat-header">
            <div class="chat-header-info">
              <h2 class="chat-title">{{ currentConversation.UserName || currentConversation.userName }}</h2>
              <p class="chat-subtitle">用户 ID: {{ currentConversation.UserId || currentConversation.userId }}</p>
            </div>
          </div>

          <!-- 聊天窗口 -->
          <div class="chat-window-container">
            <ChatWindow
              :conversation-id="currentConversation.id"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useChatStore } from '@/stores/chat'
import ChatWindow from '@/components/common/ChatWindow.vue'

// Stores
const chatStore = useChatStore()

// State
const currentConversationId = ref(null)

// Computed properties
const currentConversation = computed(() => {
  if (!currentConversationId.value) return null
  return chatStore.conversations.find(c => c.id === currentConversationId.value)
})

// Methods
const formatTime = (dateString) => {
  const date = new Date(dateString)
  const now = new Date()
  const diffTime = Math.abs(now - date)
  const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24))

  if (diffDays === 0) {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  } else if (diffDays === 1) {
    return '昨天 ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  } else if (diffDays < 7) {
    const days = ['日', '一', '二', '三', '四', '五', '六']
    return '周' + days[date.getDay()] + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
  } else {
    return date.toLocaleDateString()
  }
}

const selectConversation = (conversation) => {
  currentConversationId.value = conversation.id
  chatStore.setCurrentConversation(conversation)
  // 标记会话为已读
  chatStore.markConversationAsRead(conversation.id)
}

const getAvatar = (conversation) => {
  return conversation.UserAvatar || conversation.userAvatar || ''
}

const onAvatarError = (e) => {
  e.target.style.display = 'none'
}

// Lifecycle hooks
onMounted(async () => {
  // 连接聊天Hub
  await chatStore.connectHub()
  // 初始化聊天数据
  await chatStore.initChat()
})
</script>

<style scoped>
.admin-message-center {
  padding: 2rem;
  background-color: #f9fafb;
  min-height: calc(100vh - 8rem);
}

.page-header {
  margin-bottom: 2rem;
}

.main-content {
  display: grid;
  grid-template-columns: 350px 1fr;
  gap: 2rem;
  background-color: white;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

/* 会话列表 */
.conversation-list {
  border-right: 1px solid #e5e7eb;
  background-color: #f9fafb;
}

.conversation-list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  background-color: white;
}

.unread-count-badge {
  background-color: #ef4444;
  color: white;
  font-size: 0.75rem;
  padding: 0.25rem 0.5rem;
  border-radius: 9999px;
  font-weight: 600;
}

.conversation-items {
  height: calc(100vh - 18rem);
  overflow-y: auto;
}

.conversation-item {
  display: flex;
  align-items: center;
  padding: 1rem;
  cursor: pointer;
  transition: background-color 0.2s;
  border-bottom: 1px solid #e5e7eb;
  background-color: white;
}

.conversation-item:hover {
  background-color: #f3f4f6;
}

.conversation-item.active {
  background-color: #ebf5ff;
  border-right: 3px solid #3b82f6;
}

.conversation-item.has-unread {
  background-color: #fef3c7;
}

.conversation-avatar {
  margin-right: 1rem;
}

.conversation-info {
  flex: 1;
  min-width: 0;
}

.conversation-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.25rem;
}

.conversation-name {
  font-weight: 600;
  font-size: 0.875rem;
  color: #374151;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.conversation-time {
  font-size: 0.75rem;
  color: #9ca3af;
}

.conversation-preview {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.conversation-last-message {
  font-size: 0.75rem;
  color: #6b7280;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  flex: 1;
  margin-right: 0.5rem;
}

.unread-badge {
  background-color: #ef4444;
  color: white;
  font-size: 0.625rem;
  padding: 0.125rem 0.375rem;
  border-radius: 9999px;
  font-weight: 600;
}

.loading-state, .empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  color: #6b7280;
}

/* 聊天区域 */
.chat-area {
  display: flex;
  flex-direction: column;
  background-color: white;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-bottom: 1px solid #e5e7eb;
  background-color: #f9fafb;
}

.chat-header-info {
  display: flex;
  flex-direction: column;
}

.chat-title {
  font-size: 1rem;
  font-weight: 600;
  color: #374151;
}

.chat-subtitle {
  font-size: 0.75rem;
  color: #6b7280;
}

.empty-chat {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: calc(100vh - 18rem);
  color: #9ca3af;
}

.chat-window-container {
  flex: 1;
  height: calc(100vh - 24rem);
}

/* 滚动条样式 */
.conversation-items::-webkit-scrollbar {
  width: 6px;
}

.conversation-items::-webkit-scrollbar-track {
  background: transparent;
}

.conversation-items::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 3px;
}

.conversation-items::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}
</style>
