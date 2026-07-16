<template>
  <div v-if="!userStore.isAdmin" class="support-launcher">
    <button
      type="button"
      class="support-button"
      aria-label="联系在线客服"
      @click="isOpen = true"
    >
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.8" d="M8 10h.01M12 10h.01M16 10h.01M21 11.5a8.38 8.38 0 01-9 8.5 9.6 9.6 0 01-4.2-.95L3 20l1.4-3.7A8.1 8.1 0 013 11.5C3 6.8 7 3 12 3s9 3.8 9 8.5z" />
      </svg>
      <span class="support-label">在线客服</span>
      <span v-if="chatStore.unreadCount" class="support-badge">{{ chatStore.unreadCount > 99 ? '99+' : chatStore.unreadCount }}</span>
    </button>
    <ContactAdminModal :is-visible="isOpen" @close="isOpen = false" />
  </div>
</template>

<script setup>
import { onMounted, ref, watch } from 'vue'
import ContactAdminModal from './ContactAdminModal.vue'
import { useChatStore } from '@/stores/chat'
import { useUserStore } from '@/stores/user'

const isOpen = ref(false)
const chatStore = useChatStore()
const userStore = useUserStore()

const initializeUnread = async () => {
  if (!userStore.isAuthenticated || userStore.isAdmin) return
  await chatStore.connectHub().catch(() => {})
  await chatStore.fetchUnreadCount()
}

onMounted(initializeUnread)
watch(() => userStore.user?.id, initializeUnread)
</script>

<style scoped>
.support-launcher { position: fixed; right: max(20px, env(safe-area-inset-right)); bottom: max(24px, env(safe-area-inset-bottom)); z-index: 45; }
.support-button { position: relative; display: flex; align-items: center; gap: 8px; min-height: 52px; padding: 0 18px; border: 1px solid rgba(255,255,255,.8); border-radius: 999px; color: white; background: linear-gradient(135deg, #f472b6, #db2777); box-shadow: 0 14px 36px rgba(219,39,119,.28); transition: transform .2s ease, box-shadow .2s ease; }
.support-button:hover { transform: translateY(-3px); box-shadow: 0 18px 42px rgba(219,39,119,.36); }
.support-button:focus-visible { outline: 3px solid rgba(244,114,182,.35); outline-offset: 3px; }
.support-button svg { width: 23px; height: 23px; }
.support-label { font-size: 14px; font-weight: 700; letter-spacing: .03em; }
.support-badge { position: absolute; top: -7px; right: -5px; min-width: 21px; height: 21px; padding: 0 5px; display: grid; place-items: center; border: 2px solid white; border-radius: 999px; background: #ef4444; color: white; font-size: 10px; font-weight: 800; }
@media (max-width: 640px) { .support-label { display: none; } .support-button { width: 52px; padding: 0; justify-content: center; } }
@media (prefers-reduced-motion: reduce) { .support-button { transition: none; } }
</style>
