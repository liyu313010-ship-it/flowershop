<template>
  <div class="container mx-auto px-4 py-8 max-w-4xl">
    <div class="flex items-center mb-6">
      <button @click="goBack" class="text-pink-600 hover:text-pink-800 mr-4">
        <i class="fas fa-arrow-left"></i>
      </button>
      <h1 class="text-2xl font-bold text-gray-800">支付验证</h1>
    </div>

    <div class="bg-white rounded-lg shadow-md p-6 mb-6">
      <div v-if="loading" class="text-center py-8">
        <div class="inline-block animate-spin rounded-full h-8 w-8 border-4 border-pink-200 border-t-pink-600 mb-4"></div>
        <p class="text-gray-600">正在加载订单信息...</p>
      </div>

      <div v-else-if="!order" class="text-center py-8">
        <i class="fas fa-exclamation-circle text-red-500 text-4xl mb-4"></i>
        <p class="text-gray-600 mb-4">未找到订单信息</p>
        <button @click="goHome" class="bg-pink-600 hover:bg-pink-700 text-white px-4 py-2 rounded transition-colors">
          返回首页
        </button>
      </div>

      <div v-else>
        <div class="mb-6">
          <h2 class="text-lg font-semibold text-gray-800 mb-4">订单信息</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div class="flex items-center">
              <span class="text-gray-500 w-24">订单编号：</span>
              <span class="font-medium">{{ order.orderNumber }}</span>
            </div>
            <div class="flex items-center">
              <span class="text-gray-500 w-24">订单金额：</span>
              <span class="font-medium text-red-600">¥{{ order.totalAmount.toFixed(2) }}</span>
            </div>
            <div class="flex items-center">
              <span class="text-gray-500 w-24">创建时间：</span>
              <span>{{ formatDate(order.createdAt) }}</span>
            </div>
            <div class="flex items-center">
              <span class="text-gray-500 w-24">支付状态：</span>
              <span :class="getPaymentStatusClass(order.paymentStatus)">
                {{ getPaymentStatusText(order.paymentStatus) }}
              </span>
            </div>
          </div>
        </div>

        <div v-if="order.paymentStatus !== 'paid'" class="mb-6">
          <h2 class="text-lg font-semibold text-gray-800 mb-4">验证支付</h2>
          <div class="bg-gray-50 p-4 rounded-lg mb-4">
            <p class="text-gray-600 mb-2">请输入支付凭证号进行支付验证</p>
            <div class="flex">
              <input
                v-model="paymentReference"
                type="text"
                class="flex-1 px-4 py-2 border border-gray-300 rounded-l-lg focus:outline-none focus:ring-2 focus:ring-pink-500"
                placeholder="请输入支付凭证号"
              />
              <button
                @click="verifyPayment"
                :disabled="verifying"
                class="bg-pink-600 hover:bg-pink-700 text-white px-6 py-2 rounded-r-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <span v-if="!verifying">验证支付</span>
                <span v-else class="flex items-center">
                  <i class="fas fa-spinner fa-spin mr-2"></i>验证中...
                </span>
              </button>
            </div>
          </div>
        </div>

        <div v-if="paymentStatus === 'verified'" class="bg-green-50 border border-green-200 rounded-lg p-4 mb-6">
          <div class="flex items-start">
            <i class="fas fa-check-circle text-green-500 text-2xl mt-1 mr-3"></i>
            <div>
              <h3 class="font-semibold text-green-800">支付验证成功</h3>
              <p class="text-green-700">您的订单已支付成功，我们将尽快为您安排发货。</p>
            </div>
          </div>
        </div>

        <div v-if="paymentStatus === 'failed'" class="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
          <div class="flex items-start">
            <i class="fas fa-times-circle text-red-500 text-2xl mt-1 mr-3"></i>
            <div>
              <h3 class="font-semibold text-red-800">支付验证失败</h3>
              <p class="text-red-700">{{ paymentError || '请检查支付凭证号是否正确' }}</p>
            </div>
          </div>
        </div>

        <div class="flex justify-end space-x-4">
          <button
            @click="goToOrderDetail"
            class="border border-gray-300 hover:border-gray-400 text-gray-700 px-4 py-2 rounded transition-colors"
          >
            查看订单详情
          </button>
          <button
            @click="goBack"
            class="bg-pink-600 hover:bg-pink-700 text-white px-4 py-2 rounded transition-colors"
          >
            返回首页
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import orderService from '@/services/orderService'

const route = useRoute()
const router = useRouter()

