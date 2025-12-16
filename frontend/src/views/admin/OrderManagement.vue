<template>
  <div class="min-h-screen bg-gray-50">
    

    <div class="container mx-auto px-4 py-8">
      <!-- 订单统计 -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center">
            <div class="p-3 bg-blue-100 rounded-full">
              <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"></path>
              </svg>
            </div>
            <div class="ml-4">
              <p class="text-sm text-gray-600">总订单数</p>
              <p class="text-2xl font-bold text-gray-800">{{ totalOrders }}</p>
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
              <p class="text-sm text-gray-600">待处理</p>
              <p class="text-2xl font-bold text-gray-800">{{ pendingOrders }}</p>
            </div>
          </div>
        </div>
        
        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center">
            <div class="p-3 bg-green-100 rounded-full">
              <svg class="w-6 h-6 text-green-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
              </svg>
            </div>
            <div class="ml-4">
              <p class="text-sm text-gray-600">已完成</p>
              <p class="text-2xl font-bold text-gray-800">{{ completedOrders }}</p>
            </div>
          </div>
        </div>
        
        <div class="bg-white rounded-lg shadow p-6">
          <div class="flex items-center">
            <div class="p-3 bg-huanyu-pink-100 rounded-full">
              <svg class="w-6 h-6 text-huanyu-pink-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
            <div class="ml-4">
              <p class="text-sm text-gray-600">总收入</p>
              <p class="text-2xl font-bold text-gray-800">¥{{ totalRevenue }}</p>
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
            placeholder="搜索订单号或用户名..."
            class="border rounded-lg px-4 py-2"
          >
          <select v-model="selectedStatus" class="border rounded-lg px-4 py-2">
            <option value="">所有状态</option>
            <option value="pending">待处理</option>
            <option value="processing">处理中</option>
            <option value="shipped">已发货</option>
            <option value="delivered">已送达</option>
            <option value="cancelled">已取消</option>
          </select>
          <input 
            v-model="dateRange"
            type="date" 
            class="border rounded-lg px-4 py-2"
          >
          <button 
            @click="searchOrders"
            class="bg-gray-600 hover:bg-gray-700 text-white px-4 py-2 rounded-lg transition-colors"
          >
            搜索
          </button>
        </div>
      </div>

      <!-- 订单列表 -->
      <div class="bg-white rounded-lg shadow overflow-x-auto relative">
        <!-- 加载遮罩层 - 正确放置在容器内，表格外 -->
        <div v-if="loading" class="absolute inset-0 bg-white bg-opacity-70 flex items-center justify-center z-10">
          <div class="text-center">
            <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-huanyu-pink-600"></div>
            <p class="mt-2 text-gray-600">加载中...</p>
          </div>
        </div>
        
        <table class="w-full min-w-[1200px]">
          <thead class="bg-gray-50">
            <tr>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">订单号</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">用户</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">商品</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">金额</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">收货地址</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">状态</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">下单时间</th>
              <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">操作</th>
            </tr>
          </thead>
          <tbody class="bg-white divide-y divide-gray-200">
            <tr v-for="order in orders" :key="order.id">
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                #{{ order.orderNumber }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <div class="flex items-center">
                  <div 
                    :style="{ backgroundImage: `url(${getAvatarUrl(order.user?.avatar)})` }" 
                    :alt="order.user?.name || '用户'"
                    class="w-8 h-8 bg-center bg-cover rounded-full"
                    @error="handleAvatarError"
                  ></div>
                  <div class="ml-2">
                    <div class="text-sm font-medium text-gray-900">{{ order.customerName || order.user?.name || '未知用户' }}</div>
                    <div class="text-sm text-gray-500">{{ order.recipientPhone || order.user?.phone || '无电话' }}</div>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="text-sm text-gray-900">
                  {{ order.orderItems?.length || order.items?.length || 0 }} 件商品
                </div>
                <div class="text-sm text-gray-500">
                  {{ (order.orderItems?.[0]?.productName || order.items?.[0]?.name || '商品') }} 等
                </div>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                <div class="font-medium">¥{{ order.totalAmount }}</div>
                <div v-if="order.discountAmount > 0" class="text-xs text-red-500">
                  (已减 ¥{{ order.discountAmount }})
                </div>
              </td>
              <td class="px-6 py-4 text-sm text-gray-900 max-w-xs truncate">
                {{ order.user?.address || order.deliveryAddress || '无地址' }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap">
                <span 
                  :class="getStatusClass(order.status)"
                  class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                >
                  {{ getStatusText(order.status) }}
                </span>
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                {{ formatDate(order.createdAt) }}
              </td>
              <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <button 
                  @click="viewOrder(order)"
                  class="text-huanyu-pink-600 hover:text-huanyu-pink-900 mr-3"
                >
                  查看
                </button>
                <button 
                  v-if="order.status === 'pending'"
                  @click="openProcessModal(order)"
                  class="text-green-600 hover:text-green-900 mr-3"
                >
                  处理
                </button>
                <button 
                  v-if="order.status === 'processing'"
                  @click="shipOrder(order.id)"
                  class="text-blue-600 hover:text-blue-900 mr-3"
                >
                  发货
                </button>
                <button 
                  v-if="order.status === 'pending' || order.status === 'processing'"
                  @click="openRejectModal(order)"
                  class="text-red-600 hover:text-red-900"
                >
                  拒绝发货
                </button>
                <button 
                  @click="openDeleteModal(order)"
                  class="text-gray-600 hover:text-gray-900 ml-3"
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
        <div class="flex items-center space-x-2">
          <button 
            @click="handlePageChange(currentPage - 1)"
            :disabled="currentPage <= 1"
            class="px-3 py-2 border rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            上一页
          </button>
          
          <span class="px-4 py-2 text-sm text-gray-700">
            第 {{ currentPage }} 页，共 {{ Math.ceil(total / pageSize) }} 页
          </span>
          
          <button 
            @click="handlePageChange(currentPage + 1)"
            :disabled="currentPage >= Math.ceil(total / pageSize)"
            class="px-3 py-2 border rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            下一页
          </button>
        </div>
      </div>
    </div>

    <!-- 订单详情模态框 -->
    <div v-if="showOrderDetail" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-8 max-w-4xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        <div class="flex justify-between items-center mb-6">
          <h2 class="text-2xl font-bold">订单详情 #{{ selectedOrder?.orderNumber }}</h2>
          <button 
            @click="showOrderDetail = false"
            class="text-gray-400 hover:text-gray-600"
          >
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
          </button>
        </div>

        <div v-if="loadingOrderDetail" class="flex justify-center items-center py-12">
          <div class="text-center">
            <div class="inline-block animate-spin rounded-full h-12 w-12 border-4 border-t-huanyu-pink-600 border-gray-200 mb-4"></div>
            <p>加载订单详情中...</p>
          </div>
        </div>
        <div v-else-if="selectedOrder" class="space-y-6">
          <!-- 订单信息 -->
          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <h3 class="font-semibold mb-3">订单信息</h3>
              <div class="space-y-2 text-sm">
                <div><span class="text-gray-600">订单号：</span>{{ selectedOrder.orderNumber }}</div>
                <div><span class="text-gray-600">下单时间：</span>{{ formatDate(selectedOrder.createdAt) }}</div>
                <div><span class="text-gray-600">订单状态：</span>{{ getStatusText(selectedOrder.status) }}</div>
                <div><span class="text-gray-600">支付方式：</span>微信支付</div>
                <div v-if="latestHistory">
                  <span class="text-gray-600">最新处理备注：</span>
                  <span class="text-gray-800">{{ latestHistory.note || '无' }}</span>
                  <span class="text-gray-500 ml-2">({{ getStatusText(latestHistory.newStatus) }} · {{ formatDate(latestHistory.createdAt) }} · {{ latestHistory.operatorName }})</span>
                </div>
              </div>
            </div>
            <div>
              <h3 class="font-semibold mb-3">收货信息</h3>
              <div class="space-y-2 text-sm">
                <div><span class="text-gray-600">收货人：</span>{{ selectedOrder.recipientName || selectedOrder.address?.name || '未知' }}</div>
                <div><span class="text-gray-600">联系电话：</span>{{ selectedOrder.recipientPhone || selectedOrder.address?.phone || '未知' }}</div>
                <div><span class="text-gray-600">收货地址：</span>{{ selectedOrder.user?.address || selectedOrder.deliveryAddress || '未知' }}</div>
                <div><span class="text-gray-600">配送时间：</span>{{ selectedOrder.address?.deliveryTime || '尽快配送' }}</div>
              </div>
            </div>
          </div>

          <!-- 商品列表 -->
          <div>
            <h3 class="font-semibold mb-3">商品列表</h3>
            <div class="border rounded-lg overflow-hidden">
              <table class="w-full">
                <thead class="bg-gray-50">
                  <tr>
                    <th class="px-4 py-2 text-left text-sm">商品</th>
                    <th class="px-4 py-2 text-left text-sm">单价</th>
                    <th class="px-4 py-2 text-left text-sm">数量</th>
                    <th class="px-4 py-2 text-left text-sm">小计</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in (selectedOrder.orderItems || selectedOrder.items)" :key="item.id" class="border-t">
                    <td class="px-4 py-2">
                      <div class="flex items-center">
                        <div class="w-12 h-12">
                          <SmartImage :src="item.productImage || item.image || '/images/product-placeholder.svg'" :alt="item.productName || item.name" ratio="auto" strategy="auto" />
                        </div>
                        <div class="ml-3">
                          <div class="text-sm font-medium">{{ item.productName || item.name }}</div>
                          <div class="text-xs" :class="(item.stock !== undefined && item.quantity > item.stock) ? 'text-red-600' : 'text-gray-500'">
                            库存：{{ getItemStockText(item) }}
                          </div>
                          <div class="text-xs text-gray-500">{{ item.description || '精美花卉' }}</div>
                        </div>
                      </div>
                    </td>
                    <td class="px-4 py-2 text-sm">¥{{ item.unitPrice || item.price }}</td>
                    <td class="px-4 py-2 text-sm">{{ item.quantity }}</td>
                    <td class="px-4 py-2 text-sm">¥{{ (item.unitPrice || item.price) * item.quantity }}</td>
                  </tr>
                </tbody>
                <tfoot class="bg-gray-50">
                  <tr>
                    <td colspan="3" class="px-4 py-2 text-right font-medium">商品总额：</td>
                    <td class="px-4 py-2">¥{{ selectedOrder.subtotal || selectedOrder.totalAmount }}</td>
                  </tr>
                  <tr>
                    <td colspan="3" class="px-4 py-2 text-right font-medium">配送费：</td>
                    <td class="px-4 py-2">¥{{ selectedOrder.deliveryFee || 0 }}</td>
                  </tr>
                  <tr class="font-bold">
                    <td colspan="3" class="px-4 py-2 text-right">总计：</td>
                    <td class="px-4 py-2 text-huanyu-pink-600">¥{{ selectedOrder.totalAmount }}</td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!-- 处理订单弹窗 -->
    <div v-if="showProcessModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 w-full max-w-md mx-4">
        <div class="flex justify-between items-center mb-4">
          <h3 class="text-lg font-semibold">处理订单 #{{ processTargetOrder?.orderNumber }}</h3>
          <button @click="closeProcessModal" class="text-gray-400 hover:text-gray-600">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
          </button>
        </div>
        <div class="space-y-4">
          <div>
            <label class="block text-sm text-gray-700 mb-2">处理备注</label>
            <textarea v-model="processForm.note" rows="3" class="w-full border rounded-lg px-3 py-2"></textarea>
          </div>
          <div class="flex items-center">
            <input id="ck-ship-immediately-2" type="checkbox" v-model="processForm.shipNow" class="mr-2" />
            <label for="ck-ship-immediately-2" class="text-sm">立即发货</label>
          </div>
          <div v-if="processForm.shipNow" class="grid grid-cols-1 md:grid-cols-2 gap-3">
            <div>
              <label class="block text-sm text-gray-700 mb-2">快递公司</label>
              <input v-model="processForm.carrier" type="text" class="w-full border rounded-lg px-3 py-2" placeholder="顺丰/圆通/京东物流" />
            </div>
            <div>
              <label class="block text-sm text-gray-700 mb-2">快递单号</label>
              <input v-model="processForm.tracking" type="text" class="w-full border rounded-lg px-3 py-2" placeholder="SF1234567890" />
            </div>
          </div>
        </div>
        <div class="mt-6 flex justify-end space-x-3">
          <button @click="closeProcessModal" class="px-4 py-2 border rounded-lg hover:bg-gray-50">取消</button>
          <button @click="submitProcess" class="px-4 py-2 bg-green-600 hover:bg-green-700 text-white rounded-lg">提交</button>
        </div>
      </div>
    </div>

    <!-- 拒绝发货弹窗 -->
    <div v-if="showRejectModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 w-full max-w-md mx-4">
        <div class="flex justify-between items-center mb-4">
          <h3 class="text-lg font-semibold">拒绝发货 #{{ rejectTargetOrder?.orderNumber }}</h3>
          <button @click="closeRejectModal" class="text-gray-400 hover:text-gray-600">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
          </button>
        </div>
        <div>
          <label class="block text-sm text-gray-700 mb-2">拒绝发货理由</label>
          <textarea v-model="rejectForm.reason" rows="3" class="w-full border rounded-lg px-3 py-2" placeholder="例如：库存不足/地址异常/风险订单等"></textarea>
        </div>
        <div class="mt-6 flex justify-end space-x-3">
          <button @click="closeRejectModal" class="px-4 py-2 border rounded-lg hover:bg-gray-50">取消</button>
          <button @click="submitReject" class="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg">提交</button>
        </div>
      </div>
    </div>

    <!-- 删除订单弹窗 -->
    <div v-if="showDeleteModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 w-full max-w-md mx-4">
        <div class="flex justify-between items-center mb-4">
          <h3 class="text-lg font-semibold">删除订单 #{{ deleteTargetOrder?.orderNumber }}</h3>
          <button @click="closeDeleteModal" class="text-gray-400 hover:text-gray-600">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
          </button>
        </div>
        <p class="text-sm text-gray-700">确认删除该订单？此操作可能不可恢复。</p>
        <div class="mt-6 flex justify-end space-x-3">
          <button @click="closeDeleteModal" class="px-4 py-2 border rounded-lg hover:bg-gray-50">取消</button>
          <button @click="submitDelete" class="px-4 py-2 bg-gray-700 hover:bg-gray-800 text-white rounded-lg">删除</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed, watch } from 'vue'
