<template>
  <div class="order-detail-page">
    <div class="page-header">
      <button @click="goBack" class="back-btn">
        ← 返回订单列表
      </button>
      <h1>订单详情</h1>
    </div>

    <div v-if="loading" class="loading">
      <div class="loading-spinner"></div>
      <p>加载订单详情中...</p>
    </div>

    <div v-else-if="order" class="order-detail">
      <!-- 订单状态卡片 -->
      <div class="status-card">
        <div class="status-info">
          <h2>订单状态</h2>
          <div class="status-display">
            <span 
              :class="['status-badge', getStatusClass(order.status), (order.status === 'shipped' && order.shippingInfo) ? 'clickable' : '']"
            >
              {{ getStatusText(order.status) }}
            </span>
            <span class="order-number">{{ order.orderNumber }}</span>
          </div>
          <!-- 显示物流信息 -->
          <div v-if="order.status === 'shipped' && order.shippingInfo" class="shipping-info mt-3">
            <div class="text-sm">
              <span class="text-gray-600">物流公司：</span>
              <span>{{ getShippingCompanyName(order.shippingInfo.company) }}</span>
            </div>
            <div class="text-sm">
              <span class="text-gray-600">运单号：</span>
              <span>{{ order.shippingInfo.trackingNumber }}</span>
            </div>
            <div v-if="latestShippingNote" class="text-sm mt-2 text-gray-700">
              最新处理备注：{{ latestShippingNote }}
            </div>
          </div>
        </div>
        <div class="status-actions">
          <button 
            v-if="order.status === 'pending'" 
            @click="cancelOrder" 
            class="btn btn-danger"
          >
            取消订单
          </button>
          <button 
            v-if="order.status === 'pending' && order.paymentStatus === 'unpaid'" 
            @click="payOrder" 
            class="btn btn-primary"
          >
            立即付款
          </button>
          <button 
            v-if="(order.status === 'processing' || order.status === 'shipped') && isAdmin" 
            @click="openShippingModal" 
            class="btn btn-secondary"
          >
            更新物流
          </button>
        </div>
      </div>

      <!-- 收货信息 -->
      <div class="info-card">
        <h3>收货信息</h3>
        <div v-if="isLoadingAddresses" class="info-content loading-address">
          <div class="info-loading">
            <div class="loading-spinner"></div>
            <span>正在加载最新收货地址信息...</span>
          </div>
        </div>
        <div v-else class="info-content">
          <div class="info-item">
            <label>收货人：</label>
            <span>{{ currentAddress?.name || order.recipientName || '未知收货人' }}</span>
          </div>
          <div class="info-item">
            <label>联系电话：</label>
            <span>{{ currentAddress?.phone || order.recipientPhone || '未知电话' }}</span>
          </div>
          <div class="info-item">
            <label>收货地址：</label>
            <span>{{ currentAddress?.fullAddress || order.shippingAddress || order.deliveryAddress || '未知地址' }}</span>
          </div>
          <div class="info-item">
            <label>支付方式：</label>
            <span>{{ getPaymentMethodText(order.paymentMethod) }}</span>
          </div>
          <div class="info-item">
            <label>支付状态：</label>
            <span :class="['payment-status', order.paymentStatus]">
              {{ getPaymentStatusText(order.paymentStatus) }}
            </span>
          </div>
        </div>
      </div>

      <!-- 商品列表 -->
      <div class="items-card">
        <h3>商品清单</h3>
        <div class="items-list">
          <div v-for="item in order.orderItems" :key="item.id" class="order-item">
            <img :src="item.productImage || '/placeholder-flower.jpg'" :alt="item.productName" class="item-image">
            <div class="item-details">
              <h4>{{ item.productName }}</h4>
              <p class="item-price">单价：¥{{ item.unitPrice }}</p>
            </div>
            <div class="item-quantity">
              × {{ item.quantity }}
            </div>
            <div class="item-total">
              ¥{{ item.totalPrice }}
            </div>
            <div class="item-actions">
              <button 
                v-if="order.status === 'delivered' && !item.reviewed"
                @click="openReviewModal(item)"
                class="review-btn"
              >
                评价
              </button>
              <button 
                v-else-if="item.reviewed"
                @click="openReviewDetailByProduct(item)"
                class="review-btn"
              >
                查看评价
              </button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="userReviews.length > 0" class="items-card">
        <h3>我的评价</h3>
        <div class="items-list">
          <div v-for="rv in userReviews" :key="rv.id" class="order-item">
            <img :src="rv.avatar || '/images/user-avatar-placeholder.svg'" alt="avatar" class="item-image">
            <div class="item-details">
              <h4>@{{ rv.productName }}</h4>
              <div class="item-price">
                <span>
                  <span v-for="star in 5" :key="star" class="star" :class="{ 'filled': rv.rating >= star }">★</span>
                </span>
              </div>
              <p class="item-quantity">{{ rv.comment }}</p>
            </div>
            <div class="item-actions">
              <button class="review-btn" @click="openReviewDetail(rv)">查看详情</button>
              <button class="review-btn" @click="deleteUserReview(rv)">删除评价</button>
            </div>
          </div>
        </div>
      </div>

      <!-- 订单汇总 -->
      <div class="summary-card">
        <div class="summary-item">
          <span>商品总额：</span>
          <span>¥{{ order.totalAmount }}</span>
        </div>
        <div class="summary-item">
          <span>运费：</span>
          <span>¥0</span>
        </div>
        <div class="summary-total">
          <span>订单总额：</span>
          <span class="total-amount">¥{{ order.totalAmount }}</span>
        </div>
      </div>

      <!-- 订单时间信息和状态变更历史 -->
      <div class="timeline-card">
        <h3>订单状态历史</h3>
        <div class="timeline">
          <!-- 使用API获取的状态历史记录 -->
          <template v-if="statusHistory.length > 0">
            <div 
              v-for="(historyItem, index) in statusHistory" 
              :key="index" 
              class="timeline-item"
            >
              <div :class="['timeline-dot', historyItem.status]"></div>
              <div class="timeline-content">
                <p class="timeline-title">{{ getHistoryStatusText(historyItem.status) || historyItem.description }}</p>
                <p class="timeline-time">{{ formatDate(historyItem.timestamp) }}</p>
                <!-- 如果有物流信息，显示在相关状态中 -->
                <p v-if="historyItem.status === 'shipped' && order.shippingInfo" class="timeline-shipping">
                  {{ getShippingCompanyName(order.shippingInfo.company) }} - {{ order.shippingInfo.trackingNumber }}
                </p>
                <!-- 显示额外备注信息（如果有） -->
                <p v-if="historyItem.note" class="timeline-note">{{ historyItem.note }}</p>
              </div>
            </div>
          </template>
          <!-- 降级为原来的显示方式 -->
          <template v-else>
            <!-- 订单创建 -->
            <div class="timeline-item">
              <div class="timeline-dot created"></div>
              <div class="timeline-content">
                <p class="timeline-title">订单创建</p>
                <p class="timeline-time">{{ formatDate(order.createdAt) }}</p>
              </div>
            </div>
            
            <!-- 支付完成 -->
            <div v-if="order.paymentTime" class="timeline-item">
              <div class="timeline-dot paid"></div>
              <div class="timeline-content">
                <p class="timeline-title">支付完成</p>
                <p class="timeline-time">{{ formatDate(order.paymentTime) }}</p>
              </div>
            </div>
            
            <!-- 处理中 -->
            <div v-if="order.processingTime" class="timeline-item">
              <div class="timeline-dot processing"></div>
              <div class="timeline-content">
                <p class="timeline-title">订单处理中</p>
                <p class="timeline-time">{{ formatDate(order.processingTime) }}</p>
              </div>
            </div>
            
            <!-- 已发货 -->
            <div v-if="order.shippingTime" class="timeline-item">
              <div class="timeline-dot shipped"></div>
              <div class="timeline-content">
                <p class="timeline-title">订单已发货</p>
                <p class="timeline-time">{{ formatDate(order.shippingTime) }}</p>
                <p v-if="order.shippingInfo" class="timeline-shipping">{{ getShippingCompanyName(order.shippingInfo.company) }} - {{ order.shippingInfo.trackingNumber }}</p>
              </div>
            </div>
            
            <!-- 已送达 -->
            <div v-if="order.deliveryTime" class="timeline-item">
              <div class="timeline-dot delivered"></div>
              <div class="timeline-content">
                <p class="timeline-title">订单已送达</p>
                <p class="timeline-time">{{ formatDate(order.deliveryTime) }}</p>
              </div>
            </div>
            
            <!-- 已取消 -->
            <div v-if="order.cancelledAt" class="timeline-item">
              <div class="timeline-dot cancelled"></div>
              <div class="timeline-content">
                <p class="timeline-title">订单已取消</p>
                <p class="timeline-time">{{ formatDate(order.cancelledAt) }}</p>
              </div>
            </div>
          </template>
        </div>
      </div>
      
      <!-- 物流信息编辑模态框 -->
      <div v-if="showShippingModal" class="modal-overlay" @click.self="showShippingModal = false">
        <div class="modal">
          <div class="modal-header">
            <h3>更新物流信息</h3>
            <button class="close-btn" @click="showShippingModal = false">×</button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label for="shipping-company">物流公司</label>
              <select 
                id="shipping-company" 
                v-model="shippingInfoForm.company"
                class="form-control"
              >
                <option value="SF">顺丰速运</option>
                <option value="YT">圆通速递</option>
                <option value="YD">韵达快递</option>
                <option value="ZT">中通快递</option>
                <option value="EMS">EMS</option>
                <option value="JD">京东物流</option>
              </select>
            </div>
            <div class="form-group">
              <label for="tracking-number">运单号</label>
              <input 
                type="text" 
                id="tracking-number" 
                v-model="shippingInfoForm.trackingNumber"
                class="form-control"
                placeholder="请输入运单号"
              >
            </div>
          </div>
          <div class="modal-footer">
            <button @click="showShippingModal = false" class="btn btn-secondary">取消</button>
            <button @click="updateShipping" class="btn btn-primary">保存</button>
          </div>
        </div>
      </div>

      <!-- 评价模态框 -->
      <div v-if="showReviewModal" class="modal-overlay" @click.self="showReviewModal = false">
        <div class="modal review-modal">
          <div class="modal-header">
            <h3>评价商品</h3>
            <button class="close-btn" @click="showReviewModal = false">×</button>
          </div>
          <div class="modal-body">
            <div class="review-item-info">
              <img :src="currentReviewItem?.productImage || '/placeholder-flower.jpg'" :alt="currentReviewItem?.productName" class="review-item-image">
              <h4>{{ currentReviewItem?.productName }}</h4>
            </div>
            <div class="form-group">
              <label>评分</label>
              <div class="rating-stars">
                <span 
                  v-for="star in 5" 
                  :key="star" 
                  class="star"
                  :class="{ 'filled': reviewForm.rating >= star }"
                  @click="reviewForm.rating = star"
                >
                  ★
                </span>
              </div>
              <span class="rating-text">{{ reviewForm.rating }}星</span>
            </div>
            <div class="form-group">
              <label for="review-comment">评价内容</label>
              <textarea 
                id="review-comment" 
                v-model="reviewForm.comment"
                class="form-control"
                placeholder="请写下您对这件商品的评价..."
                rows="4"
              ></textarea>
            </div>
          </div>
          <div class="modal-footer">
            <button @click="showReviewModal = false" class="btn btn-secondary">取消</button>
            <button @click="submitReview" class="btn btn-primary" :disabled="submittingReview">
              {{ submittingReview ? '提交中...' : '提交评价' }}
            </button>
          </div>
        </div>
      </div>

      <div v-if="showReviewDetailModal" class="modal-overlay" @click.self="showReviewDetailModal = false">
        <div class="modal review-modal">
          <div class="modal-header">
            <h3>评价详情</h3>
            <button class="close-btn" @click="showReviewDetailModal = false">×</button>
          </div>
          <div class="modal-body">
            <div class="review-item-info">
              <img :src="currentUserReview?.avatar || '/images/user-avatar-placeholder.svg'" alt="avatar" class="review-item-image">
              <h4>@{{ currentUserReview?.productName }}</h4>
            </div>
            <div class="form-group">
              <div class="rating-stars">
                <span v-for="star in 5" :key="star" class="star" :class="{ 'filled': (currentUserReview?.rating || 0) >= star }">★</span>
              </div>
            </div>
            <div class="form-group">
              <div class="form-control">{{ currentUserReview?.comment }}</div>
            </div>
          </div>
          <div class="modal-footer">
            <button @click="showReviewDetailModal = false" class="btn btn-secondary">关闭</button>
            <button @click="deleteUserReview(currentUserReview); showReviewDetailModal = false" class="btn btn-danger">删除评价</button>
          </div>
        </div>
      </div>

      
    </div>

    <div v-else class="error">
      <p>订单不存在或加载失败</p>
      <button @click="goBack" class="btn btn-primary">返回</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, onBeforeUnmount } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import orderService from '@/services/orderService'
