<template>
  <div class="min-h-screen bg-gray-50">
    

    <div class="container mx-auto px-4 py-8">
      <!-- 用户统计 -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center">
            <div class="p-3 bg-blue-100 rounded-full">
              <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"></path>
              </svg>
            </div>
            <div class="ml-4">
              <p class="text-sm text-gray-600">总用户数</p>
              <p class="text-2xl font-bold text-gray-800">{{ stats.totalUsers }}</p>
            </div>
          </div>
        </div>
        
        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center">
            <div class="p-3 bg-green-100 rounded-full">
              <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z"></path>
              </svg>
            </div>
            <div class="ml-4">
              <p class="text-sm text-gray-600">今日新增</p>
              <p class="text-2xl font-bold text-gray-800">{{ stats.newUsers }}</p>
            </div>
          </div>
        </div>
        
        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center">
            <div class="p-3 bg-yellow-100 rounded-full">
              <svg class="w-6 h-6 text-yellow-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
            <div class="ml-4">
              <p class="text-sm text-gray-600">活跃用户</p>
              <p class="text-2xl font-bold text-gray-800">{{ stats.activeUsers }}</p>
            </div>
          </div>
        </div>
        
        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center">
            <div class="p-3 bg-huanyu-pink-100 rounded-full">
              <svg class="w-6 h-6 text-huanyu-pink-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
              </svg>
            </div>
            <div class="ml-4">
              <p class="text-sm text-gray-600">VIP用户</p>
              <p class="text-2xl font-bold text-gray-800">{{ stats.vipUsers }}</p>
            </div>
          </div>
        </div>
      </div>

      <!-- 搜索和筛选 -->
      <div class="bg-white rounded-lg shadow p-6 mb-6">
        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
          <input 
            v-model="searchQuery"
            type="text" 
            placeholder="搜索用户名、邮箱或手机号..."
            class="border rounded-lg px-4 py-2"
            @keyup.enter="searchUsers"
          >
          <select v-model="selectedStatus" class="border rounded-lg px-4 py-2" @change="searchUsers">
            <option value="">所有状态</option>
            <option value="active">活跃</option>
            <option value="inactive">不活跃</option>
            <option value="banned">已封禁</option>
          </select>
          <select v-model="selectedRole" class="border rounded-lg px-4 py-2" @change="searchUsers">
            <option value="">所有角色</option>
            <option value="user">普通用户</option>
            <option value="vip">VIP用户</option>
            <option value="admin">管理员</option>
          </select>
          <button 
            @click="searchUsers"
            class="bg-gray-600 hover:bg-gray-700 text-white px-4 py-2 rounded-lg transition-colors"
          >
            搜索
          </button>
        </div>
      </div>

      <!-- 用户列表 -->
  <div class="bg-white rounded-lg shadow overflow-x-auto">
        <div v-if="loading" class="text-center py-8">
          <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-huanyu-pink-600"></div>
          <p class="mt-2 text-gray-600">加载中...</p>
        </div>
        
        <table v-else class="w-full min-w-[1000px]">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">用户</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">联系方式</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">地址</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">角色</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">状态</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">注册时间</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">最后登录</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">操作</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="user in users" :key="user.id">
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <div 
                    :style="{ backgroundImage: `url(${getAvatarUrl(user.avatar)})` }" 
                    :alt="user.username"
                    class="w-10 h-10 bg-center bg-cover rounded-full"
                    @error="handleAvatarError"
                  ></div>
                  <div class="ml-4">
                    <div class="text-sm font-medium text-gray-900">{{ user.username }}</div>
                    <div class="text-sm text-gray-500">ID: {{ user.id }}</div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="text-sm text-gray-900">{{ user.email }}</div>
                <div class="text-sm text-gray-500">{{ user.phone || '未设置' }}</div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div v-if="user.role === 'admin'" class="text-sm text-gray-500">管理员无需收货地址</div>
                <div v-else-if="user.address && user.address.trim().length > 0" class="text-sm text-gray-900">{{ user.address }}</div>
                <div v-else class="text-sm text-gray-500 italic">
                  未设置地址（该地址为动态交互信息字段），请前往
                  <router-link to="/profile" class="text-huanyu-pink-600 hover:text-huanyu-pink-900">个人中心</router-link>
                  添加
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span 
                  :class="getRoleClass(user.role)"
                  class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                >
                  {{ getRoleText(user.role) }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span 
                  :class="getStatusClass(user.status)"
                  class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                >
                  {{ getStatusText(user.status) }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ formatDate(user.createdAt) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ formatDate(user.updatedAt) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <button 
                  @click="viewUser(user)"
                  class="text-huanyu-pink-600 hover:text-huanyu-pink-900 mr-3"
                >
                  查看
                </button>
                <button 
                  @click="editUser(user)"
                  class="text-blue-600 hover:text-blue-900 mr-3"
                >
                  编辑
                </button>
                <button 
                  v-if="user.status === 'active'"
                  @click="updateUserStatus(user.id, 'banned')"
                  class="text-yellow-600 hover:text-yellow-900 mr-3"
                >
                  封禁
                </button>
                <button 
                  v-else-if="user.status === 'banned'"
                  @click="updateUserStatus(user.id, 'active')"
                  class="text-green-600 hover:text-green-900 mr-3"
                >
                  解封
                </button>
                <button 
                  @click="deleteUser(user.id)"
                  class="text-red-600 hover:text-red-900"
                >
                  删除
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- 分页 -->
      <div class="flex justify-center mt-8">
        <div class="flex space-x-2">
          <button 
            @click="changePage(currentPage - 1)"
            :disabled="currentPage <= 1"
            class="px-3 py-2 border rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            上一页
          </button>
          <button 
            v-for="page in totalPages"
            :key="page"
            @click="changePage(page)"
            :class="[
              'px-3 py-2 border rounded-lg',
              page === currentPage 
                ? 'bg-huanyu-pink-600 text-white' 
                : 'hover:bg-gray-50'
            ]"
          >
            {{ page }}
          </button>
          <button 
            @click="changePage(currentPage + 1)"
            :disabled="currentPage >= totalPages"
            class="px-3 py-2 border rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            下一页
          </button>
        </div>
      </div>
    </div>

    <!-- 用户详情模态框 -->
    <div v-if="showUserDetail" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-8 max-w-4xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        <div class="flex justify-between items-center mb-6">
          <h2 class="text-2xl font-bold">用户详情</h2>
          <button 
            @click="showUserDetail = false"
            class="text-gray-400 hover:text-gray-600"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
          </button>
        </div>

        <div v-if="selectedUser" class="space-y-6">
          <!-- 基本信息 -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div class="text-center">
              <div 
                :style="{ backgroundImage: `url(${getAvatarUrl(selectedUser.avatar)})` }" 
                :alt="selectedUser.username"
                class="w-24 h-24 bg-center bg-cover rounded-full mx-auto mb-4"
                @error="handleAvatarError"
              ></div>
              <h3 class="text-xl font-semibold">{{ selectedUser.username }}</h3>
              <p class="text-gray-500">用户ID: {{ selectedUser.id }}</p>
            </div>
            <div>
              <h4 class="font-semibold mb-3">基本信息</h4>
              <div class="space-y-2 text-sm">
                <div><span class="text-gray-600">真实姓名：</span>{{ selectedUser.fullName || '未设置' }}</div>
                <div><span class="text-gray-600">邮箱：</span>{{ selectedUser.email }}</div>
                <div><span class="text-gray-600">手机：</span>{{ selectedUser.phone || '未设置' }}</div>
                <div>
                  <span class="text-gray-600">地址：</span>
                  <span v-if="selectedUser.role === 'admin'" class="text-gray-500">管理员无需收货地址</span>
                  <span v-else-if="selectedUser.address && selectedUser.address.trim().length > 0">{{ selectedUser.address }}</span>
                  <span v-else class="text-gray-500 italic">
                    未设置地址（该地址为动态交互信息字段），请前往
                    <router-link to="/profile" class="text-huanyu-pink-600 hover:text-huanyu-pink-900">个人中心</router-link>
                    添加
                  </span>
                </div>
                <div><span class="text-gray-600">角色：</span>{{ getRoleText(selectedUser.role) }}</div>
                <div><span class="text-gray-600">状态：</span>{{ getStatusText(selectedUser.status) }}</div>
              </div>
            </div>
            <div>
              <h4 class="font-semibold mb-3">账户信息</h4>
              <div class="space-y-2 text-sm">
                <div><span class="text-gray-600">注册时间：</span>{{ formatDate(selectedUser.createdAt) }}</div>
                <div><span class="text-gray-600">更新时间：</span>{{ formatDate(selectedUser.updatedAt) }}</div>
              </div>
            </div>
          </div>

          <!-- 账户信息 -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <h4 class="font-semibold mb-3">账户信息</h4>
              <div class="space-y-2 text-sm">
                <div><span class="text-gray-600">注册时间：</span>{{ formatDate(selectedUser.createdAt) }}</div>
                <div><span class="text-gray-600">更新时间：</span>{{ formatDate(selectedUser.updatedAt) }}</div>
                <div><span class="text-gray-600">最后登录：</span>{{ formatDate(selectedUser.lastLoginAt) }}</div>
                <div><span class="text-gray-600">邮箱验证：</span>{{ selectedUser.emailVerified ? '已验证' : '未验证' }}</div>
              </div>
            </div>
            <div>
              <h4 class="font-semibold mb-3">收货地址</h4>
              <div class="space-y-2 text-sm">
                <div v-if="selectedUser.addresses && selectedUser.addresses.length > 0">
                  <div v-for="(address, index) in selectedUser.addresses" :key="index" class="mb-2 p-2 border rounded">
                    <div class="font-medium">{{ address.recipientName }} {{ address.phoneNumber }}</div>
                    <div class="text-gray-600">{{ address.province }} {{ address.city }} {{ address.district }}</div>
                    <div class="text-gray-600">{{ address.detailAddress }}</div>
                    <div class="text-xs text-gray-500">{{ address.isDefault ? '默认地址' : '' }}</div>
                  </div>
                </div>
                <div v-else class="text-gray-500">暂无收货地址</div>
              </div>
            </div>
          </div>

          <!-- 订单统计 -->
          <div>
            <h4 class="font-semibold mb-3">订单统计</h4>
            <div class="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
              <div class="text-center p-3 bg-gray-50 rounded">
                <div class="font-semibold text-lg">{{ selectedUser.totalOrders || 0 }}</div>
                <div class="text-gray-600">总订单数</div>
              </div>
              <div class="text-center p-3 bg-green-50 rounded">
                <div class="font-semibold text-lg text-green-600">{{ selectedUser.completedOrders || 0 }}</div>
                <div class="text-gray-600">已完成</div>
              </div>
              <div class="text-center p-3 bg-blue-50 rounded">
                <div class="font-semibold text-lg text-blue-600">{{ selectedUser.totalSpent || 0 }}</div>
                <div class="text-gray-600">累计消费</div>
              </div>
              <div class="text-center p-3 bg-purple-50 rounded">
                <div class="font-semibold text-lg text-purple-600">{{ selectedUser.points || 0 }}</div>
                <div class="text-gray-600">积分</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 编辑用户模态框 -->
    <div v-if="showEditModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 w-full max-w-md mx-4">
        <div class="flex justify-between items-center mb-4">
          <h3 class="text-lg font-semibold">编辑用户</h3>
          <button @click="closeEdit" class="text-gray-400 hover:text-gray-600">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
          </button>
        </div>

        <div class="space-y-4">
          <div>
            <label class="text-sm text-gray-700">用户名</label>
            <div class="mt-1 px-3 py-2 border rounded bg-gray-50">{{ editForm.username }}</div>
          </div>

          <div class="flex items-center justify-between">
            <label class="flex items-center">
              <input type="checkbox" v-model="editForm.isVip" class="mr-2" />
              <span class="text-sm">设为 VIP 用户</span>
            </label>
            <span :class="getRoleClass(editForm.isVip ? 'vip' : 'user')" class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full">
              {{ getRoleText(editForm.isVip ? 'vip' : 'user') }}
            </span>
          </div>

          <div class="flex items-center justify-between">
            <label class="flex items-center">
              <input type="checkbox" v-model="editForm.resetPassword" class="mr-2" />
              <span class="text-sm">重置密码为 123456</span>
            </label>
          </div>

          <div class="flex justify-end space-x-2 mt-4">
            <button @click="closeEdit" class="px-4 py-2 border rounded hover:bg-gray-50">取消</button>
            <button @click="saveEdit" class="px-4 py-2 bg-huanyu-pink-600 text-white rounded hover:bg-huanyu-pink-700">保存</button>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import AdminNav from '@/components/admin/AdminNav.vue'
import { getAvatarUrl, handleAvatarError } from '@/utils/avatar.js'
import adminService from '@/services/adminService.js'
import { ElMessage } from 'element-plus'

// 响应式数据
const searchQuery = ref('')
const selectedStatus = ref('')
const selectedRole = ref('')
const showUserDetail = ref(false)
const selectedUser = ref(null)
const showEditModal = ref(false)
const editForm = ref({ userId: null, username: '', isVip: false, resetPassword: false })
const loading = ref(false)
const users = ref([])
const currentPage = ref(1)
const totalPages = ref(1)
const totalUsers = ref(0)
const pageSize = 10

// 统计数据
const stats = ref({
  totalUsers: 0,
  newUsers: 0,
  activeUsers: 0,
  vipUsers: 0
})

// 方法
const getGenderText = (gender) => {
  const genderMap = {
    male: '男',
    female: '女',
    other: '其他'
  }
  return genderMap[gender] || '未设置'
}

const getRoleClass = (role) => {
  const roleMap = {
    user: 'bg-gray-100 text-gray-800',
    vip: 'bg-yellow-100 text-yellow-800',
    admin: 'bg-red-100 text-red-800'
  }
  return roleMap[role] || 'bg-gray-100 text-gray-800'
}

const getRoleText = (role) => {
  const roleMap = {
    user: '普通用户',
    vip: 'VIP用户',
    admin: '管理员'
  }
  return roleMap[role] || role
}

const getStatusClass = (status) => {
  const statusMap = {
    active: 'bg-green-100 text-green-800',
    inactive: 'bg-gray-100 text-gray-800',
    banned: 'bg-red-100 text-red-800'
  }
  return statusMap[status] || 'bg-gray-100 text-gray-800'
}

const getStatusText = (status) => {
  const statusMap = {
    active: '活跃',
    inactive: '不活跃',
    banned: '已封禁'
  }
  return statusMap[status] || status
}

    // 格式化日期
    const formatDate = (dateString) => {
      if (!dateString) return '未设置'
      return new Date(dateString).toLocaleDateString('zh-CN')
    }

// 获取统计数据
const fetchStats = async () => {
  let statsLoading = true
  try {
    if (import.meta.env.DEV) {
      console.log('开始获取统计数据...')
    }
    const response = await adminService.getDashboardStats()
    if (import.meta.env.DEV) {
      console.log('统计数据响应:', response)
    }
    if (response) {
      stats.value = {
        totalUsers: response.totalUsers || 0,
        newUsers: response.newUsers || 0,
        activeUsers: response.activeUsers || 0,
        vipUsers: response.vipUsers || 0
      }
      if (import.meta.env.DEV) {
        console.log('统计数据更新成功:', stats.value)
      }
    }
  } catch (error) {
    // 优化错误处理
    if (import.meta.env.DEV) {
      console.error('获取统计数据失败:', error.message)
      console.error('错误详情:', error.response?.data)
      console.error('错误状态:', error.status || error.response?.status)
    }
    
    // 使用ElMessage显示错误信息
    const errorMessage = error.response?.data?.message || error.message || '获取统计数据失败'
    ElMessage.error(errorMessage)
    
    // 设置默认值避免显示错误
    stats.value = {
      totalUsers: 0,
      newUsers: 0,
      activeUsers: 0,
      vipUsers: 0
    }
  } finally {
    statsLoading = false
  }
}

// 获取用户列表
const fetchUsers = async () => {
  loading.value = true
  try {
    // 仅在开发环境输出日志
    if (import.meta.env.DEV) {
      console.log('开始获取用户数据...')
    }
    
    const response = await adminService.getAdminUsers()
    
    if (import.meta.env.DEV) {
      console.log('获取用户数据响应:', response)
    }
    
    // 处理响应数据 - 响应拦截器已经返回了response.data
    let userData = []
    if (Array.isArray(response)) {
      userData = response
    } else if (response && response.data) {
      userData = Array.isArray(response.data) ? response.data : response.data.users || []
    } else if (response && Array.isArray(response.users)) {
      userData = response.users
    } else if (response) {
      // 添加额外的回退逻辑，确保即使响应格式异常也能处理
      userData = [response]
    }
    
    users.value = userData
    totalUsers.value = users.value.length
    totalPages.value = Math.ceil(totalUsers.value / pageSize)
    
    // 仅在开发环境输出日志
    if (import.meta.env.DEV) {
      console.log('用户数据加载成功:', users.value.length, '个用户')
    }
  } catch (error) {
    // 错误处理优化
    if (import.meta.env.DEV) {
      console.error('获取用户列表失败:', error.message)
      console.error('错误详情:', error.data || error.response?.data || '无详细信息')
      console.error('错误状态:', error.status || error.response?.status)
    }
    
    // 显示更友好的错误信息，使用ElMessage
    const errorMessage = error.data?.message || error.response?.data?.message || error.message || '获取用户列表失败'
    ElMessage.error(errorMessage)
    
    // 错误时设置默认值避免界面显示异常
    users.value = []
    totalUsers.value = 0
    totalPages.value = 1
  } finally {
    loading.value = false
  }
}

// 搜索用户
const searchUsers = async () => {
  // 这里可以实现搜索逻辑，暂时重新加载所有用户
  currentPage.value = 1
  await fetchUsers()
}

// 切换页面
const changePage = (page) => {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
    fetchUsers()
  }
}

