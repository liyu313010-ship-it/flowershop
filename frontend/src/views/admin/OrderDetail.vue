<template>
  <div class="min-h-screen bg-gray-50">
    

    <div class="container mx-auto px-4 py-8">
      <!-- 页面头部 -->
      <div class="mb-8">
        <button @click="goBack" class="text-gray-600 hover:text-gray-900 mb-4 flex items-center">
          <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"></path>
          </svg>
          返回订单管理
        </button>
        <h1 class="text-2xl font-bold text-gray-900">订单详情 #{{ order?.orderNumber }}</h1>
      </div>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex justify-center items-center py-16">
        <div class="text-center">
          <div class="inline-block animate-spin rounded-full h-12 w-12 border-4 border-t-huanyu-pink-600 border-gray-200 mb-4"></div>
          <p>加载订单详情中...</p>
        </div>
      </div>

      <!-- 订单详情 -->
      <div v-else-if="order" class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- 订单状态和操作 -->
        <div class="lg:col-span-1 space-y-6">
          <!-- 订单状态卡片 -->
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-lg font-semibold mb-4">订单状态</h2>
            <div class="mb-4">
              <span :class="['status-badge', `status-${order?.status || 'pending'}`]">
                {{ getStatusText(order.status) }}
              </span>
            </div>
            <div class="space-y-2 text-sm text-gray-600">
              <div class="flex justify-between">
                <span>下单时间：</span>
                <span class="text-gray-900">{{ formatDate(order.createdAt) }}</span>
              </div>
              <div v-if="order.paymentTime" class="flex justify-between">
                <span>支付时间：</span>
                <span class="text-gray-900">{{ formatDate(order.paymentTime) }}</span>
              </div>
              <div v-if="order.shippingTime" class="flex justify-between">
                <span>发货时间：</span>
                <span class="text-gray-900">{{ formatDate(order.shippingTime) }}</span>
              </div>
              <div v-if="order.deliveryTime" class="flex justify-between">
                <span>送达时间：</span>
                <span class="text-gray-900">{{ formatDate(order.deliveryTime) }}</span>
              </div>
            </div>
          </div>

          <!-- 操作按钮卡片 -->
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-lg font-semibold mb-4">订单操作</h2>
            <div class="space-y-3">
              <button 
                v-if="canProcessOrder" 
                @click="processOrder" 
                class="w-full py-2 px-4 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                处理订单
              </button>
              <button 
                v-if="canShipOrder" 
                @click="openShippingModal" 
                class="w-full py-2 px-4 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors"
              >
                发货
              </button>
              <button 
                v-if="canCancelOrder" 
                @click="cancelOrder" 
                class="w-full py-2 px-4 bg-red-600 text-white rounded-lg hover:bg-red-700 transition-colors"
              >
                取消订单
              </button>
              <button 
                @click="updateOrderNotes" 
                class="w-full py-2 px-4 bg-gray-600 text-white rounded-lg hover:bg-gray-700 transition-colors"
              >
                添加备注
              </button>
            </div>
          </div>
        </div>

        <!-- 右侧：订单详细信息 -->
        <div class="lg:col-span-2 space-y-6">
          <!-- 用户信息卡片 -->
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-lg font-semibold mb-4">客户信息</h2>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <p class="text-sm text-gray-600 mb-1">客户姓名</p>
                <p class="font-medium">{{ order.recipientName || order.user?.name || '未知' }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-600 mb-1">联系电话</p>
                <p class="font-medium">{{ order.recipientPhone || order.user?.phone || '未知' }}</p>
              </div>
              <div class="md:col-span-2">
                <p class="text-sm text-gray-600 mb-1">收货地址</p>
                <p class="font-medium">{{ order.shippingAddress || order.deliveryAddress || '未知' }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-600 mb-1">支付方式</p>
                <p class="font-medium">{{ getPaymentMethodText(order.paymentMethod) }}</p>
              </div>
              <div>
                <p class="text-sm text-gray-600 mb-1">支付状态</p>
                <p class="font-medium">{{ getPaymentStatusText(order.paymentStatus) }}</p>
              </div>
            </div>
          </div>

          <!-- 商品清单卡片 -->
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-lg font-semibold mb-4">商品清单</h2>
            <div class="overflow-x-auto">
              <table class="min-w-full">
                <thead>
                  <tr class="border-b">
                    <th class="text-left py-3 px-4 text-sm font-medium text-gray-600">商品</th>
                    <th class="text-right py-3 px-4 text-sm font-medium text-gray-600">单价</th>
                    <th class="text-right py-3 px-4 text-sm font-medium text-gray-600">数量</th>
                    <th class="text-right py-3 px-4 text-sm font-medium text-gray-600">小计</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="item in orderItems" :key="item.id" class="border-b">
                    <td class="py-3 px-4">
                      <div class="flex items-center">
                        <img 
                          :src="getProductImageUrl(item.productImage)" 
                          :alt="item.productName" 
                          class="w-12 h-12 object-cover rounded mr-3"
                        >
                        <span class="text-sm">{{ item.productName }}</span>
                      </div>
                    </td>
                    <td class="py-3 px-4 text-right text-sm">¥{{ item.unitPrice }}</td>
                    <td class="py-3 px-4 text-right text-sm">{{ item.quantity }}</td>
                    <td class="py-3 px-4 text-right text-sm font-medium">¥{{ (item.unitPrice * item.quantity).toFixed(2) }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- 订单金额汇总卡片 -->
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-lg font-semibold mb-4">金额汇总</h2>
            <div class="space-y-2">
              <div class="flex justify-between text-sm">
                <span class="text-gray-600">商品总价</span>
                <span>¥{{ subTotal.toFixed(2) }}</span>
              </div>
              <div class="flex justify-between text-sm">
                <span class="text-gray-600">运费</span>
                <span>¥{{ order.shippingFee || 0 }}</span>
              </div>
              <div v-if="order.discountAmount" class="flex justify-between text-sm">
                <span class="text-gray-600">优惠金额</span>
                <span class="text-green-600">-¥{{ order.discountAmount.toFixed(2) }}</span>
              </div>
              <div class="border-t pt-2 flex justify-between text-lg font-bold mt-3">
                <span>订单总价</span>
                <span class="text-huanyu-pink-600">¥{{ totalAmount.toFixed(2) }}</span>
              </div>
            </div>
          </div>

          <!-- 订单备注卡片 -->
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-lg font-semibold mb-4">订单备注</h2>
            <div v-if="order.notes" class="p-3 bg-gray-50 rounded text-sm">
              {{ order.notes }}
            </div>
            <div v-else class="p-3 bg-gray-50 rounded text-sm text-gray-500">
              暂无备注
            </div>
          </div>

          <!-- 订单状态历史卡片 -->
      <div class="bg-white rounded-lg shadow p-6">
        <h2 class="text-lg font-semibold mb-4">订单状态历史</h2>
        <div class="space-y-4">
          <div v-if="loadingLogs" class="flex justify-center items-center py-8">
            <div class="inline-block animate-spin rounded-full h-8 w-8 border-4 border-t-huanyu-pink-600 border-gray-200 mb-4"></div>
          </div>
          <template v-else>
            <div class="timeline-item" v-for="(log, index) in orderLogs" :key="index">
              <div class="flex">
                <div class="flex-shrink-0 mt-1">
                  <div class="w-3 h-3 rounded-full bg-gray-400"></div>
                </div>
                <div class="ml-3">
                  <p class="text-sm font-medium">{{ log.statusDescription || log.description }}</p>
                  <p v-if="log.notes" class="text-sm text-gray-600 italic">{{ log.notes }}</p>
                  <p class="text-xs text-gray-500">{{ formatDate(log.timestamp) }}</p>
                </div>
              </div>
            </div>
            <div v-if="orderLogs.length === 0" class="text-sm text-gray-500">
              暂无状态记录
            </div>
          </template>
        </div>
      </div>
        </div>
      </div>

      <!-- 错误状态 -->
      <div v-else class="text-center py-16">
        <p class="text-red-600 mb-4">订单不存在或已被删除</p>
        <button @click="goBack" class="px-4 py-2 bg-gray-600 text-white rounded-lg hover:bg-gray-700">
          返回订单管理
        </button>
      </div>
    </div>

    <!-- 发货模态框 -->
    <div v-if="showShippingModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
        <h3 class="text-xl font-bold mb-4">订单发货</h3>
        <div class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">物流公司</label>
            <select v-model="shippingInfo.company" class="w-full px-3 py-2 border border-gray-300 rounded-lg">
              <option value="SF">顺丰速运</option>
              <option value="YT">圆通速递</option>
              <option value="YD">韵达快递</option>
              <option value="ZT">中通快递</option>
              <option value="EMS">EMS</option>
              <option value="JD">京东物流</option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">运单号</label>
            <input 
              v-model="shippingInfo.trackingNumber" 
              type="text" 
              class="w-full px-3 py-2 border border-gray-300 rounded-lg"
              placeholder="请输入运单号"
            >
          </div>
        </div>
        <div class="mt-6 flex justify-end space-x-3">
          <button @click="showShippingModal = false" class="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50">
            取消
          </button>
          <button @click="confirmShipOrder" class="px-4 py-2 bg-green-600 text-white rounded-lg hover:bg-green-700">
            确认发货
          </button>
        </div>
      </div>
    </div>

    <!-- 备注模态框 -->
    <div v-if="showNotesModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-6 max-w-md w-full mx-4">
        <h3 class="text-xl font-bold mb-4">添加订单备注</h3>
        <textarea 
          v-model="orderNotes" 
          class="w-full px-3 py-2 border border-gray-300 rounded-lg h-32 resize-none"
          placeholder="请输入备注信息..."
        ></textarea>
        <div class="mt-6 flex justify-end space-x-3">
          <button @click="showNotesModal = false" class="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50">
            取消
          </button>
          <button @click="confirmUpdateNotes" class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
            保存备注
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import AdminNav from '@/components/admin/AdminNav.vue'
import adminService from '@/services/adminService'
import { getProductImageUrl } from '@/utils/avatar.js'

export default {
  name: 'AdminOrderDetail',
  components: {
    AdminNav
  },
  setup() {
    const route = useRoute()
    const router = useRouter()
    const orderId = route.params.id
    
    const loading = ref(true)
    const order = ref(null)
    const showShippingModal = ref(false)
    const showNotesModal = ref(false)
    const orderNotes = ref('')
    const shippingInfo = ref({
      company: 'SF',
      trackingNumber: ''
    })

    // 获取订单详情
    const fetchOrderDetail = async () => {
      try {
        loading.value = true
        const response = await adminService.getAdminOrderById(orderId)
        order.value = response.data || response
        // 初始化备注框
        orderNotes.value = order.value.notes || ''
      } catch (error) {
        console.error('获取订单详情失败:', error)
        ElMessage.error('获取订单详情失败，请稍后重试')
      } finally {
        loading.value = false
      }
    }

    // 计算属性
    const orderItems = computed(() => {
      return order.value?.orderItems || order.value?.items || []
    })

    const subTotal = computed(() => {
      return orderItems.value.reduce((sum, item) => {
        return sum + (item.unitPrice * item.quantity)
      }, 0)
    })

    const totalAmount = computed(() => {
      let total = subTotal.value + (order.value?.shippingFee || 0)
      if (order.value?.discountAmount) {
        total -= order.value.discountAmount
      }
      return total
    })

    // 订单状态文本
    const getStatusText = (status) => {
      const statusMap = {
        pending: '待处理',
        processing: '处理中',
        shipped: '已发货',
        delivered: '已送达',
        cancelled: '已取消'
      }
      return statusMap[status] || '未知状态'
    }

    // 支付方式文本
    const getPaymentMethodText = (method) => {
      const methodMap = {
        online: '在线支付',
        cod: '货到付款',
        wechat: '微信支付',
        alipay: '支付宝'
      }
      return methodMap[method] || '其他支付'
    }

    // 支付状态文本
    const getPaymentStatusText = (status) => {
      const statusMap = {
        unpaid: '未支付',
        paid: '已支付',
        refunded: '已退款'
      }
      return statusMap[status] || '未知状态'
    }

    // 格式化日期
    const formatDate = (dateString) => {
      if (!dateString) return '--'
      try {
        return new Date(dateString).toLocaleString('zh-CN', {
          year: 'numeric', month: '2-digit', day: '2-digit',
          hour: '2-digit', minute: '2-digit', second: '2-digit',
          timeZone: 'Asia/Shanghai'
        })
      } catch { return '--' }
    }

    // 判断是否可以处理订单
    const canProcessOrder = computed(() => {
      return order.value?.status === 'pending' && order.value?.paymentStatus === 'paid'
    })

    // 判断是否可以发货
    const canShipOrder = computed(() => {
      return order.value?.status === 'processing' && order.value?.paymentStatus === 'paid'
    })

    // 判断是否可以取消订单
    const canCancelOrder = computed(() => {
      return ['pending', 'processing'].includes(order.value?.status) && 
             (order.value?.paymentStatus === 'unpaid' || 
              (order.value?.paymentStatus === 'paid' && order.value?.canRefund))
    })

    // 订单状态历史日志
    const orderLogs = ref([])
    const loadingLogs = ref(false)
    
    // 获取订单状态历史
    const fetchOrderStatusHistory = async () => {
      if (!orderId) return
      
      loadingLogs.value = true
      try {
        const response = await adminService.getOrderStatusHistory(orderId)
        orderLogs.value = response || []
      } catch (error) {
        console.error('获取订单状态历史失败:', error)
        // 如果API调用失败，使用原始逻辑作为后备
        const logs = []
        if (order.value) {
          // 添加订单创建日志
          if (order.value.createdAt) {
            logs.push({
              statusDescription: '订单创建',
              timestamp: order.value.createdAt
            })
          }

          // 添加支付日志
          if (order.value.paymentTime) {
            logs.push({
              statusDescription: '订单支付成功',
              timestamp: order.value.paymentTime
            })
          }

          // 添加处理日志
          if (order.value.processingTime) {
            logs.push({
              statusDescription: '订单开始处理',
              timestamp: order.value.processingTime
            })
          }

          // 添加发货日志
          if (order.value.shippingTime) {
            logs.push({
              statusDescription: '订单已发货',
              timestamp: order.value.shippingTime
            })
          }

          // 添加送达日志
          if (order.value.deliveryTime) {
            logs.push({
              statusDescription: '订单已送达',
              timestamp: order.value.deliveryTime
            })
          }

          // 添加取消日志
          if (order.value.cancelledAt) {
            logs.push({
              statusDescription: '订单已取消',
              timestamp: order.value.cancelledAt
            })
          }
          
          orderLogs.value = logs.sort((a, b) => new Date(a.timestamp) - new Date(b.timestamp))
        }
      } finally {
        loadingLogs.value = false
      }
    }

    // 返回订单管理页面
    const goBack = () => {
      router.push('/admin/orders')
    }

    // 处理订单
    const processOrder = async () => {
      try {
        await ElMessageBox.confirm('确认开始处理此订单？', '提示', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'warning'
        })
        
        const result = await adminService.updateOrderStatus(orderId, 'processing')
        // 验证是否真的更新成功
        if (result.success || result.status === 200) {
          ElMessage.success('订单处理成功，状态已更新为处理中')
          // 立即重新加载订单信息，确保状态变更同步
          await fetchOrderDetail()
          // 重新获取订单状态历史
          await fetchOrderStatusHistory()
          // 重新计算状态相关的计算属性
          canProcessOrder.value = false
          canShipOrder.value = true
        } else {
          ElMessage.error('订单状态更新失败，请检查服务器连接')
        }
      } catch (error) {
        if (error !== 'cancel') {
          console.error('处理订单失败:', error)
          ElMessage.error('处理订单失败，请稍后重试')
        }
      }
    }

    // 打开发货模态框
    const openShippingModal = () => {
      showShippingModal.value = true
    }

    // 确认发货
    const confirmShipOrder = async () => {
      if (!shippingInfo.value.trackingNumber) {
        ElMessage.warning('请输入运单号')
        return
      }

      try {
        const response = await adminService.shipOrder(orderId, shippingInfo.value)
        // 验证发货是否成功
        if (response.success || response.status === 200) {
          ElMessage.success('发货成功，订单状态已更新为已发货')
          showShippingModal.value = false
          // 立即重新加载订单信息，确保状态变更同步到数据库
          await fetchOrderDetail()
          // 重新获取订单状态历史
          await fetchOrderStatusHistory()
          // 重新计算状态相关的计算属性
          canShipOrder.value = false
        } else {
          ElMessage.error('发货失败，请检查服务器连接')
        }
      } catch (error) {
        console.error('发货失败:', error)
        ElMessage.error('发货失败，请稍后重试')
      }
    }

    // 取消订单
    const cancelOrder = async () => {
      try {
        await ElMessageBox.confirm('确认取消此订单？', '警告', {
          confirmButtonText: '确定',
          cancelButtonText: '取消',
          type: 'error'
        })
        
        await adminService.updateOrderStatus(orderId, 'cancelled')
        ElMessage.success('订单已取消')
        await fetchOrderDetail() // 重新加载订单信息
        await fetchOrderStatusHistory() // 重新获取订单状态历史
      } catch (error) {
        if (error !== 'cancel') {
          console.error('取消订单失败:', error)
          ElMessage.error('取消订单失败，请稍后重试')
        }
      }
    }

    // 更新订单备注
    const updateOrderNotes = () => {
      showNotesModal.value = true
    }

    // 确认更新备注
    const confirmUpdateNotes = async () => {
      try {
        await adminService.updateOrderNotes(orderId, orderNotes.value)
        ElMessage.success('备注更新成功')
        showNotesModal.value = false
        fetchOrderDetail() // 重新加载订单信息
      } catch (error) {
        console.error('更新备注失败:', error)
        ElMessage.error('更新备注失败，请稍后重试')
      }
    }

    // 组件挂载时获取订单详情和状态历史
    onMounted(async () => {
      await fetchOrderDetail()
      await fetchOrderStatusHistory()
    })

    return {
      loading,
      order,
      orderItems,
      subTotal,
      totalAmount,
      showShippingModal,
      showNotesModal,
      orderNotes,
      shippingInfo,
      orderLogs,
      canProcessOrder,
      canShipOrder,
      canCancelOrder,
      getStatusText,
      getPaymentMethodText,
      getPaymentStatusText,
      formatDate,
      goBack,
      processOrder,
      openShippingModal,
      confirmShipOrder,
      cancelOrder,
      updateOrderNotes,
      confirmUpdateNotes,
      getProductImageUrl
    }
  }
}
</script>

<style scoped>
.status-badge {
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.875rem;
  font-weight: 500;
}

.status-pending {
  background-color: #fff3cd;
  color: #856404;
}

.status-processing {
  background-color: #d1ecf1;
  color: #0c5460;
}

.status-shipped {
  background-color: #cce5ff;
  color: #004085;
}

.status-delivered {
  background-color: #d4edda;
  color: #155724;
}

.status-cancelled {
  background-color: #f8d7da;
  color: #721c24;
}

.timeline-item:not(:last-child)::after {
  content: '';
  position: absolute;
  left: 6px;
  top: 16px;
  bottom: 0;
  width: 2px;
  background-color: #e5e7eb;
}

.timeline-item {
  position: relative;
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .timeline-item:not(:last-child)::after {
    left: 5px;
  }
}

@media (max-width: 640px) {
  .container {
    padding: 0 1rem;
  }
  
  h1 {
    font-size: 1.5rem;
  }
  
  .timeline-item:not(:last-child)::after {
    left: 5px;
  }
}
</style>