import { productService } from '@/services/product'

import userService from '@/services/userService'
import { notifySuccess, notifyError } from '@/utils/notify'
import { useUserStore } from '@/stores/user'
import { getAvatarUrl } from '@/utils/avatar.js'

const route = useRoute()
const router = useRouter()
const order = ref(null)
const loading = ref(true)
const error = ref('')
let pollInterval = null
const POLLING_INTERVAL = 8000
const previousOrderStatus = ref(null)

const statusHistory = ref([])
let lastOrderJson = ''
let lastHistoryJson = ''
let currentController = null
const showShippingModal = ref(false)
const shippingInfoForm = ref({
  company: '',
  trackingNumber: '',
  status: ''
})

// 评价相关数据
const showReviewModal = ref(false)
const currentReviewItem = ref(null)
const reviewForm = ref({
  rating: 5,
  comment: ''
})
const submittingReview = ref(false)
const userStore = useUserStore()
const userReviews = ref([])
const showReviewDetailModal = ref(false)
const currentUserReview = ref(null)
// 已移除物流浏览弹窗，改为在状态卡片内实时显示物流信息

// 收货地址信息
const userAddresses = ref([])
const isLoadingAddresses = ref(false)
const currentAddress = ref(null)

const loadOrderDetail = async (setLoading = true) => {
  try {
    const orderId = route.params.id
    if (setLoading) loading.value = true
    if (currentController) {
      try { currentController.abort() } catch {}
    }
    currentController = new AbortController()
    const reqOptions = { signal: currentController.signal }
    const [orderResponse, historyResponse] = await Promise.all([
      orderService.getOrderById(orderId, reqOptions),
      orderService.getOrderStatusHistory(orderId, reqOptions)
    ])
    
    // 处理订单详情（仅在变更时更新以避免频闪）
    let newOrder = null
    if (orderResponse && orderResponse.data) {
      newOrder = orderResponse.data
    } else if (orderResponse && typeof orderResponse === 'object') {
      newOrder = orderResponse
    }
    if (!newOrder) {
      error.value = '订单数据格式错误'
    } else {
      const json = JSON.stringify(newOrder)
      if (json !== lastOrderJson) {
        order.value = newOrder
        lastOrderJson = json
      }
    }
    
    // 处理状态历史（仅在变更时更新）
    let newHistory = []
    if (historyResponse && Array.isArray(historyResponse)) {
      newHistory = historyResponse
    } else if (historyResponse && historyResponse.data && Array.isArray(historyResponse.data)) {
      newHistory = historyResponse.data
    }
    const historyJson = JSON.stringify(newHistory)
    if (historyJson !== lastHistoryJson) {
      statusHistory.value = newHistory
      lastHistoryJson = historyJson
    }
    
    // 检查订单状态是否发生变化
    if (previousOrderStatus.value && previousOrderStatus.value !== order.value?.status) {
      console.log('订单状态已更新:', previousOrderStatus.value, '->', order.value.status)
      // 显示状态变更提示
      showStatusChangeNotification(previousOrderStatus.value, order.value.status)
    }
    
    // 更新之前的订单状态
    previousOrderStatus.value = order.value?.status
    
    // 获取订单详情后，同时获取最新的用户收货地址信息
    await loadUserAddresses()
    
    // 更新当前订单使用的地址信息
    updateCurrentAddress()

    // 物流信息在状态卡片内实时显示，无需弹窗
    
  } catch (error) {
    if (error?.name === 'CanceledError' || error?.name === 'AbortError') {
      // 静默取消，不记录为错误
      return
    }
    console.error('加载订单详情失败:', error)
    error.value = '加载订单详情失败，请稍后重试'
  } finally {
    loading.value = false
  }
}