// 查看用户详情
const viewUser = async (user) => {
  try {
    const response = await adminService.getAdminUserById(user.id)
    if (response) {
      selectedUser.value = response.data || response
      showUserDetail.value = true
    }
  } catch (error) {
    // 仅在开发环境输出错误日志
    if (import.meta.env.DEV) {
      console.error('获取用户详情失败:', error)
    }
    ElMessage.error('获取用户详情失败')
  }
}

// 编辑用户
  const editUser = (user) => {
    selectedUser.value = user
    editForm.value = {
      userId: user.id,
      username: user.username,
      isVip: user.role === 'vip',
      resetPassword: false
    }
    showEditModal.value = true
  }

const closeEdit = () => { showEditModal.value = false }

const saveEdit = async () => {
  try {
    const id = editForm.value.userId
    const role = editForm.value.isVip ? 'vip' : 'user'
    await adminService.updateUserRole(id, role)
    if (editForm.value.resetPassword) {
      await adminService.resetUserPassword(id, '123456')
    }
    ElMessage.success('用户信息已更新')
    showEditModal.value = false
    await fetchUsers()
    await fetchStats()
    try { localStorage.setItem('userProfileUpdated', String(Date.now())) } catch {}
  } catch (error) {
    ElMessage.error(error?.response?.data?.message || '更新失败')
  }
}

