<template>
  <!-- 顶部导航栏 - 固定在页面顶部 -->
  <nav class="fixed top-0 left-0 right-0 z-50 bg-white shadow-md">
    <div class="container mx-auto px-4">
      <div class="flex items-center justify-between h-16">
        
        <!-- 品牌Logo区域 -->
        <div class="flex items-center space-x-2">
          <!-- Logo图片 - 可替换为自己的logo -->
          <img 
            src="/images/logo.png" 
            alt="欢雨鲜花Logo" 
            class="w-10 h-10 rounded-full object-cover"
          >
          <router-link 
            to="/" 
            class="text-2xl font-bold text-huanyu-pink-500 hover:text-huanyu-pink-600 transition-colors"
          >
            欢雨flower
          </router-link>
        </div>
        
        <!-- 导航菜单 - 桌面端显示 -->
        <div class="hidden md:flex items-center space-x-8">
          <router-link 
            to="/" 
            class="nav-link"
            :class="{ 'nav-link-active': $route.name === 'Home' }"
          >
            首页
          </router-link>
          
          <!-- 商品分类下拉菜单 -->
          <div class="relative group">
            <button class="nav-link flex items-center space-x-1">
              <span>商品分类</span>
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
              </svg>
            </button>
            
            <!-- 下拉菜单内容（动态） -->
            <div class="absolute top-full left-0 mt-2 w-56 bg-white rounded-lg shadow-xl opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200">
              <router-link 
                v-for="cat in navbarCategories" 
                :key="cat.id"
                :to="`/products?category=${cat.id}`" 
                class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50 hover:text-huanyu-pink-600"
              >
                {{ cat.name }}
              </router-link>
            </div>
          </div>
          
          <router-link 
            to="/about" 
            class="nav-link"
          >
            关于我们
          </router-link>
          
          <router-link 
            to="/contact" 
            class="nav-link"
          >
            联系我们
          </router-link>
        </div>
        
        <!-- 右侧操作区域 -->
        <div class="flex items-center space-x-4">
          
          <!-- 购物车图标 - 仅对非管理员显示 -->
          <router-link 
            v-if="userStore.isLoggedIn && !userStore.isAdmin" 
            to="/cart" 
            class="relative p-2 text-gray-600 hover:text-huanyu-pink-600 transition-colors"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"></path>
            </svg>
            <!-- 购物车商品数量徽章 -->
            <span 
              v-if="cartStore.itemCount > 0" 
              class="cart-badge"
            >
              {{ cartStore.itemCount }}
            </span>
          </router-link>
          
          <!-- 用户头像和下拉菜单 -->
          <div v-if="userStore.isLoggedIn" class="relative group">
            <button class="flex items-center space-x-2 p-2 rounded-full hover:bg-gray-100 transition-colors">
              <!-- 用户头像 - 可替换为用户上传的头像 -->
              <div 
                :style="{ backgroundImage: `url(${userAvatar})` }" 
                :alt="userStore.userName"
                class="w-8 h-8 rounded-full bg-center bg-cover border-2 border-huanyu-pink-200"
                @error="handleAvatarError($event)"
              ></div>
              <span class="text-sm font-medium text-gray-700">{{ userStore.userName }}</span>
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
              </svg>
            </button>
            
            <!-- 用户下拉菜单 -->
            <div class="absolute top-full right-0 mt-2 w-48 bg-white rounded-lg shadow-xl opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200">
              <router-link 
                v-if="!userStore.isAdmin"
                to="/favorites" 
                class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50 hover:text-huanyu-pink-600"
              >
                我的收藏
              </router-link>
              <router-link 
                v-if="userStore.isAdmin"
                to="/admin" 
                class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50 hover:text-huanyu-pink-600 relative"
                @click="ackBadge('admin')"
              >
                管理员后台
                <span v-if="badgeCounts.admin > 0" class="badge-dot">{{ badgeCounts.admin > 99 ? '99+' : badgeCounts.admin }}</span>
              </router-link>
              <router-link 
                to="/profile" 
                class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50 hover:text-huanyu-pink-600 relative"
                @click="ackBadge('profile')"
              >
                个人中心
                <span v-if="badgeCounts.profile > 0" class="badge-dot">{{ badgeCounts.profile > 99 ? '99+' : badgeCounts.profile }}</span>
              </router-link>
              <router-link 
                v-if="!userStore.isAdmin"
                to="/orders" 
                class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50 hover:text-huanyu-pink-600 relative"
                @click="ackBadge('orders')"
              >
                我的订单
                <span v-if="badgeCounts.orders > 0" class="badge-dot">{{ badgeCounts.orders > 99 ? '99+' : badgeCounts.orders }}</span>
              </router-link>
              <hr class="my-1">
              <button 
                @click="handleLogout"
                class="w-full text-left px-4 py-2 text-red-600 hover:bg-red-50 transition-colors"
              >
                退出登录
              </button>
            </div>
          </div>
          
          <!-- 登录/注册按钮 - 未登录时显示 -->
          <div v-else class="flex items-center space-x-2">
            <button 
              @click="goToAuth"
              class="btn-secondary text-sm font-bold shadow-lg hover:shadow-xl transform hover:scale-105 transition-all duration-200"
            >
              登录
            </button>
            <button 
              @click="goToAuth"
              class="btn-primary text-sm font-bold shadow-lg hover:shadow-xl transform hover:scale-105 transition-all duration-200"
            >
              注册
            </button>
          </div>
          
          <!-- 移动端菜单按钮 -->
          <button 
            @click="toggleMobileMenu"
            class="md:hidden p-2 text-gray-600 hover:text-huanyu-pink-600"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"></path>
            </svg>
          </button>
        </div>
      </div>
      
      <!-- 移动端菜单 -->
      <div v-if="showMobileMenu" class="md:hidden border-t border-gray-200 py-4">
        <div class="flex flex-col space-y-2">
          <router-link 
            to="/" 
            class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50"
            @click="showMobileMenu = false"
          >
            首页
          </router-link>
          <router-link 
            to="/products" 
            class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50"
            @click="showMobileMenu = false"
          >
            全部商品
          </router-link>
          <router-link 
            v-if="userStore.isLoggedIn && !userStore.isAdmin"
            to="/favorites" 
            class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50"
            @click="showMobileMenu = false"
          >
            我的收藏
          </router-link>
          <!-- 购物车 - 仅对非管理员显示 -->
          <router-link 
            v-if="userStore.isLoggedIn && !userStore.isAdmin"
            to="/cart" 
            class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50"
            @click="showMobileMenu = false"
          >
            购物车
          </router-link>
          <router-link 
            to="/about" 
            class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50"
            @click="showMobileMenu = false"
          >
            关于我们
          </router-link>
          <router-link 
            to="/contact" 
            class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50"
            @click="showMobileMenu = false"
          >
            联系我们
          </router-link>
          <button 
            @click="goToAuth" 
            class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50 text-left"
          >
            登录
          </button>
          <button 
            @click="goToAuth" 
            class="block px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50 text-left"
          >
            注册
          </button>
        </div>
      </div>
    </div>
  </nav>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useCartStore } from '@/stores/cart'