import { getAvatarUrl } from '@/utils/avatar.js'
import AdminNav from '@/components/admin/AdminNav.vue'
import adminService from '@/services/adminService.js'
import { productService } from '@/services/product'
import { ElMessage, ElMessageBox } from 'element-plus'

// 计算统计数据
const totalOrders = computed(() => statistics.value.totalOrders)
const pendingOrders = computed(() => statistics.value.pendingOrders)
const completedOrders = computed(() => statistics.value.completedOrders)
const totalRevenue = computed(() => statistics.value.totalRevenue)

// 响应式数据
const orders = ref([])
const loading = ref(false)
const loadingOrderDetail = ref(false)
const selectedOrder = ref(null)
const showOrderDetail = ref(false)
const latestHistory = ref(null)
const actionLock = ref(false)
const showProcessModal = ref(false)
const processForm = ref({ note: '', shipNow: false, carrier: '', tracking: '' })
const processTargetOrder = ref(null)
const showRejectModal = ref(false)
const rejectForm = ref({ reason: '' })
const rejectTargetOrder = ref(null)
const searchQuery = ref('')
const selectedStatus = ref('') // 修复变量名
const dateRange = ref('')
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

// 统计数据
const statistics = ref({
  totalOrders: 0,
  pendingOrders: 0,
  completedOrders: 0,
  totalRevenue: 0
})