// 更新用户状态
const updateUserStatus = async (userId, status) => {
  const statusText = status === 'banned' ? '封禁' : '解封'
  if (!confirm(`确定要${statusText}这个用户吗？`)) {
    return
  }

  try {
    const response = await adminService.updateUserStatus(userId, status)
    if (response) {
      alert(`用户已${statusText}`)
      await fetchUsers() // 重新加载用户列表
      await fetchStats() // 重新加载统计数据
    }
  } catch (error) {
    // 仅在开发环境输出错误日志
    if (import.meta.env.DEV) {
      console.error(`${statusText}用户失败:`, error)
    }
    ElMessage.error(`${statusText}用户失败`)
  }
}

// 删除用户
const deleteUser = async (userId) => {
  if (!confirm('确定要删除这个用户吗？此操作不可恢复！')) {
    return
  }

  try {
    const response = await adminService.deleteUser(userId)
    if (response) {
      alert('用户已删除')
      await fetchUsers() // 重新加载用户列表
      await fetchStats() // 重新加载统计数据
    }
  } catch (error) {
    // 仅在开发环境输出错误日志
    if (import.meta.env.DEV) {
      console.error('删除用户失败:', error)
      console.error('错误详情:', error.response?.data)
      console.error('错误状态:', error.response?.status)
    }
    
    // 显示更友好的错误信息
    const errorMessage = error.response?.data?.message || error.message || '未知错误'
    alert('删除用户失败: ' + errorMessage)
  }
}