// 从数据库加载用户地址信息
const loadUserAddresses = async () => {
  isLoadingAddresses.value = true
  try {
    console.log('从数据库加载最新收货地址信息...')
    const response = await userService.getAddresses()
    
    // 处理响应数据，确保格式统一
    const addressList = response.data || response.Data || []
    console.log('从数据库获取的地址列表:', addressList)
    
    // 格式化地址数据，确保字段名称一致性
    userAddresses.value = Array.isArray(addressList) ? addressList.map(addr => ({
      id: addr.id || addr.Id,
      name: addr.name || addr.RecipientName || addr.recipientName || '',
      phone: addr.phone || addr.PhoneNumber || addr.phoneNumber || '',
      province: addr.province || addr.Province || '',
      city: addr.city || addr.City || '',
      district: addr.district || addr.District || '',
      detailAddress: addr.detailAddress || addr.DetailAddress || '',
      fullAddress: addr.fullAddress || `${addr.province || addr.Province || ''}${addr.city || addr.City || ''}${addr.district || addr.District || ''}${addr.detailAddress || addr.DetailAddress || ''}`,
      isDefault: addr.isDefault || addr.IsDefault || false
    })) : []
    
    console.log('地址加载完成，共加载', userAddresses.value.length, '个地址')
  } catch (error) {
    console.error('加载收货地址失败:', error)
    userAddresses.value = []
  } finally {
    isLoadingAddresses.value = false
  }
}