// 定时器
let refreshTimer = null
const POLLING_INTERVAL = 15000 // 15秒轮询一次

// 获取订单数据
const fetchOrders = async () => {
  try {
    loading.value = true
    const response = await adminService.getAdminOrders({
      page: currentPage.value,
      pageSize: pageSize.value,
      search: searchQuery.value,
      status: selectedStatus.value,
      startDate: dateRange.value,
      endDate: dateRange.value
    })
    
    // 处理API响应数据结构
    const responseData = response.data
    let newOrders = []
    
    if (responseData && responseData.data) {
      newOrders = responseData.data.map(order => ({
        ...order,
        // 确保订单项数据结构正确
        orderItems: order.orderItems || order.items || [],
        // 确保用户信息正确
        user: order.user || {},
        // 确保地址信息正确
        address: order.address || {}
      }))
      total.value = responseData.total || responseData.pagination?.total || 0
    } else if (responseData && Array.isArray(responseData)) {
      // 如果直接返回数组
      newOrders = responseData.map(order => ({
        ...order,
        orderItems: order.orderItems || order.items || [],
        user: order.user || {},
        address: order.address || {}
      }))
      total.value = responseData.length
    } else {
      newOrders = []
      total.value = 0
    }
    
    // 使用Vue的响应式方式更新数组，避免完全替换导致的闪烁
    // 先清空数组
    orders.value.splice(0, orders.value.length)
    // 然后逐个添加新数据
    newOrders.forEach(order => {
      orders.value.push(order)
    })
    
    // 更新统计数据
    updateStatistics()
  } catch (error) {
    console.error('获取订单数据失败:', error)
    ElMessage.error('获取订单数据失败: ' + (error.response?.data?.message || error.message))
    orders.value = []
    total.value = 0
  } finally {
    loading.value = false
  }
}