// 页面加载时获取数据
onMounted(async () => {
  // 初始加载时获取最新数据
  await refreshData()
  
  // 监听用户信息更新事件
  const handleStorageChange = (e) => {
    // 检查是否是用户资料更新事件
    if (e.key === 'userProfileUpdated') {
      if (import.meta.env.DEV) {
        console.log('检测到用户资料更新，刷新数据...')
      }
      refreshData() // 刷新所有数据
    }
    // 也可以响应localStorage中user对象的变化
    else if (e.key === 'user') {
      if (import.meta.env.DEV) {
        console.log('检测到用户对象更新，刷新数据...')
      }
      // 延迟一小段时间确保数据已完全更新
      setTimeout(() => refreshData(), 100)
    }
  }
  
  // 添加存储事件监听器
  window.addEventListener('storage', handleStorageChange)
  
  // 组件卸载时移除监听器
  onUnmounted(() => {
    window.removeEventListener('storage', handleStorageChange)
  })
})

// 刷新所有数据的辅助函数
let lastRefreshTs = 0
let refreshing = false
const refreshData = async () => {
  const now = Date.now()
  if (refreshing || (now - lastRefreshTs < 1500)) return
  refreshing = true
  loading.value = true
  try {
    await fetchUsers()
    await new Promise(r => setTimeout(r, 250))
    await fetchStats()
    lastRefreshTs = Date.now()
  } catch (error) {
    if (import.meta.env.DEV) {
      console.error('刷新数据失败:', error)
    }
  } finally {
    loading.value = false
    refreshing = false
  }
}
</script>
