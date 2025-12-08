<template>
  <PageTransition>
    <div class="orders-page">
      <!-- 页面加载动画 -->
      <LoadingSpinner v-if="isLoading" />
      
      <div class="page-header">
        <h1>我的订单</h1>
        <p>查看和管理您的所有订单</p>
      </div>

      <div class="orders-container">
        <div v-if="bannerMessage" :class="['banner', bannerType]" class="mb-4 p-3 rounded">
          {{ bannerMessage }}
        </div>
        <div v-if="loading" class="loading">
          <div class="loading-spinner"></div>
          <p>加载订单中...</p>
        </div>

        <div v-else-if="orders.length === 0" class="empty-orders">
          <div class="empty-icon">📦</div>
          <h3>暂无订单</h3>
          <p>您还没有任何订单，快去选购鲜花吧！</p>
          <router-link to="/products" class="btn btn-primary">去选购</router-link>
        </div>

        <div v-else class="orders-list">
          <div v-for="order in orders" :key="order.id" class="order-card">
            <div class="order-header">
              <div class="order-info">
                <h3 class="order-number">订单号: {{ order.orderNumber || order.id }}</h3>
                <p class="order-date">{{ formatDate(order.createdAt) }}</p>
              </div>
              <div class="order-status">
                <span :class="['status-badge', getStatusClass(order.status)]">
                  <i :class="getStatusIcon(order.status)"></i>
                  {{ getStatusText(order.status) }}
                </span>
                <button 
                  v-if="order.status === 'shipped'"
                  @click="confirmReceipt(order.id)"
                  class="confirm-receipt-btn"
                >
                  确认收货
                </button>
                <button 
                  v-else-if="order.status === 'delivered' && (order._hasUnreviewed ?? true)"
                  @click="goToReview(order.id)"
                  class="confirm-receipt-btn"
                >
                  立即去评价
                </button>
                <button 
                  v-else-if="order.status === 'delivered' && !(order._hasUnreviewed ?? true)"
                  @click="openOrderReviewPage(order)"
                  class="confirm-receipt-btn"
                >
                  查看评价
                </button>
              </div>
            </div>
            
            <div class="order-items">
              <div class="items-header">
                <h4>商品信息</h4>
                <span class="items-count">共 {{ order.orderItems?.length || 0 }} 件商品</span>
              </div>
              
              <div class="items-list">
                <div v-for="(item, index) in order.orderItems?.slice(0, 3)" :key="item.id" class="order-item">
                  <div class="item-image">
                    <img :src="item.productImage || '/placeholder-flower.jpg'" :alt="item.productName">
                  </div>
                  <div class="item-details">
                    <h5 class="item-name">{{ item.productName }}</h5>
                    <p class="item-quantity">数量: {{ item.quantity }}</p>
                  </div>
                  <div class="item-price">
                    <span class="price">¥{{ (item.unitPrice || 0).toFixed(2) }}</span>
                    <span class="total">¥{{ (item.totalPrice || 0).toFixed(2) }}</span>
                  </div>
                </div>
                
                <div v-if="order.orderItems?.length > 3" class="more-items">
                  <i class="fas fa-ellipsis-h"></i>
                  <span>还有 {{ order.orderItems.length - 3 }} 件商品</span>
                </div>
              </div>
            </div>
            
            <div class="order-summary">
              <div class="summary-row">
                <span class="label">商品总价:</span>
                <span class="value">¥{{ (order.totalAmount || 0).toFixed(2) }}</span>
              </div>
              <div class="summary-row">
                <span class="label">配送费用:</span>
                <span class="value">{{ order.shippingFee === 0 ? '免运费' : `¥${order.shippingFee?.toFixed(2) || '0.00'}` }}</span>
              </div>
              <div v-if="order.discount" class="summary-row discount">
                <span class="label">优惠金额:</span>
                <span class="value">-¥{{ order.discount.toFixed(2) }}</span>
              </div>
              <div class="summary-row total">
                <span class="label">实付金额:</span>
                <span class="value total-amount">¥{{ (order.finalAmount || order.totalAmount || 0).toFixed(2) }}</span>
              </div>
            </div>
            
            <div class="order-actions">
              <button @click="viewOrderDetail(order.id)" class="action-btn primary">
                <i class="fas fa-eye"></i>
                查看详情
              </button>
              
              <button 
                v-if="order.status === 'pending'" 
                @click="cancelOrder(order.id)" 
                class="action-btn danger"
              >
                <i class="fas fa-times-circle"></i>
                取消订单
              </button>

              <button
                v-if="order.status === 'pending' && (order.paymentStatus === 'unpaid' || !order.paymentStatus)"
                @click="payOrder(order)"
                class="action-btn secondary"
              >
                <i class="fas fa-credit-card"></i>
                立即付款
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
    <div v-if="showOrderReviewModal" class="modal-overlay" @click.self="closeOrderReviewModal">
      <div class="modal" @click.stop>
        <div class="modal-header">
          <h3>评价详情</h3>
          <button class="action-btn secondary" @click="closeOrderReviewModal">关闭</button>
        </div>
        <div class="modal-body">
          <div v-if="currentOrderReviews.length === 0" class="empty-orders">
            暂无评价
          </div>
          <div v-else class="items-list">
            <div v-for="rv in currentOrderReviews" :key="rv.id" class="order-item">
              <div class="item-image">
                <div :style="{ backgroundImage: `url(${rv.avatar})` }" class="w-16 h-16 bg-center bg-cover rounded-full"></div>
              </div>
              <div class="item-details">
                <h4>@{{ rv.productName }}</h4>
                <div class="flex text-yellow-400">
                  <span v-for="star in 5" :key="star" class="text-sm">{{ star <= (rv.rating || 0) ? '★' : '☆' }}</span>
                </div>
                <p class="item-quantity">{{ rv.comment }}</p>
                <div class="text-sm text-gray-500">{{ formatDateTime(rv.createdAt) }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </PageTransition>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import PageTransition from '@/components/PageTransition.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import orderService from '@/services/orderService'
import { productService } from '@/services/product'
import { getAvatarUrl } from '@/utils/avatar.js'

import { notifySuccess, notifyError } from '@/utils/notify'

// 响应式数据
const orders = ref([])
const loading = ref(false)
const isLoading = ref(false)
const router = useRouter()
const bannerMessage = ref('')
const bannerType = ref('info')
const setBanner = (msg, type = 'info') => { bannerMessage.value = msg; bannerType.value = type; setTimeout(() => { bannerMessage.value = '' }, 2500) }

const showOrderReviewModal = ref(false)
const currentOrderReviews = ref([])
const currentOrder = ref(null)

// 方法
const formatDate = (dateString) => {
  if (!dateString) return ''
  return new Date(dateString).toLocaleDateString('zh-CN')
}

const getStatusClass = (status) => {
  const statusMap = {
    pending: 'status-pending',
    processing: 'status-processing',
    shipped: 'status-shipped',
    delivered: 'status-delivered',
    cancelled: 'status-cancelled',
    completed: 'status-completed'
  }
  return statusMap[status] || 'status-default'
}

const getStatusText = (status) => {
  const statusMap = {
    pending: '待付款',
    processing: '处理中',
    shipped: '已发货',
    delivered: '已签收',
    cancelled: '已取消',
    completed: '已完成'
  }
  return statusMap[status] || status
}

const getStatusIcon = (status) => {
  const iconMap = {
    pending: 'fas fa-clock',
    processing: 'fas fa-cog',
    shipped: 'fas fa-truck',
    delivered: 'fas fa-check-circle',
    cancelled: 'fas fa-times-circle',
    completed: 'fas fa-check'
  }
  return iconMap[status] || 'fas fa-question-circle'
}

const viewOrderDetail = (orderId) => {
  router.push(`/orders/${orderId}`)
}

const viewShipping = (order) => {
  const id = order.id || order.orderId
  if (!id) return
  router.push({ path: `/orders/${id}`, query: { showShipping: '1' } })
}

const cancelOrder = async (orderId) => {
  await orderService.cancelOrder(orderId)
  await loadOrders()
  notifySuccess('订单已取消')
  setBanner('订单已取消', 'success')
}

const payOrder = async (order) => {
  // 模拟支付过程
  try {
    // 这里可以添加实际的支付逻辑
    await new Promise(resolve => setTimeout(resolve, 1500))
    
    // 直接调用orderService更新订单状态为已支付
    await orderService.updateOrderStatus(order.id, 'paid')
    await loadOrders()
    notifySuccess('支付成功')
    setBanner('支付成功', 'success')
  } catch (error) {
    notifyError('支付失败')
    setBanner('支付失败', 'error')
  }
}

// 确认收货函数
  const confirmReceipt = async (orderId) => {
    try {
      if (confirm('确认已收到商品吗？确认后将无法更改。')) {
        // 调用orderService更新订单状态为已确认收货
        await orderService.updateOrderStatus(orderId, { status: 'delivered' })
        await loadOrders()
        notifySuccess('已确认收货')
        setBanner('已确认收货', 'success')
      }
    } catch (error) {
      console.error('确认收货失败:', error)
      notifyError('确认收货失败，请重试')
      setBanner('确认收货失败', 'error')
    }
  }

// 立即去评价函数
const goToReview = (orderId) => {
  router.push(`/orders/${orderId}?review=true`)
}

const goToOrderReviews = (orderId) => {
  router.push(`/orders/${orderId}`)
}

const openOrderReviewPage = async (order) => {
  if (!Array.isArray(order.orderItems) || order.orderItems.length === 0) {
    notifyError('该订单暂无评价')
    return
  }
  for (const it of order.orderItems) {
    try {
      const rv = await productService.getUserReviewForProduct(it.productId)
      const mine = rv?.data || rv
      const rid = mine?.id || mine?.Id
      if (rid) {
        await router.push(`/reviews/${rid}?productId=${it.productId}`)
        return
      }
    } catch {}
  }
  notifyError('该订单暂无评价')
}

const closeOrderReviewModal = () => { showOrderReviewModal.value = false }

const formatDateTime = (val) => {
  if (!val) return ''
  try { return new Date(val).toLocaleString('zh-CN') } catch { return '' }
}

const loadOrders = async () => {
  loading.value = true
  try {
    const res = await orderService.getUserOrders()
    // 正确处理响应数据：orderService返回{ success: true, data: 订单列表 }
    // 而orderService内部的api.get('/Orders')返回的是订单列表本身
    orders.value = res?.data || []
    if (!orders.value || orders.value.length === 0) {
      setBanner('暂无订单', 'info')
    }
    await updateDeliveredOrdersReviewStates()
  } catch (e) {
    notifyError('加载订单失败')
    setBanner('加载订单失败', 'error')
    orders.value = []
  } finally {
    loading.value = false
  }
}

const updateDeliveredOrdersReviewStates = async () => {
  const list = Array.isArray(orders.value) ? orders.value : []
  for (const o of list) {
    if (o.status !== 'delivered' || !Array.isArray(o.orderItems)) { o._hasUnreviewed = true; continue }
    let unreviewed = false
    for (const it of o.orderItems) {
      try {
        const rv = await productService.getUserReviewForProduct(it.productId)
        const mine = rv?.data || rv
        if (!mine || !(mine.id || mine.Id)) { unreviewed = true } else { it.reviewed = true }
      } catch { unreviewed = true }
    }
    o._hasUnreviewed = unreviewed
  }
}

onMounted(() => {
  loadOrders()
})
</script>

<style scoped>
/* 订单页面样式 */
.orders-page {
  min-height: 100vh;
  background-color: #f8f9fa;
  padding: 20px;
}

.page-header {
  text-align: center;
  margin-bottom: 40px;
}

.page-header h1 {
  font-size: 2.5rem;
  color: #333;
  margin-bottom: 10px;
}

.page-header p {
  font-size: 1.1rem;
  color: #666;
}

.orders-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 20px;
}

.loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 0;
  color: #666;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #f3f3f3;
  border-top: 3px solid #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 20px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.empty-orders {
  text-align: center;
  padding: 60px 20px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 20px;
}

.empty-orders h3 {
  font-size: 1.8rem;
  color: #333;
  margin-bottom: 10px;
}

.empty-orders p {
  color: #666;
  margin-bottom: 30px;
}

.btn-primary {
  display: inline-block;
  padding: 12px 24px;
  background: linear-gradient(135deg, #ff6b6b, #ff8e53);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  text-decoration: none;
}

.btn-primary:hover {
  background: linear-gradient(135deg, #ee5a5a, #ee7c3b);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(255, 107, 107, 0.3);
}

.orders-list {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.order-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  overflow: hidden;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.order-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #eee;
  background: #f8f9fa;
}

.order-info h3 {
  font-size: 1.3rem;
  color: #333;
  margin-bottom: 5px;
}

.order-info p {
  color: #666;
  font-size: 0.9rem;
}

.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  border-radius: 20px;
  font-size: 0.9rem;
  font-weight: 500;
}

.status-badge.clickable {
  cursor: pointer;
  transition: all 0.2s ease;
}

.status-badge.clickable:hover {
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.status-pending {
  background: #fff3cd;
  color: #856404;
}

.status-processing {
  background: #d1ecf1;
  color: #0c5460;
}

.status-shipped {
  background: #e7f3ff;
  color: #004085;
}

.status-delivered {
  background: #d4edda;
  color: #155724;
}

.status-cancelled {
  background: #f8d7da;
  color: #721c24;
}

.status-completed {
  background: #c3e6cb;
  color: #155724;
}

/* 确认收货按钮样式 */
.confirm-receipt-btn {
  margin-left: 12px;
  padding: 6px 12px;
  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
}

.confirm-receipt-btn:hover {
  background: linear-gradient(135deg, #3ca9ff 0%, #00e1fe 100%);
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(79, 172, 254, 0.3);
}

/* 调整订单状态区域的布局 */
.order-status {
  display: flex;
  align-items: center;
}

.order-items {
  padding: 24px;
}

.items-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #eee;
}

.items-header h4 {
  font-size: 1.1rem;
  color: #333;
  font-weight: 600;
}

.items-count {
  color: #666;
  font-size: 0.9rem;
}

.items-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.order-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  border: 1px solid #f0f0f0;
  border-radius: 8px;
  background: #fafafa;
}

.item-image {
  width: 80px;
  height: 80px;
  border-radius: 8px;
  overflow: hidden;
  flex-shrink: 0;
}

.item-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.item-details {
  flex: 1;
  min-width: 0;
}

.item-name {
  font-size: 1rem;
  color: #333;
  font-weight: 500;
  margin-bottom: 8px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.item-quantity {
  color: #666;
  font-size: 0.9rem;
}

.item-price {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 4px;
  min-width: 100px;
}

.price {
  color: #666;
  font-size: 0.9rem;
  text-decoration: line-through;
}

.total {
  color: #ff6b6b;
  font-size: 1.1rem;
  font-weight: 600;
}

.more-items {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 12px;
  color: #666;
  font-size: 0.9rem;
  border-top: 1px dashed #eee;
  margin-top: 8px;
}

.order-summary {
  padding: 24px;
  border-top: 1px solid #eee;
  background: #f8f9fa;
}

.summary-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  font-size: 0.95rem;
}

.summary-row:last-child {
  margin-bottom: 0;
}

.summary-row .label {
  color: #666;
}

.summary-row .value {
  color: #333;
  font-weight: 500;
}

.summary-row.discount .value {
  color: #ff6b6b;
  font-weight: 600;
}

.summary-row.total {
  padding-top: 16px;
  border-top: 1px solid #eee;
  margin-top: 8px;
  font-size: 1.1rem;
}

.total-amount {
  color: #ff6b6b;
  font-size: 1.3rem;
  font-weight: 700;
}

.order-actions {
  display: flex;
  gap: 12px;
  padding: 24px;
  border-top: 1px solid #eee;
  justify-content: flex-end;
  background: #fff;
}

.action-btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  font-size: 0.95rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s ease;
  min-width: 120px;
  justify-content: center;
}