// 更新统计数据
const updateStatistics = () => {
  statistics.value.totalOrders = orders.value.length
  statistics.value.pendingOrders = orders.value.filter(order => order.status === 'processing').length
  statistics.value.completedOrders = orders.value.filter(order => order.status === 'delivered').length
  statistics.value.totalRevenue = orders.value
    .filter(order => order.status !== 'cancelled')
    .reduce((sum, order) => sum + (order.totalAmount || 0), 0)
}

// 查看订单详情
const viewOrder = async (order) => {
  showOrderDetail.value = true
  loadingOrderDetail.value = true
  
  try {
    // 先显示基础信息，避免等待
    selectedOrder.value = { ...order }
    
    // 通过API获取完整的订单详情
    const response = await adminService.getAdminOrderById(order.id)
    const detailedOrder = response.data || response
    
    // 确保数据结构完整
    selectedOrder.value = {
      ...detailedOrder,
      orderItems: detailedOrder.orderItems || detailedOrder.items || [],
      user: detailedOrder.user || {},
      address: detailedOrder.address || {}
    }
    try {
      const ids = (selectedOrder.value.orderItems || []).map(x => x.productId || x.ProductId).filter(Boolean)
      const details = await Promise.all(ids.map(async pid => { try { const r = await productService.getProductById(pid); const p = r?.data || r; return { pid, stock: Number(p?.Stock ?? p?.stock ?? 0) } } catch { return { pid, stock: undefined } } }))
      const stocks = Object.fromEntries(details.map(d => [d.pid, d.stock]))
      selectedOrder.value.orderItems = (selectedOrder.value.orderItems || []).map(it => ({ ...it, stock: stocks[it.productId || it.ProductId] }))
    } catch {}

    // 若订单未包含收货地址，则尝试从用户默认地址补全
    try {
      const uid = selectedOrder.value.UserId || selectedOrder.value.userId || order.UserId || order.userId
      if (uid && (!selectedOrder.value.DeliveryAddress && !selectedOrder.value.deliveryAddress)) {
        const userRes = await adminService.getAdminUserById(uid)
        const userDetail = userRes?.data || userRes
        const addrs = userDetail?.addresses || []
        const def = addrs.find(a => a.isDefault) || addrs[0]
        if (def) {
          const full = `${def.province || ''}${def.city || ''}${def.district || ''}${def.detailAddress || ''}`
          selectedOrder.value.deliveryAddress = full
          selectedOrder.value.recipientName = def.recipientName || selectedOrder.value.recipientName
          selectedOrder.value.recipientPhone = def.phoneNumber || selectedOrder.value.recipientPhone
        }
      }
    } catch (e) {
      // 静默失败，不影响详情显示
    }

    const hist = await adminService.getLatestOrderHistory(order.id)
    latestHistory.value = hist?.data || hist || null
  } catch (error) {
    console.error('获取订单详情失败:', error)
    ElMessage.warning('无法获取完整订单详情，显示基础信息')
    // 即使API调用失败，仍然显示基础信息
    if (!selectedOrder.value) {
      selectedOrder.value = { ...order }
    }
  } finally {
    loadingOrderDetail.value = false
  }
}

