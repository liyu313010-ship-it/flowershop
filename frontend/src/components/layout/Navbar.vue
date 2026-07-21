<template>
  <!-- 顶部导航栏 - 固定在页面顶部 -->
  <nav class="site-nav fixed top-0 left-0 right-0 z-50 bg-white/95 shadow-md backdrop-blur" aria-label="主导航">
    <div class="container mx-auto px-4 sm:px-6">
      <div class="flex items-center justify-between min-h-16 h-16">
        
        <!-- 品牌Logo区域 -->
        <div class="flex items-center space-x-2">
          <!-- Logo图片 - 可替换为自己的logo -->
          <img 
            src="/images/brand-mark.svg" 
            alt="欢雨鲜花Logo" 
            class="w-9 h-9 sm:w-10 sm:h-10 rounded-full object-cover"
          >
          <router-link 
            to="/" 
            class="text-xl sm:text-2xl font-bold text-huanyu-pink-500 hover:text-huanyu-pink-600 transition-colors whitespace-nowrap"
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
            <button type="button" class="nav-link flex items-center space-x-1">
              <span>商品分类</span>
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
              </svg>
            </button>
            
            <!-- 下拉菜单内容（动态） -->
            <div class="absolute top-full left-0 mt-2 w-56 bg-white rounded-lg shadow-xl opacity-0 invisible group-hover:opacity-100 group-hover:visible group-focus-within:opacity-100 group-focus-within:visible transition-all duration-200" role="menu">
              <router-link 
                v-for="cat in navbarCategories" 
                :key="cat.id"
                :to="`/products?category=${cat.id}`" 
                class="block min-h-11 px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50 hover:text-huanyu-pink-600 focus-visible:outline-none"
                role="menuitem"
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

          <router-link to="/products" class="nav-shop-cta">
            立即选花
          </router-link>
        </div>
        
        <!-- 右侧操作区域 -->
        <div class="flex items-center gap-1 sm:gap-3">
          
          <!-- 购物车图标 - 仅对非管理员显示 -->
          <router-link 
            v-if="userStore.isLoggedIn && !userStore.isAdmin"
            to="/cart" 
            class="relative min-w-11 min-h-11 p-2 flex items-center justify-center text-gray-600 hover:text-huanyu-pink-600 transition-colors rounded-full hover:bg-huanyu-pink-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-huanyu-pink-400"
            aria-label="购物车"
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
            <button class="flex items-center gap-2 p-2 rounded-full hover:bg-gray-100 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-huanyu-pink-400" :aria-label="accountMenuLabel" aria-haspopup="menu">
              <!-- 用户头像 - 可替换为用户上传的头像 -->
              <div class="relative shrink-0">
                <div
                  :style="{ backgroundImage: `url(${userAvatar})` }"
                  :alt="userStore.userName"
                  class="w-8 h-8 rounded-full bg-center bg-cover border-2 border-huanyu-pink-200"
                  @error="handleAvatarError($event)"
                ></div>
                <span
                  v-if="userStore.isAdmin && adminUnreadCount > 0"
                  class="admin-avatar-badge"
                  :aria-label="`${adminUnreadCount}条未读客服消息`"
                >{{ adminUnreadLabel }}</span>
              </div>
              <span class="hidden lg:inline text-sm font-medium text-gray-700 max-w-24 truncate">{{ userStore.userName }}</span>
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
              </svg>
            </button>
            
            <!-- 用户下拉菜单 -->
            <div class="absolute top-full right-0 mt-2 w-48 bg-white rounded-lg shadow-xl opacity-0 invisible group-hover:opacity-100 group-hover:visible group-focus-within:opacity-100 group-focus-within:visible transition-all duration-200" role="menu">
              <router-link 
                v-if="userStore.isAdmin"
                to="/admin" 
                class="block min-h-11 px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50 hover:text-huanyu-pink-600 focus-visible:outline-none"
                role="menuitem"
              >
                管理员后台
              </router-link>
              <router-link
                v-if="userStore.isAdmin"
                to="/admin/messages"
                class="admin-message-menu-item"
                role="menuitem"
              >
                <span class="flex items-center gap-2">
                  <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor" aria-hidden="true">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.8" d="M8 10h.01M12 10h.01M16 10h.01M21 11.5a8.38 8.38 0 01-9 8.5 9.6 9.6 0 01-4.2-.95L3 20l1.4-3.7A8.1 8.1 0 013 11.5C3 6.8 7 3 12 3s9 3.8 9 8.5z" />
                  </svg>
                  客服消息
                </span>
                <span v-if="adminUnreadCount > 0" class="admin-menu-badge">{{ adminUnreadLabel }}</span>
              </router-link>
              <router-link 
                to="/profile" 
                class="block min-h-11 px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50 hover:text-huanyu-pink-600 focus-visible:outline-none"
                role="menuitem"
              >
                个人中心
              </router-link>
              <router-link 
                v-if="!userStore.isAdmin"
                to="/orders" 
                class="block min-h-11 px-4 py-2 text-gray-700 hover:bg-huanyu-pink-50 hover:text-huanyu-pink-600 focus-visible:outline-none"
                role="menuitem"
              >
                我的订单
              </router-link>
              <hr class="my-1">
              <button 
                @click="handleLogout"
                class="w-full min-h-11 text-left px-4 py-2 text-red-600 hover:bg-red-50 transition-colors focus-visible:outline-none"
                role="menuitem"
              >
                退出登录
              </button>
            </div>
          </div>
          
          <!-- 登录/注册按钮 - 未登录时显示 -->
          <div v-else class="hidden sm:flex items-center space-x-2">
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
            type="button"
            @click="toggleMobileMenu"
            class="mobile-menu-toggle md:hidden min-w-11 min-h-11 p-2 flex items-center justify-center rounded-lg text-gray-600 hover:text-huanyu-pink-600 hover:bg-huanyu-pink-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-huanyu-pink-400"
            aria-label="打开菜单"
            :aria-expanded="showMobileMenu"
            aria-controls="mobile-navigation"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"></path>
            </svg>
          </button>
        </div>
      </div>
      
      <!-- 移动端菜单 -->
      <div v-if="showMobileMenu" id="mobile-navigation" class="md:hidden border-t border-gray-100 py-3 pb-safe bg-white/98" role="menu">
        <div class="flex flex-col gap-1">
          <router-link 
            to="/" 
            class="mobile-nav-link"
            role="menuitem"
            @click="showMobileMenu = false"
          >
            首页
          </router-link>
          <router-link 
            to="/products" 
            class="mobile-nav-link"
            role="menuitem"
            @click="showMobileMenu = false"
          >
            全部商品
          </router-link>
          <!-- 购物车 - 仅对非管理员显示 -->
          <router-link 
            v-if="userStore.isLoggedIn && !userStore.isAdmin"
            to="/cart" 
            class="mobile-nav-link"
            role="menuitem"
            @click="showMobileMenu = false"
          >
            购物车
          </router-link>
          <router-link
            v-if="userStore.isLoggedIn && !userStore.isAdmin"
            to="/orders"
            class="mobile-nav-link"
            role="menuitem"
            @click="showMobileMenu = false"
          >
            我的订单
          </router-link>
          <router-link
            v-if="userStore.isAdmin"
            to="/admin/messages"
            class="mobile-nav-link admin-message-mobile"
            role="menuitem"
            @click="showMobileMenu = false"
          >
            <span>客服消息</span>
            <span v-if="adminUnreadCount > 0" class="admin-menu-badge">{{ adminUnreadLabel }}</span>
          </router-link>
          <router-link
            v-if="userStore.isLoggedIn"
            to="/profile"
            class="mobile-nav-link"
            role="menuitem"
            @click="showMobileMenu = false"
          >
            个人中心
          </router-link>
          <router-link 
            to="/about" 
            class="mobile-nav-link"
            role="menuitem"
            @click="showMobileMenu = false"
          >
            关于我们
          </router-link>
          <router-link 
            to="/contact" 
            class="mobile-nav-link"
            role="menuitem"
            @click="showMobileMenu = false"
          >
            联系我们
          </router-link>
          <template v-if="!userStore.isLoggedIn">
          <button 
            @click="goToAuth" 
            class="mobile-nav-link text-left"
            role="menuitem"
          >
            登录
          </button>
          <button 
            @click="goToAuth" 
            class="mobile-nav-link text-left"
            role="menuitem"
          >
            注册
          </button>
          </template>
          <button v-else @click="handleLogout" class="mobile-nav-link text-left text-red-600" role="menuitem">退出登录</button>
        </div>
      </div>
    </div>
  </nav>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { useCartStore } from '@/stores/cart'