// 更新当前订单使用的地址信息
const updateCurrentAddress = () => {
  if (!order.value) return
  
  // 优先使用用户地址列表中的默认地址，其次使用列表第一条
  if (userAddresses.value.length > 0) {
    const def = userAddresses.value.find(a => a.isDefault) || userAddresses.value[0]
    if (def) {
      currentAddress.value = def
      return
    }
  }
  
  // 如果找不到匹配的地址，则使用订单中的地址信息作为后备
  console.log('未找到匹配的地址，使用订单中的地址信息')
  const orderRecipientName = order.value.recipientName || order.value.RecipientName || ''
  const orderRecipientPhone = order.value.recipientPhone || order.value.Phone || ''
  currentAddress.value = {
    id: 'order_default',
    name: orderRecipientName || '未知收货人',
    phone: orderRecipientPhone || '未知电话',
    province: order.value.province || '',
    city: order.value.city || '',
    district: order.value.district || '',
    detailAddress: order.value.deliveryAddress || order.value.detailAddress || '',
    fullAddress: order.value.shippingAddress || order.value.fullAddress || 
                `${order.value.province || ''}${order.value.city || ''}${order.value.district || ''}${order.value.deliveryAddress || order.value.detailAddress || ''}`,
    isDefault: false
  }
}