// 移除重复的函数定义，保留后面完整的实现

// 搜索订单
const searchOrders = () => {
  currentPage.value = 1
  fetchOrders()
}

// 重置筛选
const resetFilters = () => {
  searchQuery.value = ''
  selectedStatus.value = ''
  dateRange.value = ''
  currentPage.value = 1
  fetchOrders()
}

// 分页处理
const handlePageChange = (page) => {
  currentPage.value = page
  fetchOrders()
}

const handleSizeChange = (size) => {
  pageSize.value = size
  currentPage.value = 1
  fetchOrders()
}

// 启动定时刷新
const startPolling = () => {
  // 使用定义的轮询间隔
  refreshTimer = setInterval(() => {
    fetchOrders()
  }, POLLING_INTERVAL)
}

// 停止定时刷新
const stopPolling = () => {
  if (refreshTimer) {
    clearInterval(refreshTimer)
    refreshTimer = null
  }
}

let detailTimer = null
const ORDER_DETAIL_POLL_MS = 5000
const startDetailPolling = (orderId) => {
  stopDetailPolling()
  detailTimer = setInterval(async () => {
    try {
      const res = await adminService.getAdminOrderById(orderId)
      const d = res?.data || res
      if (d) {
        selectedOrder.value = {
          ...selectedOrder.value,
          ...d,
          orderItems: d.orderItems || d.items || [],
          user: d.user || {},
          address: d.address || {}
        }
        try {
          const ids = (selectedOrder.value.orderItems || []).map(x => x.productId || x.ProductId).filter(Boolean)
          const details = await Promise.all(ids.map(async pid => { try { const r = await productService.getProductById(pid); const p = r?.data || r; return { pid, stock: Number(p?.Stock ?? p?.stock ?? 0) } } catch { return { pid, stock: undefined } } }))
          const stocks = Object.fromEntries(details.map(d => [d.pid, d.stock]))
          selectedOrder.value.orderItems = (selectedOrder.value.orderItems || []).map(it => ({ ...it, stock: stocks[it.productId || it.ProductId] }))
        } catch {}
      }
      const hist = await adminService.getLatestOrderHistory(orderId)
      latestHistory.value = hist?.data || hist || latestHistory.value
    } catch {}
  }, ORDER_DETAIL_POLL_MS)
}
const stopDetailPolling = () => {
  if (detailTimer) { clearInterval(detailTimer); detailTimer = null }
}