const loading = ref(true)
const verifying = ref(false)
const order = ref(null)
const paymentReference = ref('')
const paymentStatus = ref('')
const paymentError = ref('')

onMounted(async () => {
  await loadOrderInfo()
  
  // 检查URL参数，如果有out_trade_no或trade_no，说明是从支付回调回来的，自动验证
  const { out_trade_no, trade_no, verify } = route.query
  if (out_trade_no) {
    paymentReference.value = out_trade_no
    // 稍微延迟一下，确保后端状态同步
    setTimeout(() => verifyPayment(), 500)
  } else if (trade_no) {
    paymentReference.value = trade_no
    setTimeout(() => verifyPayment(), 500)
  } else if (verify === 'true') {
    // 如果只有verify=true参数，尝试使用订单的paymentReference进行验证
    if (order.value && order.value.paymentReference) {
      paymentReference.value = order.value.paymentReference
      setTimeout(() => verifyPayment(), 500)
    }
  }
})

const loadOrderInfo = async () => {
  try {
    loading.value = true
    // 先从params获取orderId，如果没有则从query获取
    const orderId = route.params.orderId || route.query.orderId
    
    if (!orderId) {
      throw new Error('订单ID缺失')
    }
    
    // 从后端获取订单信息
    const res = await orderService.getOrderById(orderId)
    if (res.success) {
      const data = res.data
      order.value = {
        id: data.id || data.Id,
        orderNumber: data.orderNumber || data.OrderNumber,
        totalAmount: data.totalAmount || data.TotalAmount,
        createdAt: data.createdAt || data.CreatedAt,
        paymentStatus: data.paymentStatus || data.PaymentStatus
      }
    } else {
       throw new Error('获取订单失败')
    }
  } catch (error) {
    console.error('加载订单信息失败:', error)
  } finally {
    loading.value = false
  }
}

const verifyPayment = async () => {
  try {
    verifying.value = true
    paymentStatus.value = ''
    paymentError.value = ''
    
    let ref = paymentReference.value.trim()
    if (!ref) {
      try {
        const statusRes = await orderService.getOrderPaymentStatus(order.value.id)
        ref = statusRes?.data?.paymentReference || order.value?.paymentReference || ''
      } catch {}
    }
    if (!ref) {
      throw new Error('缺少支付凭证号')
    }
    const res = await orderService.verifyPayment(order.value.id, ref)
    
    if (res.success) {
        paymentStatus.value = 'verified'
        order.value.paymentStatus = 'paid'
        router.push(`/orders/${order.value.id}`)
    } else {
        throw new Error(res.message || '验证失败')
    }
  } catch (error) {
    console.error('验证支付失败:', error)
    paymentStatus.value = 'failed'
    // 优先显示后端返回的具体错误信息（如果 axios 拦截器或 service 处理了）
    // 通常 axios 错误在 error.response.data 中
    if (error.response && error.response.data && error.response.data.message) {
        paymentError.value = error.response.data.message
    } else if (error.message) {
        paymentError.value = error.message
    } else {
        paymentError.value = '验证过程中出现错误，请联系客服'
    }
  } finally {
    verifying.value = false
  }
}

const getPaymentStatusClass = (status) => {
  switch (status) {
    case 'paid':
      return 'text-green-600 font-medium'
    case 'unpaid':
      return 'text-orange-600 font-medium'
    case 'refunded':
      return 'text-blue-600 font-medium'
    default:
      return 'text-gray-600'
  }
}

const getPaymentStatusText = (status) => {
  switch (status) {
    case 'paid':
      return '已支付'
    case 'unpaid':
      return '未支付'
    case 'refunded':
      return '已退款'
    default:
      return '未知状态'
  }
}

const formatDate = (dateString) => {
  if (!dateString) return ''
  try {
    return new Date(dateString).toLocaleString('zh-CN', {
      year: 'numeric', month: '2-digit', day: '2-digit',
      hour: '2-digit', minute: '2-digit',
      timeZone: 'Asia/Shanghai'
    })
  } catch { return '' }
}

const goToOrderDetail = () => {
  const orderId = order.value?.id || route.params.orderId
  if (orderId) {
    router.push(`/orders/${orderId}`)
  } else {
    router.push('/orders')
  }
}

const goHome = () => {
  router.push('/')
}

const goBack = () => {
  if (window.history.length > 1) {
    router.back()
  } else {
    router.push('/')
  }
}
</script>

<style scoped>
.container {
  min-height: calc(100vh - 100px);
}

button {
  cursor: pointer;
}

input:focus {
  box-shadow: 0 0 0 2px rgba(236, 72, 153, 0.25);
}
</style>