// 监听页面可见性变化，实现页面重新聚焦时自动刷新地址
const handleVisibilityChange = async () => {
  if (!document.hidden) {
    await loadOrderDetail(false)
    await loadUserAddresses()
    updateCurrentAddress()
  }
}

const goBack = () => {
  router.push('/orders')
}

const formatDate = (dateString) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const getStatusClass = (status) => {
  const statusMap = {
    'pending': 'pending',
    'processing': 'processing',
    'shipped': 'shipped',
    'delivered': 'delivered',
    'cancelled': 'cancelled'
  }
  return statusMap[status] || 'pending'
}

const getStatusText = (status) => {
  // 与管理员端统一的状态文本映射
  const statusMap = {
    'pending': '待处理',
    'processing': '处理中',
    'shipped': '已发货',
    'delivered': '已送达',
    'cancelled': '已取消'
  }
  return statusMap[status] || '未知状态'
}

const getPaymentMethodText = (method) => {
  const methodMap = {
    'online': '在线支付',
    'cod': '货到付款',
    'alipay': '支付宝',
    'wechat': '微信支付',
    'credit_card': '信用卡',
    'cash_on_delivery': '货到付款'
  }
  return methodMap[method] || method
}

const getPaymentStatusText = (status) => {
  const statusMap = {
    'unpaid': '未支付',
    'paid': '已支付',
    'refunded': '已退款'
  }
  return statusMap[status] || status
}

// 获取物流公司名称
const getShippingCompanyName = (code) => {
  const companyMap = {
    'SF': '顺丰速运',
    'YT': '圆通速递',
    'YD': '韵达快递',
    'ZT': '中通快递',
    'EMS': 'EMS',
    'JD': '京东物流'
  }
  return companyMap[code] || code
}

// 获取历史状态文本
const getHistoryStatusText = (status) => {
  const statusMap = {
    'created': '订单创建',
    'pending': '等待支付',
    'paid': '支付完成',
    'processing': '订单处理中',
    'shipped': '订单已发货',
    'in_transit': '运输中',
    'out_for_delivery': '派送中',
    'delivered': '订单已送达',
    'cancelled': '订单已取消',
    'refunded': '已退款'
  }
  return statusMap[status]
}

// 检查用户是否为管理员
const isAdmin = computed(() => {
  const userRole = localStorage.getItem('userRole')
  return userRole === 'admin' || userRole === 'ADMIN'
})

// 显示状态变更通知
const showStatusChangeNotification = (oldStatus, newStatus) => {
  // 创建通知元素
  const notification = document.createElement('div')
  notification.className = 'status-notification'
  notification.textContent = `订单状态已更新：${getStatusText(oldStatus)} → ${getStatusText(newStatus)}`
  
  // 添加到页面
  document.body.appendChild(notification)
  
  // 添加CSS样式
  Object.assign(notification.style, {
    position: 'fixed',
    top: '20px',
    right: '20px',
    backgroundColor: '#4CAF50',
    color: 'white',
    padding: '12px 20px',
    borderRadius: '4px',
    zIndex: '10000',
    boxShadow: '0 2px 5px rgba(0,0,0,0.2)',
    fontSize: '14px',
    animation: 'slideIn 0.3s ease-out'
  })
  
  // 添加动画样式
  const style = document.createElement('style')
  style.textContent = `
    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }
  `
  document.head.appendChild(style)
  
  // 3秒后移除通知
  setTimeout(() => {
    notification.style.transition = 'opacity 0.3s ease-out, transform 0.3s ease-out'
    notification.style.opacity = '0'
    notification.style.transform = 'translateX(100%)'
    
    setTimeout(() => {
      document.body.removeChild(notification)
      document.head.removeChild(style)
    }, 300)
  }, 3000)
}