// 订单状态更新后的处理函数 - 确保与用户端同步
const handleOrderStatusChange = async (orderId, newStatus, note) => {
  try {
    // 先禁用按钮避免重复点击
    loading.value = true
    
    // 调用更新订单状态的API
    const safeNote = (note || '').trim().slice(0, 480)
    await adminService.updateOrderStatus(orderId, newStatus, safeNote)
    
    // 立即刷新订单列表，确保用户端能看到最新状态
    await fetchOrders()
    
    // 显示成功消息
    ElMessage.success('订单状态更新成功')
    
    // 如果当前正在查看该订单详情，也更新详情数据
    if (selectedOrder.value && selectedOrder.value.id === orderId) {
      selectedOrder.value.status = newStatus
    }
  } catch (error) {
    console.error('更新订单状态失败:', error)
    const msg = (error?.data?.message) || (error?.response?.data?.message) || error?.message || '更新失败，请重试'
    ElMessage.error(`更新订单状态失败: ${msg}`)
  } finally {
    loading.value = false
  }
}

// 处理订单
const processOrder = async (orderId) => {
  if (actionLock.value) return
  try {
    actionLock.value = true
    await ElMessageBox.confirm('确定要处理该订单吗？', '确认操作', {
      confirmButtonText: '确定',
      cancelButtonText: '取消',
      type: 'info'
    })
    
    // 先置为处理中
    await handleOrderStatusChange(orderId, 'processing')

    // 处理后直接进入发货流程
    try {
      const { value: tracking } = await ElMessageBox.prompt('请输入快递单号（可选）', '发货信息', {
        confirmButtonText: '下一步',
        cancelButtonText: '取消',
        inputPlaceholder: '例如：SF1234567890'
      })
      const { value: carrier } = await ElMessageBox.prompt('请输入快递公司（可选）', '发货信息', {
        confirmButtonText: '发货',
        cancelButtonText: '取消',
        inputPlaceholder: '例如：顺丰/圆通/京东物流'
      })
      const note = [`快递公司：${carrier || '未填写'}`, `单号：${tracking || '未填写'}`].join('；')
      await handleOrderStatusChange(orderId, 'shipped', note)
    } catch (e) {
      // 用户取消发货信息填写则保留在处理中状态
      ElMessage.info('订单已标记为处理中，可稍后在“发货”按钮填写物流信息')
    }
  } catch (error) {
    // 用户取消操作
  }
  finally {
    actionLock.value = false
  }
}