import { useChatStore } from '@/stores/chat'
import { getAvatarUrl, handleAvatarError } from '@/utils/avatar.js'
import { formatUnreadBadge } from '@/utils/chatData'
import { categoryService } from '@/services/category'

// 响应式数据
const showMobileMenu = ref(false)
const navbarCategories = ref([])

// 路由和状态管理
const router = useRouter()
const userStore = useUserStore()
const cartStore = useCartStore()
const chatStore = useChatStore()

// 计算属性 - 用户头像
const userAvatar = computed(() => {
  // 使用头像处理工具函数处理头像URL
  return getAvatarUrl(userStore.user?.avatar)
})
const adminUnreadCount = computed(() => Math.max(0, Number(chatStore.unreadCount || 0)))
const adminUnreadLabel = computed(() => formatUnreadBadge(adminUnreadCount.value))
const accountMenuLabel = computed(() => {
  const unread = userStore.isAdmin && adminUnreadCount.value > 0
    ? `，${adminUnreadCount.value}条未读客服消息`
    : ''
  return `${userStore.userName}账户菜单${unread}`
})

let adminChatInitialization = null
const initializeAdminMessages = async () => {
  if (!userStore.isAuthenticated || !userStore.isAdmin) return
  if (adminChatInitialization) return adminChatInitialization

  adminChatInitialization = (async () => {
    await chatStore.connectHub().catch(() => {})
    await chatStore.fetchUnreadCount()
  })()

  try {
    await adminChatInitialization
  } finally {
    adminChatInitialization = null
  }
}

