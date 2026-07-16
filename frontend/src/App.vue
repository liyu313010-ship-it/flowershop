<template>
  <div id="app" class="min-h-screen">
    <!-- 导航栏组件 -->
    <Navbar v-if="!isStandaloneRoute" />
    <AdminNav v-if="isAdminRoute && !isStandaloneRoute" />
    
    <!-- 主要内容区域 -->
    <main :class="{ 'pt-16': !isStandaloneRoute }">
      <!-- 路由视图 - 根据路由显示不同页面 -->
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>
    
    <!-- 页脚组件 -->
    <Footer v-if="!isStandaloneRoute && !isAdminRoute" />
    <SupportChatLauncher v-if="!isStandaloneRoute && !isAdminRoute" />
  </div>
</template>

<script setup>
import { onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useCartStore } from '@/stores/cart'
import Navbar from './components/layout/Navbar.vue'
import Footer from './components/layout/Footer.vue'
import AdminNav from '@/components/admin/AdminNav.vue'
import SupportChatLauncher from '@/components/common/SupportChatLauncher.vue'

const userStore = useUserStore()
const cartStore = useCartStore()
const route = useRoute()
const isAdminRoute = computed(() => route.path.startsWith('/admin'))
const isStandaloneRoute = computed(() => ['Auth', 'AdminLogin'].includes(route.name))

// 应用初始化
onMounted(async () => {
  // 检查用户登录状态
  await userStore.checkAuth()
  
  // 如果用户已登录，加载购物车数据
  if (userStore.isLoggedIn) {
    await cartStore.fetchCart()
  }
  // 过滤开发环境中的中断错误，避免覆盖页面
  try {
    const markImages = (root = document) => {
      root.querySelectorAll?.('img').forEach((image) => {
        if (image.closest('.hero-section, .site-nav')) {
          image.setAttribute('decoding', 'async')
          return
        }
        if (!image.hasAttribute('loading')) image.setAttribute('loading', 'lazy')
        image.setAttribute('decoding', 'async')
      })
    }
    markImages()
    const imageObserver = new MutationObserver((records) => {
      records.forEach((record) => record.addedNodes.forEach((node) => {
        if (node.nodeType === Node.ELEMENT_NODE) markImages(node)
      }))
    })
    imageObserver.observe(document.body, { childList: true, subtree: true })
    window.addEventListener('beforeunload', () => imageObserver.disconnect(), { once: true })

    window.addEventListener('unhandledrejection', (e) => {
      const name = e?.reason?.name || ''
      if (name === 'AbortError' || name === 'CanceledError') {
        e.preventDefault()
      }
    })
    window.addEventListener('error', (e) => {
      const msg = String(e?.message || '')
      if (msg.includes('ERR_ABORTED')) {
        e.preventDefault()
      }
    })
  } catch (error) {
    console.warn('[app-init] 无法启用图片性能优化', error)
  }
})
</script>

<style scoped>
/* 页面切换动画 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
