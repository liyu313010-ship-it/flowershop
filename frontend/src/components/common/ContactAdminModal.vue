<template>
  <Transition name="modal" v-if="props.isVisible">
    <div class="modal-overlay" @click="close">
      <div class="modal-content" @click.stop>
        <!-- 弹窗头部 -->
        <div class="modal-header">
          <h3 class="text-xl font-semibold">联系管理员</h3>
          <button 
            class="text-gray-500 hover:text-gray-700" 
            @click="close"
            aria-label="关闭"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
          </button>
        </div>

        <!-- 弹窗主体 -->
        <div class="modal-body">
          <div v-if="!isInitialized" class="loading-container">
            <div class="spinner-border animate-spin inline-block w-8 h-8 border-4 rounded-full border-blue-500 border-t-transparent"></div>
            <p class="mt-4 text-gray-600">正在初始化聊天...</p>
          </div>
          <div v-else class="chat-container">
            <ChatWindow 
              v-if="chatStore.currentConversation || adminConversationId !== null" 
              :conversation-id="adminConversationId ?? (chatStore.currentConversation?.id ?? 0)" 
              @close="close"
              @message-sent="handleMessageSent"
            />
            <div v-else class="flex flex-col items-center justify-center h-full p-6 text-center">
              <svg class="w-12 h-12 text-gray-400 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6M7 20h10a2 2 0 002-2V6a2 2 0 00-2-2H7a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <p class="text-gray-600 mb-4">
                {{ userStore.isAuthenticated ? '暂时无法连接客服，稍后再试或点击重试' : '请登录后使用聊天功能' }}
              </p>
              <div class="flex gap-3">
                <button v-if="userStore.isAuthenticated" @click="retryInit" class="px-4 py-2 bg-pink-500 text-white rounded hover:bg-pink-600">重试</button>
                <button v-else @click="goToAuth" class="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">去登录</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useChatStore } from '@/stores/chat'
import { useUserStore } from '@/stores/user'
import ChatWindow from './ChatWindow.vue'

// Props
const props = defineProps({
  isVisible: {
    type: Boolean,
    default: false
  }
})

// Emits
const emit = defineEmits(['close'])

// Stores
const chatStore = useChatStore()
const userStore = useUserStore()
const router = useRouter()

// State
const adminConversationId = ref(null)
const isInitialized = ref(false)
let initTimer = null

// Methods
const close = () => {
  emit('close')
}

const handleMessageSent = () => {
  // 消息发送成功后的处理
  console.log('消息发送成功')
}

const initializeChat = async () => {
  if (!props.isVisible) return
  if (!userStore.isAuthenticated) {
    isInitialized.value = true
    adminConversationId.value = null
    return
  }

  try {
    isInitialized.value = false
    if (initTimer) { clearTimeout(initTimer); initTimer = null }
    initTimer = setTimeout(() => { isInitialized.value = true }, 3000)
    try {
      // 重新连接Hub，确保使用最新用户的Token
      chatStore.disconnectHub()
      await chatStore.connectHub()
    } catch {}
    const conversation = await chatStore.fetchAdminConversation()
    adminConversationId.value = conversation.id
    isInitialized.value = true
  } catch (error) {
    console.error('初始化聊天失败:', error)
    isInitialized.value = true
    adminConversationId.value = null
  }
}

// Watchers
if (import.meta.hot) {
  import.meta.hot.on('vue:setup-update', () => {
    // 热更新时重新初始化
    initializeChat()
  })
}

// Lifecycle hooks
onMounted(() => {
  if (props.isVisible) {
    initializeChat()
  }
  const onKey = (e) => { if (e.key === 'Escape') close() }
  window.addEventListener('keydown', onKey)
  // 保存以便卸载
  ;(window).__contact_admin_modal_onkey = onKey
})

// Watch visibility changes
const unwatch = watch(() => props.isVisible, (newVal) => {
  if (newVal) {
    initializeChat()
  } else {
    adminConversationId.value = null
    isInitialized.value = false
  }
})

// Reset chat when user changes
const unwatchUser = watch(() => userStore.user?.id, () => {
  if (props.isVisible) {
    chatStore.reset()
    adminConversationId.value = null
    initializeChat()
  }
})

onUnmounted(() => {
  unwatch()
  unwatchUser()
  if (initTimer) { clearTimeout(initTimer); initTimer = null }
  try { const onKey = (window).__contact_admin_modal_onkey; if (onKey) window.removeEventListener('keydown', onKey) } catch {}
})

const retryInit = () => { initializeChat() }
const goToAuth = () => { emit('close'); router.push('/auth') }
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
  padding: 1rem;
}

.modal-content {
  background-color: white;
  border-radius: 0.5rem;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.modal-body {
  flex: 1;
  padding: 0;
  overflow: hidden;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 400px;
}

.chat-container {
  height: 500px;
  width: 100%;
}

/* 过渡动画 */
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active .modal-content,
.modal-leave-active .modal-content {
  transition: transform 0.3s ease;
}

.modal-enter-from .modal-content,
.modal-leave-to .modal-content {
  transform: scale(0.95);
}
</style>