const cancelOrder = async () => {
  if (!confirm('确定要取消这个订单吗？')) {
    return
  }

  try {
    await orderService.cancelOrder(order.value.id)
    await loadOrderDetail()
    notifySuccess('订单已取消')
  } catch (error) {
    console.error('取消订单失败:', error)
    alert('取消订单失败，请重试')
  }
}

// 更新物流信息
const updateShipping = async () => {
  if (!confirm('确定要更新物流信息吗？')) {
    return
  }

  try {
    await orderService.updateShippingInfo(order.value.id, shippingInfoForm.value)
    await loadOrderDetail()
    showShippingModal.value = false
    notifySuccess('物流信息已更新')
  } catch (error) {
    console.error('更新物流信息失败:', error)
    notifyError('更新物流信息失败')
  }
}

// 打开物流信息编辑模态框
const openShippingModal = () => {
  // 初始化表单数据
  if (order.value?.shippingInfo) {
    shippingInfoForm.value = {
      company: order.value.shippingInfo.company || '',
      trackingNumber: order.value.shippingInfo.trackingNumber || '',
      status: order.value.shippingInfo.status || 'shipped'
    }
  } else {
    shippingInfoForm.value = {
      company: '',
      trackingNumber: '',
      status: 'shipped'
    }
  }
  showShippingModal.value = true
}

// 弹窗浏览已移除，点击徽章不再弹出

// 评价相关方法
const openReviewModal = (item) => {
  if (order.value?.status !== 'delivered') { notifyError('确认收货后才能评价'); return }
  currentReviewItem.value = item
  reviewForm.value = {
    rating: 5,
    comment: ''
  }
  showReviewModal.value = true
}

const submitReview = async () => {
  if (!currentReviewItem.value) return
  if (order.value?.status !== 'delivered') { notifyError('确认收货后才能评价'); return }
  
  try {
    submittingReview.value = true
    // 确保评价数据格式正确
    const reviewData = {
      productId: currentReviewItem.value.productId,
      rating: reviewForm.value.rating,
      comment: `@${currentReviewItem.value.productName} ${reviewForm.value.comment || ''}`.trim()
    }
    const resp = await productService.addProductReview(currentReviewItem.value.productId, reviewData)
    notifySuccess('评价提交成功！')
    
    // 标记该商品已评价
    currentReviewItem.value.reviewed = true
    currentReviewItem.value.reviewId = resp?.id || resp?.data?.id
    showReviewModal.value = false
    // 重新加载订单详情，确保数据最新
    await loadOrderDetail()
    await loadUserReviewsForOrder()
    // 刷新首页评价列表（如果在首页）
    if (window.location.pathname === '/') {
      await fetchReviews()
    }
  } catch (error) {
    console.error('提交评价失败:', error)
    notifyError('提交评价失败，请重试')
  } finally {
    submittingReview.value = false
  }
}

const loadUserReviewsForOrder = async () => {
  userReviews.value = []
  if (!order.value || !Array.isArray(order.value.orderItems)) return
  for (const it of order.value.orderItems) {
    try {
      const rv = await productService.getUserReviewForProduct(it.productId)
      const mine = rv?.data || rv
      if (mine && (mine.id || mine.Id)) {
        it.reviewed = true
        it.reviewId = mine.id || mine.Id
        userReviews.value.push({
          id: mine.id || mine.Id,
          productId: it.productId,
          productName: it.productName,
          rating: mine.rating || mine.Rating,
          comment: mine.comment || mine.Comment,
          avatar: getAvatarUrl(userStore.user?.avatar)
        })
      }
    } catch {}
  }
}

  const openReviewDetail = (rv) => {
    currentUserReview.value = rv
    showReviewDetailModal.value = true
  }
  
  const openReviewDetailByProduct = (item) => {
    const rv = userReviews.value.find(r => r.productId === item.productId)
    if (rv) {
      openReviewDetail(rv)
    } else {
      notifyError('未找到该商品的评价')
    }
  }

const deleteUserReview = async (rv) => {
  if (!rv?.id) return
  try {
    await productService.deleteProductReview(rv.id)
    userReviews.value = userReviews.value.filter(x => x.id !== rv.id)
    if (order.value && Array.isArray(order.value.orderItems)) {
      const it = order.value.orderItems.find(x => x.productId === rv.productId)
      if (it) {
        it.reviewed = false
        it.reviewId = null
      }
    }
    notifySuccess('评价已删除')
  } catch (e) {
    notifyError('删除评价失败')
  }
}