const openProcessModal = (order) => {
  processTargetOrder.value = order
  processForm.value = { note: '', shipNow: false, carrier: '', tracking: '' }
  showProcessModal.value = true
}

const closeProcessModal = () => { showProcessModal.value = false }

const submitProcess = async () => {
  if (actionLock.value) return
  try {
    actionLock.value = true
    await handleOrderStatusChange(processTargetOrder.value.id, 'processing', (processForm.value.note || '').trim().slice(0, 480))
    if (processForm.value.shipNow) {
      // 自动补充默认快递公司与模拟单号
      const carrier = (processForm.value.carrier || '顺丰').trim()
      const tracking = (processForm.value.tracking || generateTrackingNo()).trim()
      const note = [`快递公司：${carrier}`, `单号：${tracking}`, (processForm.value.note || '').trim()].filter(Boolean).join('；').slice(0, 480)
      await handleOrderStatusChange(processTargetOrder.value.id, 'shipped', note)
    }
    showProcessModal.value = false
  } catch (e) {
    ElMessage.error('提交失败，请重试')
  } finally {
    actionLock.value = false
  }
}

// 生成模拟快递单号
const generateTrackingNo = () => {
  const prefix = 'SF'
  const n = Math.floor(100000000 + Math.random() * 900000000)
  return `${prefix}${n}`
}

// 发货订单
const shipOrder = async (orderId) => {
  if (actionLock.value) return
  try {
    actionLock.value = true
    const { value: tracking } = await ElMessageBox.prompt('请输入快递单号（可选）', '发货信息', {
      confirmButtonText: '下一步',
      cancelButtonText: '取消',
      inputPlaceholder: '例如：SF1234567890'
    })
    const { value: carrier } = await ElMessageBox.prompt('请输入快递公司（可选）', '发货信息', {
      confirmButtonText: '发货',
      cancelButtonText: '取消',
      inputPlaceholder: '例如：顺丰/圆通/京东物流'
    })
    const note = [`快递公司：${carrier || '未填写'}`, `单号：${tracking || '未填写'}`].join('；')
    await handleOrderStatusChange(orderId, 'shipped', note)
  } catch (error) {
    // 用户取消操作或报错
  }
  finally {
    actionLock.value = false
  }
}

// 拒绝发货（填写理由）
const rejectOrder = async (orderId) => {
  if (actionLock.value) return
  try {
    actionLock.value = true
    const { value } = await ElMessageBox.prompt('请填写拒绝发货的理由', '拒绝发货', {
      confirmButtonText: '提交',
      cancelButtonText: '取消',
      inputPlaceholder: '例如：库存不足/地址异常/风险订单等',
      inputValidator: (val) => {
        if (!val || !val.trim()) return '请填写理由'
        return true
      }
    })
    await adminService.updateOrderStatus(orderId, 'cancelled', value.trim().slice(0, 480))
    await fetchOrders()
    ElMessage.success('已拒绝发货并同步到用户订单')
  } catch (error) {
    // 用户取消或请求失败
    if (error) {
      console.error('拒绝发货失败:', error)
      ElMessage.error('提交失败，请重试')
    }
  }
  finally {
    actionLock.value = false
  }
}

const openRejectModal = (order) => {
  rejectTargetOrder.value = order
  rejectForm.value = { reason: '' }
  showRejectModal.value = true
}

const closeRejectModal = () => { showRejectModal.value = false }

