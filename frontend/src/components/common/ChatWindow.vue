<template>
  <div class="chat-window">
    <!-- 聊天头部 -->
    <div class="chat-header">
      <div>
        <h3 class="text-lg font-semibold">{{ chatTitle }}</h3>
        <p class="connection-status" :class="chatStore.isConnected ? 'online' : 'offline'">
          {{ chatStore.isConnected ? '实时连接中' : '连接中断，消息仍会保存' }}
        </p>
      </div>
      <button 
        class="text-gray-500 hover:text-gray-700" 
        @click="onCloseClick"
        aria-label="关闭聊天"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
        </svg>
      </button>
    </div>

    <!-- 消息列表 -->
    <div class="messages-container" ref="messagesContainer">
      <div v-if="isLoading" class="loading-message">
        <div class="flex justify-center items-center">
          <div class="spinner-border animate-spin inline-block w-4 h-4 border-2 rounded-full border-blue-500 border-t-transparent"></div>
          <span class="ml-2 text-sm text-gray-500">加载消息中...</span>
        </div>
      </div>
      <div v-if="chatStore.error" class="error-message">{{ chatStore.error }}</div>
      <div>
        <div 
          v-for="message in sortedCurrentMessages" 
          :key="message.id" 
          class="message-item"
          :class="message.senderId === currentUserId ? 'sent' : 'received'"
        >
          <div class="message-content">
            <div class="message-text">{{ message.content }}</div>
            <div class="message-meta">
              <span class="text-xs text-gray-400">{{ formatTime(message.createdAt) }}</span>
              <span v-if="message.isRead && message.senderId === currentUserId" class="ml-2 text-xs text-blue-400">已读</span>
            </div>
          </div>
        </div>
        <div v-if="!sortedCurrentMessages.length" class="empty-message">
          <p class="text-gray-500 text-center py-8">暂无消息，开始您的对话吧！</p>
        </div>
      </div>
    </div>

    <!-- 消息输入区域 -->
    <div class="chat-input-area">
      <textarea
        v-model="messageText"
        class="message-input"
        placeholder="输入消息..."
        maxlength="2000"
        :disabled="isClosed"
        rows="1"
        @keydown.enter.exact="sendMessage"
        @keydown.enter.shift="handleShiftEnter"
        @input="autoResizeTextarea($event)"
      ></textarea>
      <button 
        class="send-button" 
        @click="sendMessage"
        :disabled="!messageText.trim() || isSending || isClosed"
      >
        <svg v-if="isSending" class="w-5 h-5 animate-spin" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"></path>
        </svg>
        <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8"></path>
        </svg>
      </button>
    </div>
    <div v-if="isClosed" class="closed-message">本次客服会话已结束，重新打开客服可发起新会话。</div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import { useChatStore } from '@/stores/chat'
import chatService from '@/services/chat'
import { useUserStore } from '@/stores/user'

// Props
const props = defineProps({
  conversationId: {
    type: Number,
    required: true
  }
})

// Emits
const emit = defineEmits(['close', 'messageSent'])

// Stores
const chatStore = useChatStore()
const userStore = useUserStore()

// State
const messageText = ref('')
const isSending = ref(false)
const messagesContainer = ref(null)

// Computed properties
const isLoading = computed(() => chatStore.isLoading)
const currentUserId = computed(() => userStore.user?.id || 0)
const currentMessages = computed(() => chatStore.currentMessages)
const sortedCurrentMessages = computed(() => [...currentMessages.value].sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt)))
const chatTitle = computed(() => {
  const c = chatStore.currentConversation
  if (!c) return '聊天'
  const isAdmin = userStore.isAdmin
  const otherName = isAdmin ? c.userName : c.adminName
  return otherName || (isAdmin ? '用户' : '欢雨在线客服')
})
const isClosed = computed(() => chatStore.currentConversation?.status === 'closed')

// Methods
const formatTime = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}

const sendMessage = async () => {
  if (!messageText.value.trim() || isSending.value) return

  try {
    isSending.value = true
    chatStore.error = null
    await chatStore.sendMessage(messageText.value)
    messageText.value = ''
    
    // 触发消息发送事件
    emit('messageSent')
    
    // 滚动到底部
    await nextTick()
    scrollToBottom()
  } catch (error) {
    chatStore.error = error?.response?.data?.message || error.message || '消息发送失败，请重试'
  } finally {
    isSending.value = false
  }
}

