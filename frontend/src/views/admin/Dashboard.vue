<template>
  <div class="min-h-screen bg-gray-50">
    

    <div class="container mx-auto px-4 py-8">
      <!-- 加载状态 -->
      <div v-if="loading" class="flex justify-center items-center py-12">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-huanyu-pink-600"></div>
      </div>

      <!-- 错误提示 -->
      <div v-else-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6">
        {{ error }}
      </div>

      <div v-else>
        <!-- 统计卡片 -->
        <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-500 text-sm">总用户数</p>
                <p class="text-2xl font-bold text-gray-800">{{ stats.totalUsers.toLocaleString() }}</p>
                <p class="text-xs text-green-600 mt-1">+{{ stats.newUsers }} 本月新增</p>
              </div>
              <div class="bg-blue-100 rounded-full p-3">
                <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"></path>
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-500 text-sm">总订单数</p>
                <p class="text-2xl font-bold text-gray-800">{{ stats.totalOrders.toLocaleString() }}</p>
                <p class="text-xs text-yellow-600 mt-1">{{ stats.pendingOrders }} 待处理</p>
              </div>
              <div class="bg-green-100 rounded-full p-3">
                <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z"></path>
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-500 text-sm">商品数量</p>
                <p class="text-2xl font-bold text-gray-800">{{ stats.totalProducts.toLocaleString() }}</p>
                <p class="text-xs text-purple-600 mt-1">{{ stats.lowStockProducts }} 库存不足</p>
              </div>
              <div class="bg-purple-100 rounded-full p-3">
                <svg class="w-6 h-6 text-purple-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"></path>
                </svg>
              </div>
            </div>
          </div>

          <div class="bg-white rounded-lg shadow p-6">
            <div class="flex items-center justify-between">
              <div>
                <p class="text-gray-500 text-sm">总收入</p>
                <p class="text-2xl font-bold text-gray-800">¥{{ stats.totalRevenue.toLocaleString() }}</p>
                <p class="text-xs text-green-600 mt-1">+{{ stats.todayRevenue }} 今日收入</p>
              </div>
              <div class="bg-yellow-100 rounded-full p-3">
                <svg class="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                </svg>
              </div>
            </div>
          </div>
        </div>

        <!-- 快速操作和图表 -->
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-8">
          <!-- 快速操作 -->
          <div class="lg:col-span-1">
            <div class="bg-white rounded-lg shadow p-6">
              <h2 class="text-lg font-semibold mb-4">快速操作</h2>
              <div class="space-y-3">
                <button 
                  @click="$router.push('/admin/stats-report')"
                  class="w-full p-4 border rounded-lg hover:bg-gray-50 text-left transition-colors"
                >
                  <h3 class="font-medium text-gray-800">数据统计</h3>
                  <p class="text-sm text-gray-600">查看详细销售报表和分析</p>
                </button>
                <button 
                  @click="$router.push('/admin/products')"
                  class="w-full p-4 border rounded-lg hover:bg-gray-50 text-left transition-colors"
                >
                  <h3 class="font-medium text-gray-800">添加商品</h3>
                  <p class="text-sm text-gray-600">添加新的鲜花商品</p>
                </button>
                <button 
                  @click="$router.push('/admin/orders')"
                  class="w-full p-4 border rounded-lg hover:bg-gray-50 text-left transition-colors"
                >
                  <h3 class="font-medium text-gray-800">管理订单</h3>
                  <p class="text-sm text-gray-600">查看和处理订单</p>
                </button>
                <button 
                  @click="$router.push('/admin/users')"
                  class="w-full p-4 border rounded-lg hover:bg-gray-50 text-left transition-colors"
                >
                  <h3 class="font-medium text-gray-800">用户管理</h3>
                  <p class="text-sm text-gray-600">管理用户账户</p>
                </button>
              </div>
            </div>
          </div>

          <!-- 最新订单 -->
          <div class="lg:col-span-2">
            <div class="bg-white rounded-lg shadow p-6">
              <div class="flex justify-between items-center mb-4">
                <h2 class="text-lg font-semibold">最新订单</h2>
                <button 
                  @click="$router.push('/admin/orders')"
                  class="text-huanyu-pink-600 hover:text-huanyu-pink-800 text-sm"
                >
                  查看全部
                </button>
              </div>
              <div class="space-y-3">
                <div v-for="order in recentOrders" :key="order.id" class="flex items-center justify-between p-3 border rounded-lg">
                  <div class="flex-1">
                    <div class="flex items-center space-x-3">
                      <span class="font-medium text-gray-900">#{{ order.id }}</span>
                      <span class="text-sm text-gray-500">{{ order.customerName }}</span>
                      <span :class="getStatusClass(order.status)" class="px-2 py-1 text-xs rounded-full">
                        {{ getStatusText(order.status) }}
                      </span>
                    </div>
                    <div class="text-sm text-gray-600 mt-1">
                      {{ formatDate(order.createdAt) }} · ¥{{ order.totalAmount.toFixed(2) }}
                    </div>
                  </div>
                  <button 
                    @click="viewOrder(order.id)"
                    class="text-huanyu-pink-600 hover:text-huanyu-pink-800 text-sm"
                  >
                    查看
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 数据统计图表 -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
          

          <!-- 热销商品 -->
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-lg font-semibold mb-4">热销商品</h2>
            <div class="space-y-3">
              <div v-for="product in topProducts" :key="product.id" class="flex items-center justify-between">
                <div class="flex items-center space-x-3">
                  <img :src="((product.image || product.ImageUrl || product.imageUrl) ? (((product.image || product.ImageUrl || product.imageUrl).startsWith('http') ? (product.image || product.ImageUrl || product.imageUrl) : (((product.image || product.ImageUrl || product.imageUrl).startsWith('/uploads') || (product.image || product.ImageUrl || product.imageUrl).startsWith('/images')) ? (product.image || product.ImageUrl || product.imageUrl) : '/api' + (product.image || product.ImageUrl || product.imageUrl)))) : '/images/product-placeholder.svg')" :alt="product.name" class="w-10 h-10 rounded-lg object-cover">
                  <div>
                    <p class="font-medium text-gray-900">{{ product.name }}</p>
                    <p class="text-sm text-gray-600">销量: {{ product.salesCount }}</p>
                  </div>
                </div>
                <p class="font-medium text-gray-900">¥{{ product.price.toFixed(2) }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import AdminNav from '@/components/admin/AdminNav.vue'
import adminService from '@/services/adminService.js'

const router = useRouter()
const userStore = useUserStore()

// 响应式数据
const loading = ref(true)
const error = ref('')
const stats = ref({
  totalUsers: 0,
  newUsers: 0,
  totalOrders: 0,
  pendingOrders: 0,
  totalProducts: 0,
  lowStockProducts: 0,
  totalRevenue: 0,
  todayRevenue: 0
})
const recentOrders = ref([])
const topProducts = ref([])

// 方法
const logout = () => {
  userStore.logout()
  router.push('/auth')
}

const loadDashboardData = async () => {
    try {
      loading.value = true
      error.value = ''
      
      // 并行获取数据
      const [statsResponse, ordersResponse, productsResponse] = await Promise.all([
        adminService.getDashboardStats(),
        adminService.getOrders({ limit: 5, sortBy: 'createdAt', sortOrder: 'desc' }),
        adminService.getProductSalesRanking(5)
      ])
      
      // 添加调试信息
      console.log('API响应结果:');
      console.log('统计数据:', statsResponse);
      console.log('订单数据:', ordersResponse);
      console.log('热销商品数据:', productsResponse);
      
      // 更新统计数据
      if (statsResponse) {
        stats.value = {
          ...stats.value,
          ...statsResponse
        }
        console.log('更新后的统计数据:', stats.value);
      }
      
      // 更新最新订单
      if (ordersResponse) {
        recentOrders.value = Array.isArray(ordersResponse) ? ordersResponse : [];
        console.log('更新后的订单数据:', recentOrders.value);
      }
      
      // 更新热销商品
      if (productsResponse) {
        topProducts.value = Array.isArray(productsResponse) ? productsResponse : [];
        console.log('更新后的热销商品数据:', topProducts.value);
      }
    } catch (err) {
      error.value = '加载数据失败，请稍后重试'
      
      // 使用模拟数据作为后备
      stats.value = {
        totalUsers: 1234,
        newUsers: 56,
        totalOrders: 5678,
        pendingOrders: 23,
        totalProducts: 89,
        lowStockProducts: 5,
        totalRevenue: 123456,
        todayRevenue: 2340
      }
      
      recentOrders.value = [
        {
          id: 1001,
          customerName: '张三',
          status: 'pending',
          totalAmount: 299,
          createdAt: new Date().toISOString()
        },
        {
          id: 1002,
          customerName: '李四',
          status: 'processing',
          totalAmount: 599,
          createdAt: new Date(Date.now() - 3600000).toISOString()
        }
      ]
      
      topProducts.value = [
        {
          id: 1,
          name: '红玫瑰束',
          price: 299,
          salesCount: 156,
          image: 'https://images.unsplash.com/photo-1560419015-7c427e8ae5ba?w=50&h=50&auto=format&fit=crop&crop=center'
        },
        {
          id: 2,
          name: '粉色康乃馨',
          price: 199,
          salesCount: 98,
          image: 'https://images.unsplash.com/photo-1587872364533-1b6c9e5c9d1b?w=50&h=50&auto=format&fit=crop&crop=center'
        }
      ]
    } finally {
      loading.value = false
    }
  }

const getStatusClass = (status) => {
  const statusMap = {
    pending: 'bg-yellow-100 text-yellow-800',
    processing: 'bg-blue-100 text-blue-800',
    shipped: 'bg-purple-100 text-purple-800',
    delivered: 'bg-green-100 text-green-800',
    cancelled: 'bg-red-100 text-red-800'
  }
  return statusMap[status] || 'bg-gray-100 text-gray-800'
}

const getStatusText = (status) => {
  const statusMap = {
    pending: '待处理',
    processing: '处理中',
    shipped: '已发货',
    delivered: '已送达',
    cancelled: '已取消'
  }
  return statusMap[status] || status
}

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleString('zh-CN', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const viewOrder = (orderId) => {
  router.push(`/admin/orders/${orderId}`)
}

// 页面加载时获取数据
onMounted(async () => {
  // 仅在开发环境输出初始化日志
  if (import.meta.env.DEV) {
    console.log('Dashboard mounted')
    console.log('当前用户:', userStore.user)
    console.log('Token存在:', !!userStore.token)
    console.log('是管理员:', userStore.isAdmin)
  }
  
  // 直接加载数据，不进行自动登录
  await loadDashboardData()
  
  // 输出最终数据，用于调试
  console.log('最终统计数据:', stats.value)
  console.log('最终订单数据:', recentOrders.value)
  console.log('最终热销商品数据:', topProducts.value)
})
</script>