const submitReject = async () => {
  if (actionLock.value) return
  try {
    actionLock.value = true
    const reason = (rejectForm.value.reason || '').trim()
    if (!reason) { ElMessage.warning('请填写拒绝发货的理由'); actionLock.value = false; return }
    await adminService.updateOrderStatus(rejectTargetOrder.value.id, 'cancelled', reason.slice(0, 480))
    await fetchOrders()
    ElMessage.success('已拒绝发货并同步到用户订单')
    showRejectModal.value = false
  } catch (e) {
    ElMessage.error('提交失败，请重试')
  } finally {
    actionLock.value = false
  }
}

// 删除订单弹窗状态
const showDeleteModal = ref(false)
const deleteTargetOrder = ref(null)

const openDeleteModal = (order) => {
  deleteTargetOrder.value = order
  showDeleteModal.value = true
}

const closeDeleteModal = () => { showDeleteModal.value = false }

const submitDelete = async () => {
  if (actionLock.value) return
  try {
    actionLock.value = true
    await adminService.deleteAdminOrder(deleteTargetOrder.value.id)
    // 即时从本地列表移除，提高响应速度
    const deletedId = deleteTargetOrder.value.id
    orders.value = orders.value.filter(o => o.id !== deletedId)
    updateStatistics()
    // 如果详情正在打开且是该订单，关闭详情
    if (showOrderDetail.value && selectedOrder.value && selectedOrder.value.id === deletedId) {
      showOrderDetail.value = false
      selectedOrder.value = null
      latestHistory.value = null
    }
    // 再执行一次刷新以确保与后端一致
    await fetchOrders()
    ElMessage.success('订单已删除或标记为取消')
    showDeleteModal.value = false
  } catch (error) {
    ElMessage.error('删除失败：' + (error.response?.data?.message || error.message))
  } finally {
    actionLock.value = false
  }
}

// 组件挂载时获取数据
onMounted(() => {
  fetchOrders()
  startPolling()
})

// 组件卸载时清除定时器
onUnmounted(() => {
  stopPolling()
  stopDetailPolling()
})

watch(() => showOrderDetail.value, (v) => {
  if (v && selectedOrder.value?.id) startDetailPolling(selectedOrder.value.id)
  else stopDetailPolling()
})

const getStatusClass = (status) => {
  switch (status) {
    case 'pending':
      return 'bg-yellow-100 text-yellow-800'
    case 'processing':
      return 'bg-blue-100 text-blue-800'
    case 'shipped':
      return 'bg-purple-100 text-purple-800'
    case 'delivered':
      return 'bg-green-100 text-green-800'
    case 'cancelled':
      return 'bg-red-100 text-red-800'
    default:
      return 'bg-gray-100 text-gray-800'
  }
}

const getStatusText = (status) => {
  switch (status) {
    case 'pending':
      return '待处理'
    case 'processing':
      return '处理中'
    case 'shipped':
      return '已发货'
    case 'delivered':
      return '已完成'
    case 'cancelled':
      return '已取消'
    default:
      return '未知状态'
  }
}

const formatDate = (dateString) => {
  if (!dateString) return '未知时间'
  try {
    return new Date(dateString).toLocaleString('zh-CN', {
      year: 'numeric', month: '2-digit', day: '2-digit',
      hour: '2-digit', minute: '2-digit',
      timeZone: 'Asia/Shanghai'
    })
  } catch {
    return new Date(dateString).toLocaleDateString('zh-CN', { timeZone: 'Asia/Shanghai' })
  }
}

const handleImageError = (event) => {
  event.target.src = '/images/product-placeholder.svg'
}

// 头像错误处理
const handleAvatarError = (event) => {
  event.target.src = '/images/default-avatar.png'
}
</script>

<style scoped>
.vip-badge {
  position: absolute;
  top: -6px;
  right: -6px;
  background: linear-gradient(135deg, #fef3c7, #fde68a);
  color: #92400e;
  font-size: 10px;
  font-weight: 700;
  padding: 2px 6px;
  border-radius: 9999px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.12);
}
</style>

const getItemStockText = (item) => {
  const s = item?.stock
  if (s === undefined || s === null || Number.isNaN(Number(s))) return '未知'
  return Number(s)
}