import { getAvatarUrl, handleAvatarError } from '@/utils/avatar.js'
import { categoryService } from '@/services/category'
import orderService from '@/services/orderService'
import api from '@/services/api'

// 响应式数据
const showMobileMenu = ref(false)
const navbarCategories = ref([])

// 路由和状态管理
const router = useRouter()
const userStore = useUserStore()
const cartStore = useCartStore()

// 计算属性 - 用户头像
const userAvatar = computed(() => {
  // 使用头像处理工具函数处理头像URL
  return getAvatarUrl(userStore.user?.avatar)
})

// 方法
const toggleMobileMenu = () => {
  showMobileMenu.value = !showMobileMenu.value
}

const handleLogout = () => {
  userStore.logout()
  router.push('/')
  showMobileMenu.value = false
}

const closeUserMenu = (event) => {
  // 通过失去焦点来关闭下拉菜单
  if (event && event.target) {
    event.target.blur()
  }
}

const goToAuth = () => {
  console.log('goToAuth clicked, navigating to /auth')
  router.push('/auth')
  showMobileMenu.value = false
}

onMounted(async () => {
  try {
    const res = await categoryService.getCategories()
    const list = res?.data || res || []
  navbarCategories.value = Array.isArray(list)
    ? list.map(c => ({ id: Number(c.id ?? c.Id), name: c.name ?? c.Name }))
    : []
  } catch {
    navbarCategories.value = []
  }
  try {
    await refreshBadges()
  } catch {}
})