// 组合最新物流备注文本
const latestShippingNote = computed(() => {
  try {
    if (!order.value?.shippingInfo) return ''
    const company = getShippingCompanyName(order.value.shippingInfo.company)
    const tracking = order.value.shippingInfo.trackingNumber
    // 找到最近一次发货状态的历史记录
    const shippedHistories = (statusHistory.value || []).filter(h => (h.status || '').toLowerCase() === 'shipped')
    const latest = shippedHistories.length > 0 ? shippedHistories[shippedHistories.length - 1] : null
    const timeStr = latest?.timestamp ? formatDate(latest.timestamp) : ''
    const operator = latest?.operatorName || ''
    const suffixParts = ['已发货', timeStr, operator].filter(Boolean)
    const suffix = suffixParts.length ? `（${suffixParts.join(' ・ ')}）` : ''
    return `快递公司：${company}；单号：${tracking}${suffix}`
  } catch { return '' }
})
const payOrder = async () => {
  if (!order.value) return;
  
  try {
    loading.value = true
    const useVirtualPayment = confirm('是否使用虚拟付款进行测试？\n\n确定 = 使用虚拟付款（立即成功）\n取消 = 使用真实支付流程')
    console.log('支付方式选择:', useVirtualPayment ? '虚拟付款' : '真实支付')
    await new Promise(resolve => setTimeout(resolve, 1200))
    notifySuccess('虚拟付款成功！')
    // 使用后端支付状态接口更新为已支付
    await orderService.updatePaymentStatus(order.value.id, { paymentStatus: 'paid', paymentMethod: 'online', paymentReference: 'VIRTUAL' })
    await loadOrderDetail()
  } catch (error) {
    console.error('支付失败:', error);
    notifyError('支付过程中出现错误')
  } finally {
    loading.value = false;
  }
}

// 启动轮询
const startPolling = () => {
  loadOrderDetail(true)
  pollInterval = setInterval(() => {
    if (!document.hidden) {
      loadOrderDetail(false)
    }
  }, POLLING_INTERVAL)
}

// 停止轮询
const stopPolling = () => {
  if (pollInterval) {
    clearInterval(pollInterval)
    pollInterval = null
  }
}

onMounted(async () => {
  loading.value = true
  startPolling()
  document.addEventListener('visibilitychange', handleVisibilityChange)
  
  // 处理URL参数，如果有review=true则自动打开评价弹窗
  const { review } = route.query
  if (review === 'true') {
    // 等待订单数据加载完成
    await new Promise(resolve => {
      const checkOrder = setInterval(() => {
        if (order.value && order.value.orderItems && order.value.orderItems.length > 0) {
          clearInterval(checkOrder)
          resolve()
        }
      }, 100)
    })
    
    // 自动打开第一个商品的评价弹窗
    if (order.value.orderItems && order.value.orderItems.length > 0) {
      openReviewModal(order.value.orderItems[0])
    }
  }
  await loadUserReviewsForOrder()
})

// 组件卸载时停止轮询和移除事件监听
onUnmounted(() => {
  stopPolling()
  document.removeEventListener('visibilitychange', handleVisibilityChange)
  if (currentController) {
    try { currentController.abort() } catch {}
  }
})

// 移除重复的 onMounted 监听，避免多次绑定
</script>

<style scoped>
.order-detail-page {
  max-width: 900px;
  margin: 0 auto;
  padding: 2rem;
}

.page-header {
  margin-bottom: 2rem;
}

.back-btn {
  background: none;
  border: none;
  color: #666;
  cursor: pointer;
  font-size: 1rem;
  margin-bottom: 1rem;
  transition: color 0.3s ease;
}

.back-btn:hover {
  color: #ff69b4;
}

.page-header h1 {
  color: #333;
  margin: 0;
}

.loading {
  text-align: center;
  padding: 3rem;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #f3f3f3;
  border-top: 3px solid #ff69b4;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.loading-address .info-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  color: #666;
}

.loading-address .loading-spinner {
  width: 20px;
  height: 20px;
  border: 2px solid #f3f3f3;
  border-top: 2px solid #ff69b4;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-right: 10px;
}

.error {
  text-align: center;
  padding: 3rem;
}

