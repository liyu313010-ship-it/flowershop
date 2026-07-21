<template>
  <nav class="bg-white shadow-sm border-b fixed top-16 left-0 right-0 z-40">
    <div class="container mx-auto px-4">
      <div class="flex justify-between items-center h-16">
        <!-- Logo和标题 -->
        <div class="flex items-center space-x-4">
          <div class="flex items-center space-x-2">
            <div class="w-8 h-8 bg-transparent rounded-lg flex items-center justify-center overflow-hidden">
              <img 
                src="/images/admin-flower-mark.svg"
                alt="管理员后台"
                class="w-full h-full object-cover"
              />
            </div>
            <h1 class="text-xl font-bold text-gray-800 truncate max-w-[200px]">管理员后台</h1>
          </div>
        </div>

        <!-- 导航菜单 -->
        <div class="hidden md:flex items-center space-x-8">
          <router-link 
            v-for="item in menuItems" 
            :key="item.path"
            :to="item.path"
            class="flex items-center space-x-2 px-3 py-2 rounded-lg text-gray-600 hover:text-huanyu-pink-600 hover:bg-pink-50 transition-colors whitespace-nowrap"
            :class="{ 'text-huanyu-pink-600 bg-pink-50': isActive(item.path) }"
          >
            <component :is="item.icon" class="w-5 h-5" />
            <span class="truncate max-w-[100px]">{{ item.name }}</span>
          </router-link>
        </div>

        <!-- 用户菜单（移除头像与用户名显示） -->
        <div class="flex items-center">
          <div class="relative" ref="userMenuBtn">
            
            <!-- 下拉菜单 -->
            <div 
              v-if="showUserMenu"
              class="mt-2 w-48 bg-white rounded-lg shadow-lg border py-2 z-[99999] pointer-events-auto fixed"
              :style="menuStyle"
            >
              <router-link 
                to="/admin/profile"
                class="block px-4 py-2 text-gray-700 hover:bg-gray-100 transition-colors"
              >
                个人设置
              </router-link>
              <div class="border-t my-2"></div>
              <button 
                @click="logout"
                class="w-full text-left px-4 py-2 text-red-600 hover:bg-red-50 transition-colors"
              >
                退出登录
              </button>
            </div>
          </div>
        </div>

        <!-- 移动端菜单按钮 -->
        <button 
          @click="showMobileMenu = !showMobileMenu"
          class="md:hidden p-2 text-gray-600 hover:text-gray-800"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"/>
          </svg>
        </button>
      </div>

      <!-- 移动端菜单 -->
      <div v-if="showMobileMenu" class="md:hidden py-4 border-t">
        <router-link 
          v-for="item in menuItems" 
          :key="item.path"
          :to="item.path"
          class="flex items-center space-x-3 px-3 py-2 rounded-lg text-gray-600 hover:text-huanyu-pink-600 hover:bg-pink-50 transition-colors mb-2"
          :class="{ 'text-huanyu-pink-600 bg-pink-50': isActive(item.path) }"
          @click="showMobileMenu = false"
        >
          <component :is="item.icon" class="w-5 h-5" />
          <span>{{ item.name }}</span>
        </router-link>
      </div>
    </div>
  </nav>
  <div class="h-16"></div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { getAvatarUrl, handleAvatarError } from '@/utils/avatar.js'

const userStore = useUserStore()

const route = useRoute()
const router = useRouter()

const showUserMenu = ref(false)
const showMobileMenu = ref(false)
const userMenuBtn = ref(null)
const menuStyle = ref({ top: '56px', right: '16px' })



const updateMenuPosition = () => {
  const el = userMenuBtn.value
  if (!el) return
  const rect = el.getBoundingClientRect()
  const top = rect.bottom + 8 + window.scrollY
  const right = (window.innerWidth - rect.right)
  menuStyle.value = { position: 'fixed', top: `${top}px`, right: `${right}px` }
}

const toggleUserMenu = () => {
  showUserMenu.value = !showUserMenu.value
  if (showUserMenu.value) updateMenuPosition()
}

// 菜单项配置
const menuItems = [
  {
    name: '仪表板',
    path: '/admin',
    icon: 'DashboardIcon'
  },
  {
    name: '数据统计',
    path: '/admin/stats-report',
    icon: 'StatsIcon'
  },
  {
    name: '订单管理',
    path: '/admin/orders',
    icon: 'OrderIcon'
  },
  {
    name: '商品管理',
    path: '/admin/products',
    icon: 'ProductIcon'
  },
  {
    name: '用户管理',
    path: '/admin/users',
    icon: 'UserIcon'
  },
  {
    name: '评价管理',
    path: '/admin/reviews',
    icon: 'ReviewIcon'
  },
  {
    name: '优惠券',
    path: '/admin/coupons',
    icon: 'StatsIcon'
  }
]

// 图标组件
const DashboardIcon = {
  template: `
    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"/>
    </svg>
  `
}

const OrderIcon = {
  template: `
    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"/>
    </svg>
  `
}

const ProductIcon = {
  template: `
    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"/>
    </svg>
  `
}

const UserIcon = {
  template: `
    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"/>
    </svg>
  `
}

const StatsIcon = {
  template: `
    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/>
    </svg>
  `
}

const ReviewIcon = {
  template: `
    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"/>
    </svg>
  `
}

// 检查当前路由是否激活
const isActive = (path) => {
  return route.path === path || route.path.startsWith(path + '/')
}

// 退出登录
const logout = () => {
  showUserMenu.value = false
  userStore.logout()
  router.push('/auth')
}

// 点击外部关闭下拉菜单
const handleClickOutside = (e) => {
  if (!e.target.closest('.relative')) {
    showUserMenu.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside)
  window.addEventListener('scroll', updateMenuPosition, { passive: true })
  window.addEventListener('resize', updateMenuPosition)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
  window.removeEventListener('scroll', updateMenuPosition)
  window.removeEventListener('resize', updateMenuPosition)
})
</script>