// badges
const badgeCounts = ref({ admin: 0, profile: 0, orders: 0 })
const badgeKeys = { admin: 'badge_admin', profile: 'badge_profile', orders: 'badge_orders' }
const getLastSeen = (key) => {
  try { const m = JSON.parse(localStorage.getItem('nav_last_seen') || '{}'); const v = m[key]; return v ? new Date(v) : null } catch { return null }
}
const setLastSeen = (key, date = new Date()) => {
  try { const m = JSON.parse(localStorage.getItem('nav_last_seen') || '{}'); m[key] = date.toISOString(); localStorage.setItem('nav_last_seen', JSON.stringify(m)) } catch {}
}
const clamp99 = (n) => (n > 99 ? 99 : n)
const refreshBadges = async () => {
  const token = localStorage.getItem('token')
  if (!token) { badgeCounts.value = { admin: 0, profile: 0, orders: 0 }; return }
  const now = new Date()
  const adminSeen = getLastSeen(badgeKeys.admin) || new Date(now.getTime() - 24*60*60*1000)
  const profileSeen = getLastSeen(badgeKeys.profile) || new Date(now.getTime() - 24*60*60*1000)
  const ordersSeen = getLastSeen(badgeKeys.orders) || new Date(now.getTime() - 24*60*60*1000)
  let adminCount = 0
  let profileCount = 0
  let ordersCount = 0
  try {
    if (userStore.isAdmin) {
      const users = await api.get('/User', { silent: true })
      const list = users?.data || users || []
      const arr = Array.isArray(list) ? list : (list.items || list.Items || [])
      adminCount = clamp99((arr || []).filter(u => String(u.Role || u.role || '').toLowerCase() === 'th' && new Date(u.CreatedAt || u.createdAt || 0) > adminSeen).length)
    }
  } catch {}
  try {
    const me = await api.get('/Profile', { silent: true })
    const d = me?.data || me || {}
    const updated = d.UpdatedAt || d.updatedAt || d.LastLoginAt || d.lastLoginAt || d.CreatedAt || d.createdAt
    profileCount = updated && new Date(updated) > profileSeen ? 1 : 0
  } catch {}
  try {
    const my = await orderService.getUserOrders()
    const list = my?.data || []
    const arr = Array.isArray(list) ? list : (list.items || list.Items || [])
    ordersCount = clamp99((arr || []).filter(o => new Date(o.CreatedAt || o.createdAt || o.created_at || 0) > ordersSeen).length)
  } catch {}
  badgeCounts.value = { admin: adminCount, profile: profileCount, orders: ordersCount }
}
const ackBadge = (key) => { setLastSeen(badgeKeys[key]); badgeCounts.value[key] = 0 }
</script>

<style scoped>
/* 导航栏样式增强 */
.nav-link {
  position: relative;
}

.nav-link::after {
  content: '';
  position: absolute;
  bottom: -2px;
  left: 0;
  width: 0;
  height: 2px;
  background-color: #ec4899;
  transition: width 0.3s ease;
}

.nav-link:hover::after,
.nav-link-active::after {
  width: 100%;
}

/* 购物车徽章样式 */
.cart-badge {
  position: absolute;
  top: 0;
  right: 0;
  background-color: #ef4444;
  color: white;
  font-size: 10px;
  font-weight: bold;
  min-width: 16px;
  height: 16px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 4px;
  transform: translate(25%, -25%);
}
.badge-dot {
  position: absolute;
  top: 6px;
  right: 10px;
  min-width: 16px;
  height: 16px;
  background-color: #ef4444;
  color: #fff;
  font-size: 10px;
  border-radius: 9999px;
  padding: 0 4px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 0 0 2px #fff;
}
</style>
 