.order-detail {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.status-card {
  background: white;
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.status-info h2 {
  margin: 0 0 1rem 0;
  color: #333;
}

.status-display {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.status-badge {
  padding: 0.5rem 1rem;
  border-radius: 20px;
  font-size: 1rem;
  font-weight: 500;
}
.status-badge.clickable {
  cursor: pointer;
  box-shadow: 0 0 0 1px rgba(255,105,180,0.3) inset;
}

.status-badge.pending {
  background: #fff3cd;
  color: #856404;
}

.status-badge.paid {
  background: #d4edda;
  color: #155724;
}

.status-badge.shipped {
  background: #cce5ff;
  color: #004085;
}

.status-badge.delivered {
  background: #d1ecf1;
  color: #0c5460;
}

.status-badge.cancelled {
  background: #f8d7da;
  color: #721c24;
}

.order-number {
  color: #666;
  font-family: monospace;
}

.status-actions {
  display: flex;
  gap: 0.75rem;
}

.info-card, .items-card, .summary-card, .timeline-card {
  background: white;
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

.info-card h3, .items-card h3, .timeline-card h3 {
  margin: 0 0 1.5rem 0;
  color: #333;
}

.info-content {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.info-item {
  display: flex;
  align-items: center;
}

.info-item label {
  font-weight: 500;
  color: #666;
  min-width: 100px;
}

.payment-status.unpaid {
  color: #dc3545;
}

.payment-status.paid {
  color: #28a745;
}

.items-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.order-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
  background: #fafafa;
  border-radius: 8px;
}

.item-image {
  width: 80px;
  height: 80px;
  object-fit: cover;
  border-radius: 8px;
}

.item-details {
  flex: 1;
}

.item-details h4 {
  margin: 0 0 0.5rem 0;
  color: #333;
}

.item-price {
  margin: 0;
  color: #666;
  font-size: 0.875rem;
}

.item-quantity {
  color: #666;
  font-size: 1.1rem;
}

.item-total {
  font-weight: 600;
  color: #ff69b4;
  font-size: 1.1rem;
}

.summary-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 0;
  border-bottom: 1px solid #f0f0f0;
}

.summary-item:last-of-type {
  border-bottom: none;
}

.summary-total {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 0;
  border-top: 2px solid #f0f0f0;
  margin-top: 0.5rem;
  font-weight: 600;
  font-size: 1.1rem;
}

.total-amount {
  color: #ff69b4;
  font-size: 1.25rem;
  font-weight: 700;
}

.timeline {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.timeline-item {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
}

.timeline-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  margin-top: 4px;
  flex-shrink: 0;
}

.timeline-dot.created {
  background: #ff69b4;
}

.timeline-dot.updated {
  background: #17a2b8;
}

.timeline-content {
  flex: 1;
}

.timeline-title {
  margin: 0 0 0.25rem 0;
  color: #333;
  font-weight: 500;
}

.timeline-time {
  margin: 0;
  color: #666;
  font-size: 0.875rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.875rem;
  transition: all 0.3s ease;
  text-decoration: none;
  display: inline-block;
}

.btn-primary {
  background: #ff69b4;
  color: white;
}

.btn-primary:hover {
  background: #ff1493;
}

.btn-danger {
  background: #dc3545;
  color: white;
}

.btn-danger:hover {
  background: #c82333;
}

@media (max-width: 768px) {
  .order-detail-page {
    padding: 1rem;
  }

  .status-card {
    flex-direction: column;
    align-items: flex-start;
    gap: 1.5rem;
  }

  .status-actions {
    width: 100%;
    justify-content: flex-end;
  }

  .order-item {
    gap: 0.75rem;
  }

  .item-image {
    width: 60px;
    height: 60px;
  }

  .info-item {
    flex-direction: column;
    align-items: flex-start;
  }

  .info-item label {
    min-width: auto;
  }
}

/* 评价相关样式 */
.item-actions {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  margin-left: auto;
}

.review-btn {
  background: #ff69b4;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.875rem;
  transition: all 0.3s ease;
}

.review-btn:hover {
  background: #ff1493;
}

.reviewed-tag {
  color: #666;
  font-size: 0.875rem;
  background: #f0f0f0;
  padding: 0.375rem 0.75rem;
  border-radius: 12px;
}

/* 评价模态框样式 */
.review-modal .modal-body {
  padding: 1.5rem;
}

.review-item-info {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1.5rem;
  padding: 1rem;
  background: #fafafa;
  border-radius: 8px;
}

.review-item-image {
  width: 60px;
  height: 60px;
  object-fit: cover;
  border-radius: 8px;
}

.review-item-info h4 {
  margin: 0;
  color: #333;
}

.rating-stars {
  display: flex;
  gap: 0.5rem;
  margin: 0.5rem 0;
}

.star {
  font-size: 2rem;
  color: #ddd;
  cursor: pointer;
  transition: all 0.2s ease;
}

.star:hover, .star.filled {
  color: #ffd700;
}

.rating-text {
  display: block;
  margin-top: 0.5rem;
  color: #666;
  font-size: 0.875rem;
}

.form-control {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 1rem;
  transition: border-color 0.3s ease;
}

.form-control:focus {
  outline: none;
  border-color: #ff69b4;
  box-shadow: 0 0 0 2px rgba(255, 105, 180, 0.1);
}

textarea.form-control {
  resize: vertical;
  min-height: 100px;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
  color: #333;
}
</style>
