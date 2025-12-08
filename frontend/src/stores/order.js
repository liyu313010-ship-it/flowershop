import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { orderService } from '@/services/order'

// 订单状态管理store
export const useOrderStore = defineStore('order', () => {
  // 状态定义
  const orders = ref([])
  const currentOrder = ref(null)
  const orderStatuses = ref([])
  const shippingFee = ref(0)
  const availableCoupons = ref([])
  const isLoading = ref(false)
  const error = ref('')
  const pagination = ref({
    page: 1,
    limit: 10,
    total: 0,
    totalPages: 0
  })

  // 计算属性
  const hasOrders = computed(() => orders.value.length > 0)
  const pendingOrders = computed(() => 
    orders.value.filter(order => order.status === 'pending')
  )
  const processingOrders = computed(() => 
    orders.value.filter(order => order.status === 'processing')
  )
  const completedOrders = computed(() => 
    orders.value.filter(order => order.status === 'completed')
  )
  const cancelledOrders = computed(() => 
    orders.value.filter(order => order.status === 'cancelled')
  )

  // 获取用户订单列表
  const fetchOrders = async (params = {}) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await orderService.getUserOrders(params)
      orders.value = response.orders || []
      pagination.value = response.pagination || pagination.value
      return response
    } catch (err) {
      console.error('Fetch orders error:', err)
      error.value = err.response?.data?.message || '获取订单列表失败'
      orders.value = []
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 获取订单详情
  const fetchOrderDetail = async (orderId) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await orderService.getOrderById(orderId)
      currentOrder.value = response
      return response
    } catch (err) {
      console.error('Fetch order detail error:', err)
      error.value = err.response?.data?.message || '获取订单详情失败'
      currentOrder.value = null
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 创建订单
  const createOrder = async (orderData) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await orderService.createOrder(orderData)
      
      // 将新订单添加到列表开头
      if (response.order) {
        orders.value.unshift(response.order)
      }
      
      return response
    } catch (err) {
      console.error('Create order error:', err)
      error.value = err.response?.data?.message || '创建订单失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 取消订单
  const cancelOrder = async (orderId, reason = '') => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await orderService.cancelOrder(orderId, reason)
      
      // 更新本地订单状态
      const orderIndex = orders.value.findIndex(order => order.id === orderId)
      if (orderIndex !== -1) {
        orders.value[orderIndex] = { ...orders.value[orderIndex], ...response.order }
      }
      
      // 如果是当前订单，也更新
      if (currentOrder.value && currentOrder.value.id === orderId) {
        currentOrder.value = { ...currentOrder.value, ...response.order }
      }
      
      return response
    } catch (err) {
      console.error('Cancel order error:', err)
      error.value = err.response?.data?.message || '取消订单失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 确认收货
  const confirmOrder = async (orderId) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await orderService.confirmOrder(orderId)
      
      // 更新本地订单状态
      const orderIndex = orders.value.findIndex(order => order.id === orderId)
      if (orderIndex !== -1) {
        orders.value[orderIndex] = { ...orders.value[orderIndex], ...response.order }
      }
      
      // 如果是当前订单，也更新
      if (currentOrder.value && currentOrder.value.id === orderId) {
        currentOrder.value = { ...currentOrder.value, ...response.order }
      }
      
      return response
    } catch (err) {
      console.error('Confirm order error:', err)
      error.value = err.response?.data?.message || '确认收货失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 申请退款
  const requestRefund = async (orderId, refundData) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await orderService.requestRefund(orderId, refundData)
      
      // 更新本地订单状态
      const orderIndex = orders.value.findIndex(order => order.id === orderId)
      if (orderIndex !== -1) {
        orders.value[orderIndex] = { ...orders.value[orderIndex], ...response.order }
      }
      
      // 如果是当前订单，也更新
      if (currentOrder.value && currentOrder.value.id === orderId) {
        currentOrder.value = { ...currentOrder.value, ...response.order }
      }
      
      return response
    } catch (err) {
      console.error('Request refund error:', err)
      error.value = err.response?.data?.message || '申请退款失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 重新下单
  const reorder = async (orderId) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await orderService.reorder(orderId)
      return response
    } catch (err) {
      console.error('Reorder error:', err)
      error.value = err.response?.data?.message || '重新下单失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 获取订单状态列表
  const fetchOrderStatuses = async () => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await orderService.getOrderStatuses()
      orderStatuses.value = response.statuses || []
      return response
    } catch (err) {
      console.error('Fetch order statuses error:', err)
      error.value = err.response?.data?.message || '获取订单状态失败'
      orderStatuses.value = []
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 计算运费
  const calculateShippingFee = async (addressData) => {
    try {
      const response = await orderService.calculateShippingFee(addressData)
      shippingFee.value = response.fee || 0
      return response
    } catch (err) {
      console.error('Calculate shipping fee error:', err)
      error.value = err.response?.data?.message || '计算运费失败'
      shippingFee.value = 0
      throw err
    }
  }

  // 获取可用优惠券
  const fetchAvailableCoupons = async (orderAmount) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await orderService.getAvailableCoupons(orderAmount)
      availableCoupons.value = response.coupons || []
      return response
    } catch (err) {
      console.error('Fetch available coupons error:', err)
      error.value = err.response?.data?.message || '获取优惠券失败'
      availableCoupons.value = []
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 使用优惠券
  const applyCoupon = async (couponCode, orderAmount) => {
    try {
      const response = await orderService.useCoupon(couponCode, orderAmount)
      return response
    } catch (err) {
      console.error('Apply coupon error:', err)
      error.value = err.response?.data?.message || '使用优惠券失败'
      throw err
    }
  }

  // 获取订单统计信息
  const getOrderStats = () => {
    const totalOrders = orders.value.length
    const totalAmount = orders.value.reduce((sum, order) => sum + (order.totalAmount || 0), 0)
    const pendingCount = pendingOrders.value.length
    const completedCount = completedOrders.value.length

    return {
      totalOrders,
      totalAmount,
      pendingCount,
      completedCount
    }
  }

  // 重置状态
  const resetState = () => {
    orders.value = []
    currentOrder.value = null
    orderStatuses.value = []
    shippingFee.value = 0
    availableCoupons.value = []
    isLoading.value = false
    error.value = ''
    pagination.value = {
      page: 1,
      limit: 10,
      total: 0,
      totalPages: 0
    }
  }

  return {
    // 状态
    orders,
    currentOrder,
    orderStatuses,
    shippingFee,
    availableCoupons,
    isLoading,
    error,
    pagination,

    // 计算属性
    hasOrders,
    pendingOrders,
    processingOrders,
    completedOrders,
    cancelledOrders,

    // 方法
    fetchOrders,
    fetchOrderDetail,
    createOrder,
    cancelOrder,
    confirmOrder,
    requestRefund,
    reorder,
    fetchOrderStatuses,
    calculateShippingFee,
    fetchAvailableCoupons,
    applyCoupon,
    getOrderStats,
    resetState
  }
})