// 方法
const toggleMobileMenu = () => {
  showMobileMenu.value = !showMobileMenu.value
}

const handleLogout = () => {
  chatStore.reset()
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
})

watch(
  () => [userStore.user?.id, userStore.isAuthenticated, userStore.isAdmin],
  initializeAdminMessages,
  { immediate: true }
)




</script>

<style scoped>
/* 44px minimum touch target improves one-handed mobile use. */
.mobile-nav-link {
  display: block;
  width: 100%;
  min-height: 44px;
  padding: .65rem 1rem;
  border-radius: .65rem;
  color: #374151;
  transition: background-color .2s, color .2s;
}

.mobile-nav-link:hover,
.mobile-nav-link:focus-visible {
  background-color: rgba(252, 231, 243, .8);
  color: #db2777;
  outline: none;
}

.pb-safe {
  padding-bottom: max(.75rem, env(safe-area-inset-bottom));
}

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

.admin-avatar-badge,
.admin-menu-badge {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 20px;
  height: 20px;
  padding: 0 5px;
  border-radius: 999px;
  color: white;
  background: #ef4444;
  font-size: 10px;
  font-weight: 800;
  line-height: 1;
  box-shadow: 0 3px 9px rgba(239, 68, 68, .3);
}

.admin-avatar-badge {
  position: absolute;
  top: -7px;
  right: -8px;
  border: 2px solid white;
}

.admin-message-menu-item {
  display: flex;
  min-height: 44px;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: .5rem 1rem;
  color: #374151;
  transition: background-color .2s, color .2s;
}

.admin-message-menu-item:hover,
.admin-message-menu-item:focus-visible {
  color: #db2777;
  background: #fdf2f8;
  outline: none;
}

.admin-message-mobile {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

@media (max-width: 640px) {
  .site-nav :deep(.container) {
    padding-left: max(1rem, env(safe-area-inset-left));
    padding-right: max(1rem, env(safe-area-inset-right));
  }
}
</style>