const handleShiftEnter = (event) => {
  // Shift+Enter 使用 textarea 默认换行行为。
  autoResizeTextarea(event)
}

const autoResizeTextarea = (e) => {
  const textarea = e?.target
  if (!textarea) return
  textarea.style.height = 'auto'
  textarea.style.height = Math.min(textarea.scrollHeight, 100) + 'px' // 限制最大高度
}

const scrollToBottom = () => {
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight
  }
}

const onCloseClick = () => { emit('close') }

// Watchers
watch(() => props.conversationId, async (newId) => {
  if (newId) {
    let conversation = (chatStore.conversations || []).find(c => c.id === newId)
    if (!conversation) conversation = await chatService.getConversation(newId)
    await chatStore.setCurrentConversation(conversation)
    await nextTick()
    scrollToBottom()
  }
}, { immediate: true })

watch(() => chatStore.messages.length, async () => {
  await nextTick()
  scrollToBottom()
})

// Lifecycle hooks
onMounted(async () => {
  scrollToBottom()
})
</script>

<style scoped>
.chat-window {
  display: flex;
  flex-direction: column;
  height: 100%;
  border-radius: 8px;
  background-color: #fff;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  border-bottom: 1px solid #e5e7eb;
  background: linear-gradient(135deg, #fff7fb, #fff);
}

.connection-status { margin-top: 2px; font-size: 11px; }
.connection-status.online { color: #16a34a; }
.connection-status.offline { color: #b45309; }

.messages-container {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
  background-color: #f9fafb;
}

.message-item {
  display: flex;
  margin-bottom: 12px;
}

.message-item.sent {
  justify-content: flex-end;
}

.message-item.received {
  justify-content: flex-start;
}

.message-content {
  max-width: 70%;
  padding: 8px 12px;
  border-radius: 18px;
  word-wrap: break-word;
}

.message-item.sent .message-content {
  background: linear-gradient(135deg, #ec4899, #db2777);
  color: #fff;
  border-bottom-right-radius: 4px;
}

.message-item.received .message-content {
  background-color: #fff;
  color: #374151;
  border: 1px solid #e5e7eb;
  border-bottom-left-radius: 4px;
}

.message-text {
  font-size: 14px;
  line-height: 1.4;
  margin-bottom: 4px;
}

.message-meta {
  display: flex;
  justify-content: flex-end;
  align-items: center;
}

.loading-message,
.empty-message {
  padding: 16px;
  text-align: center;
}

.chat-input-area {
  display: flex;
  align-items: flex-end;
  padding: 12px 16px;
  border-top: 1px solid #e5e7eb;
  background-color: #fff;
  gap: 8px;
}

.message-input {
  flex: 1;
  min-height: 36px;
  max-height: 100px;
  padding: 8px 12px;
  border: 1px solid #e5e7eb;
  border-radius: 18px;
  font-size: 14px;
  resize: none;
  outline: none;
  transition: border-color 0.2s;
  font-family: inherit;
}

.message-input:focus {
  border-color: #ec4899;
  box-shadow: 0 0 0 3px rgba(236, 72, 153, 0.1);
}

.send-button {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border: none;
  border-radius: 50%;
  background-color: #ec4899;
  color: #fff;
  cursor: pointer;
  transition: background-color 0.2s;
  flex-shrink: 0;
}

.send-button:hover:not(:disabled) {
  background-color: #db2777;
}

.send-button:disabled {
  background-color: #f9a8d4;
  cursor: not-allowed;
}

.error-message { margin: 8px 16px; padding: 8px 12px; border-radius: 10px; background: #fff1f2; color: #be123c; font-size: 13px; }
.closed-message { padding: 8px 16px 12px; background: #fff7ed; color: #9a3412; text-align: center; font-size: 12px; }

/* 滚动条样式 */
.messages-container::-webkit-scrollbar {
  width: 6px;
}

.messages-container::-webkit-scrollbar-track {
  background: transparent;
}

.messages-container::-webkit-scrollbar-thumb {
  background: #cbd5e1;
  border-radius: 3px;
}

.messages-container::-webkit-scrollbar-thumb:hover {
  background: #94a3b8;
}
</style>
