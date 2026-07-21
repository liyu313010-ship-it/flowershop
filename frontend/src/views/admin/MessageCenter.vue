<template>
  <div class="admin-message-center">
    <!-- 页面标题 -->
    <div class="page-header">
      <div>
        <h1 class="text-2xl font-bold">客服消息中心</h1>
        <p class="text-gray-600">接待咨询、实时回复并管理会话状态</p>
      </div>
      <div class="connection-chip" :class="chatStore.isConnected ? 'is-online' : 'is-offline'">
        <span></span>{{ chatStore.isConnected ? '实时在线' : '正在重连' }}
      </div>
    </div>

    <div class="status-filters" aria-label="会话状态筛选">
      <button v-for="item in statusOptions" :key="item.value" type="button"
              :class="{ active: statusFilter === item.value }" @click="statusFilter = item.value">
        {{ item.label }} <span>{{ countByStatus(item.value) }}</span>
      </button>
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
          <div v-else-if="filteredConversations.length === 0" class="empty-state">
            <svg class="w-16 h-16 text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"></path>
            </svg>
            <p class="mt-4 text-gray-500">暂无会话</p>
          </div>
          <div v-else>
            <div
              v-for="conversation in filteredConversations"
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
                  <span class="status-dot" :class="`status-${conversation.status}`">{{ statusLabel(conversation.status) }}</span>
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
            <div class="chat-actions">
              <button v-if="currentConversation.status === 'waiting'" type="button" class="assign-button" @click="assignCurrent">接入会话</button>
              <button v-if="currentConversation.status !== 'closed'" type="button" class="close-button" @click="closeCurrent">结束会话</button>
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
import { ref, computed, onMounted } from 'vue'
import { useChatStore } from '@/stores/chat'
import ChatWindow from '@/components/common/ChatWindow.vue'
import { notifySuccess, notifyError } from '@/utils/notify'

// Stores
const chatStore = useChatStore()

// State
const currentConversationId = ref(null)
const statusFilter = ref('all')
const statusOptions = [
  { value: 'all', label: '全部' },
  { value: 'waiting', label: '等待接待' },
  { value: 'active', label: '处理中' },
  { value: 'closed', label: '已结束' }
]

// Computed properties
const currentConversation = computed(() => {
  if (!currentConversationId.value) return null
  return chatStore.conversations.find(c => c.id === currentConversationId.value)
})
const filteredConversations = computed(() => statusFilter.value === 'all'
  ? chatStore.sortedConversations
  : chatStore.sortedConversations.filter(item => item.status === statusFilter.value))

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
}

const statusLabel = status => ({ waiting: '待接入', active: '处理中', closed: '已结束' }[status] || '未知')
const countByStatus = status => status === 'all'
  ? chatStore.conversations.length
  : chatStore.conversations.filter(item => item.status === status).length

const assignCurrent = async () => {
  try {
    await chatStore.assignConversation(currentConversationId.value)
    notifySuccess('已接入该客服会话')
  } catch (error) {
    notifyError(error?.response?.data?.message || '接入会话失败')
  }
}

const closeCurrent = async () => {
  try {
    await chatStore.closeConversation(currentConversationId.value)
    notifySuccess('客服会话已结束')
  } catch (error) {
    notifyError(error?.response?.data?.message || '结束会话失败')
  }
}

const getAvatar = (conversation) => {
  return conversation.UserAvatar || conversation.userAvatar || ''
}

const onAvatarError = (e) => {
  e.target.style.display = 'none'
}

// Lifecycle hooks
onMounted(async () => {
  // 顶部管理员菜单负责维持全局实时连接，这里只加载会话数据。
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
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 2rem;
}

.connection-chip { display: flex; align-items: center; gap: 7px; padding: 8px 12px; border-radius: 999px; font-size: 12px; font-weight: 700; }
.connection-chip span { width: 8px; height: 8px; border-radius: 50%; }
.connection-chip.is-online { color: #15803d; background: #f0fdf4; }
.connection-chip.is-online span { background: #22c55e; box-shadow: 0 0 0 4px rgba(34,197,94,.14); }
.connection-chip.is-offline { color: #b45309; background: #fffbeb; }
.connection-chip.is-offline span { background: #f59e0b; }

.status-filters { display: flex; flex-wrap: wrap; gap: 10px; margin: -1rem 0 1.25rem; }
.status-filters button { padding: 8px 14px; border: 1px solid #f3d7e3; border-radius: 999px; color: #6b4c5b; background: white; font-size: 13px; }
.status-filters button.active { color: white; border-color: #db2777; background: linear-gradient(135deg, #ec4899, #db2777); }
.status-filters span { margin-left: 4px; opacity: .8; }

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
  background-color: #fff1f7;
  border-right: 3px solid #db2777;
}

.conversation-item.has-unread {
  background-color: #fff7fb;
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

.status-dot { margin-left: 6px; padding: 2px 6px; border-radius: 999px; font-size: 9px; white-space: nowrap; }
.status-waiting { color: #b45309; background: #fef3c7; }
.status-active { color: #15803d; background: #dcfce7; }
.status-closed { color: #64748b; background: #f1f5f9; }

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
.chat-actions { display: flex; gap: 8px; }
.assign-button, .close-button { padding: 7px 12px; border-radius: 9px; font-size: 12px; font-weight: 700; }
.assign-button { color: white; background: #db2777; }
.close-button { color: #9f1239; background: #fff1f2; }

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

@media (max-width: 900px) {
  .admin-message-center { padding: 1rem; }
  .main-content { grid-template-columns: 1fr; }
  .conversation-list { border-right: 0; border-bottom: 1px solid #e5e7eb; }
  .conversation-items { height: 280px; }
  .chat-window-container { height: 55vh; }
}
</style>
