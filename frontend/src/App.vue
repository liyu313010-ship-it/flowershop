<template>
  <div id="app" class="min-h-screen">
    <!-- 导航栏组件 -->
    <Navbar />
    <AdminNav v-if="isAdminRoute" />
    
    <!-- 主要内容区域 -->
    <main class="pt-16">
      <!-- 路由视图 - 根据路由显示不同页面 -->
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </router-view>
    </main>
    
    <!-- 页脚组件 -->
    <Footer />
    
    <!-- 返回顶部组件 -->
    <BackToTop />
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
import BackToTop from './components/BackToTop.vue'

const userStore = useUserStore()
const cartStore = useCartStore()
const route = useRoute()
const isAdminRoute = computed(() => route.path.startsWith('/admin'))

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
  } catch {}
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