.action-btn.primary {
  background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);
  color: white;
}

.action-btn.primary:hover {
  background: linear-gradient(135deg, #3ca9ff 0%, #00e1fe 100%);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(79, 172, 254, 0.3);
}

.action-btn.danger {
  background: linear-gradient(135deg, #dc3545 0%, #e74c3c 100%);
  color: white;
}

.action-btn.danger:hover {
  background: linear-gradient(135deg, #c82333 0%, #c0392b 100%);
}

.action-btn.warning {
  background: linear-gradient(135deg, #ffc107 0%, #fd7e14 100%);
  color: #212529;
}

.action-btn.warning:hover {
  background: linear-gradient(135deg, #e0a800 0%, #e8590c 100%);
}

.action-btn.info {
  background: linear-gradient(135deg, #17a2b8 0%, #138496 100%);
  color: white;
}

.action-btn.info:hover {
  background: linear-gradient(135deg, #138496 0%, #117a8b 100%);
}

.action-btn.secondary {
  background: linear-gradient(135deg, #6c757d 0%, #5a6268 100%);
  color: white;
}

.action-btn.secondary:hover {
  background: linear-gradient(135deg, #5a6268 0%, #495057 100%);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .orders-container {
    padding: 15px;
  }
  
  .page-header h1 {
    font-size: 2rem;
  }
  
  .order-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
    padding: 16px 20px;
  }
  
  .order-info h3 {
    font-size: 1.1rem;
  }
  
  .order-item {
    padding: 12px;
    gap: 12px;
  }
  
  .item-image {
    width: 50px;
    height: 50px;
  }
  
  .item-details {
    min-width: 0;
  }
  
  .item-name {
    font-size: 0.9rem;
  }
  
  .order-actions {
    padding: 16px 20px;
    justify-content: center;
  }
  
  .action-btn {
    padding: 8px 16px;
    font-size: 0.85rem;
    flex: 1;
    min-width: 120px;
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .orders-container {
    padding: 10px;
  }
  
  .order-header {
    padding: 12px 16px;
  }
  
  .order-items,
  .order-summary,
  .order-actions {
    padding: 16px;
  }
  
  .status-badge {
    padding: 6px 12px;
    font-size: 0.8rem;
  }
  
  .action-btn {
    padding: 8px 12px;
    font-size: 0.8rem;
    min-width: 100px;
  }
}

/* 动画效果 */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.order-card {
  animation: fadeIn 0.5s ease-out;
}

.order-card:nth-child(1) { animation-delay: 0.1s; }
.order-card:nth-child(2) { animation-delay: 0.2s; }
.order-card:nth-child(3) { animation-delay: 0.3s; }
.order-card:nth-child(4) { animation-delay: 0.4s; }
.order-card:nth-child(5) { animation-delay: 0.5s; }
</style>
const showOrderReviewModal = ref(false)
const currentOrderReviews = ref([])
const currentOrder = ref(null)
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  background: #fff;
  border-radius: 12px;
  max-width: 800px;
  width: 92%;
  overflow: hidden;
  box-shadow: 0 10px 30px rgba(0,0,0,0.15);
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  border-bottom: 1px solid #eee;
}

.modal-body {
  padding: 20px;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 16px 20px;
  border-top: 1px solid #eee;
}
