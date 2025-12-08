<template>
  <PageTransition>
    <div class="cart-page">
      <!-- 页面加载动画 -->
      <LoadingSpinner v-if="isLoading" />
      
      <div class="container">
        <div class="page-header">
          <h1>购物车</h1>
        </div>

        <div v-if="bannerMessage" :class="['banner', bannerType]" class="mb-3 p-3 rounded">
          {{ bannerMessage }}
        </div>

        <!-- 错误提示 -->
        <div v-if="cartStore.error" class="error-message">
          {{ cartStore.error }}
          <button @click="cartStore.clearError()" class="close-btn">×</button>
        </div>

        <!-- 购物车为空 -->
        <div v-if="cartStore.isEmpty && !cartStore.loading" class="empty-cart">
          <div class="empty-icon">🛒</div>
          <h3>购物车是空的</h3>
          <p>快去挑选心仪的花卉吧！</p>
          <router-link to="/products" class="btn btn-primary">
            去购物
          </router-link>
        </div>

        <!-- 购物车商品列表 -->
        <div v-else class="cart-content">
          <div class="cart-items">
            <div v-if="cartStore.loading" class="loading">
              <div class="spinner"></div>
              <p>加载中...</p>
            </div>

            <div v-else>
              <div v-for="item in cartStore.cartItems" :key="item.id" class="cart-item">
                <div class="item-image">
                  <img :src="item.productImage || '/placeholder-flower.jpg'" :alt="item.productName" @error="handleImageError" />
                  <div class="item-badges">
                    <span v-if="item.isHot" class="badge hot">热卖</span>
                    <span v-if="item.isNew" class="badge new">新品</span>
                  </div>
                </div>

                <div class="item-details">
                  <div class="item-header">
                    <h3>{{ item.productName }}</h3>
                    <button @click="removeItem(item.id)" class="remove-btn" title="移除商品">
                      <i class="fas fa-trash-alt"></i>
                    </button>
                  </div>
                  
                  <div class="item-meta">
                    <span v-if="item.stock !== undefined" class="stock-info" :class="{ 
                      'low-stock': item.stock < 10 && item.stock > 0,
                      'out-of-stock': item.stock === 0 
                    }">
                      <i class="fas fa-box"></i>
                      <template v-if="item.stock > 0">库存: {{ item.stock }}</template>
                      <template v-else>已售罄</template>
                      <span v-if="item.stock < 5 && item.stock > 0" class="urgent-tip">（仅剩几件）</span>
                    </span>
                    <span v-if="item.salesCount" class="sales-info">
                      <i class="fas fa-shopping-bag"></i>
                      已售: {{ item.salesCount }}
                    </span>
                  </div>

                  <div class="item-footer">
                    <div class="quantity-control">
                      <button 
                        @click="updateQuantity(item.id, item.quantity - 1)"
                        :disabled="item.quantity <= 1 || item.stock === 0"
                        class="quantity-btn"
                        title="减少数量"
                      >
                        <i class="fas fa-minus"></i>
                      </button>
                      <span class="quantity-display" :class="{ 'invalid-quantity': item.quantity > item.stock && item.stock > 0 }">
                        {{ item.quantity }}
                      </span>
                      <button 
                        @click="updateQuantity(item.id, item.quantity + 1)"
                        :disabled="item.quantity >= item.stock || item.stock === 0"
                        class="quantity-btn"
                        title="增加数量"
                      >
                        <i class="fas fa-plus"></i>
                      </button>
                    </div>
                    <div v-if="item.quantity > item.stock && item.stock > 0" class="stock-warning">
                      库存不足，请减少购买数量
                    </div>

                    <div class="item-price-info">
                      <div class="price-row">
                        <span class="price-label">单价:</span>
                        <span class="unit-price">¥{{ item.price.toFixed(2) }}</span>
                      </div>
                      <div class="price-row total">
                        <span class="price-label">小计:</span>
                        <span class="item-total">¥{{ (item.price * item.quantity).toFixed(2) }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 购物车总结 -->
          <div class="cart-summary">
            <div class="summary-content">
              <h3 class="summary-title">订单总结</h3>
              
              <div class="summary-row">
                <span class="summary-label">商品数量</span>
                <span class="summary-value">{{ cartStore.cartCount }} 件</span>
              </div>
              
              <div class="summary-row subtotal">
                <span class="summary-label">商品总价</span>
                <span class="summary-value">¥{{ cartStore.cartTotal.toFixed(2) }}</span>
              </div>
              
              <div class="summary-row shipping">
                <span class="summary-label">配送费用</span>
                <span class="summary-value shipping-fee">
                  {{ cartStore.cartTotal >= 88 ? '免运费' : '¥10.00' }}
                </span>
              </div>
              
              <div class="summary-row discount" v-if="cartStore.cartTotal >= 200">
                <span class="summary-label">
                  <i class="fas fa-tag"></i>
                  满减优惠
                </span>
                <span class="summary-value discount-amount">-¥20.00</span>
              </div>

              <div class="summary-row">
                <span class="summary-label">优惠券</span>
                <div class="summary-value" style="width: 60%">
                  <div class="flex gap-2">
                    <input v-model="couponCode" placeholder="输入优惠券代码" class="w-full border rounded px-2 py-1" />
                    <button class="btn btn-outline" @click="applyCoupon" :disabled="applyingCoupon || !couponCode">{{ applyingCoupon ? '验证中' : '使用' }}</button>
                    <button class="btn btn-outline" @click="clearCoupon" :disabled="!appliedCoupon">取消</button>
                  </div>
                  <div v-if="couponMessage" class="text-sm mt-1" :class="{'text-green-600': couponDiscount>0, 'text-red-600': couponDiscount===0}">{{ couponMessage }}</div>
                  <div v-if="myCoupons.length" class="text-xs mt-2">
                    <span>可用：</span>
                    <button v-for="c in myCoupons" :key="c.Id||c.id" class="px-2 py-1 border rounded mr-1 mb-1" @click="useMyCoupon(c)">
                      {{ (c.Code||c.code) }}
                    </button>
                  </div>
                </div>
              </div>

              <div class="summary-row discount" v-if="couponDiscount>0">
                <span class="summary-label">
                  <i class="fas fa-ticket"></i>
                  优惠券折扣
                </span>
                <span class="summary-value discount-amount">-¥{{ couponDiscount.toFixed(2) }}</span>
              </div>
              
              <div class="divider"></div>
              
              <div class="summary-row total">
                <span class="summary-label">应付总额</span>
                <span class="summary-value total-price">
                  ¥{{ ( (cartStore.cartTotal >= 88 ? cartStore.cartTotal : cartStore.cartTotal + 10) - (cartStore.cartTotal >= 200 ? 20 : 0) - couponDiscount ).toFixed(2) }}
                </span>
              </div>
              
              <div class="savings-info" v-if="cartStore.cartTotal >= 88 || cartStore.cartTotal >= 200">
                <i class="fas fa-piggy-bank"></i>
                <span>您已节省 ¥{{ ((cartStore.cartTotal >= 88 ? 10 : 0) + (cartStore.cartTotal >= 200 ? 20 : 0)).toFixed(2) }}</span>
              </div>
            </div>
            
            <div class="summary-actions">
              <button 
                @click="clearCart" 
                class="btn btn-outline"
                :disabled="cartStore.isEmpty || cartStore.loading"
              >
                <i class="fas fa-broom"></i>
                清空购物车
              </button>
              
              <button 
                @click="proceedToCheckout" 
                class="btn btn-primary"
                :disabled="cartStore.isEmpty || cartStore.loading || hasOutOfStockItems || hasInvalidQuantity"
              >
                <i class="fas fa-credit-card"></i>
                <template v-if="hasOutOfStockItems">缺货，无法下单</template>
                <template v-else-if="hasInvalidQuantity">库存不足，请调整数量</template>
                <template v-else>立即下单</template>
              </button>
            </div>
            
            <div class="checkout-tips">
              <div class="tip-item">
                <i class="fas fa-truck"></i>
                <span>满88元免运费</span>
              </div>
              <div class="tip-item">
                <i class="fas fa-gift"></i>
                <span>满200元减20元</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </PageTransition>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useCartStore } from '@/stores/cart'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import PageTransition from '@/components/PageTransition.vue'
import { notifySuccess, notifyError, notifyInfo } from '@/utils/notify'
import { couponService } from '@/services/coupon'

const router = useRouter()
const cartStore = useCartStore()

// 页面加载状态
const isLoading = ref(true)

// 页面横幅提示
const bannerMessage = ref('')
const bannerType = ref('info')
const setBanner = (msg, type = 'info') => {
  bannerMessage.value = msg
  bannerType.value = type
  setTimeout(() => { bannerMessage.value = '' }, 2500)
}

// 计算属性
const totalItems = computed(() => cartStore.cartItems.reduce((total, item) => total + item.quantity, 0))
const totalPrice = computed(() => cartStore.cartItems.reduce((total, item) => total + (item.price * item.quantity), 0))
const shippingFee = computed(() => totalPrice.value >= 88 ? 0 : 10)
const discount = computed(() => totalPrice.value >= 200 ? 20 : 0)
const finalPrice = computed(() => totalPrice.value + shippingFee.value - discount.value)
const savingsAmount = computed(() => shippingFee.value + discount.value)
const hasOutOfStockItems = computed(() => cartStore.cartItems.some(item => item.stock === 0))
const hasInvalidQuantity = computed(() => cartStore.cartItems.some(item => item.stock > 0 && item.quantity > item.stock))

const couponCode = ref('')
const couponDiscount = ref(0)
const appliedCoupon = ref(null)
const applyingCoupon = ref(false)
const couponMessage = ref('')
const myCoupons = ref([])

// 页面加载时获取购物车数据
onMounted(async () => {
  // 设置加载超时
  setTimeout(() => {
    if (isLoading.value) {
      isLoading.value = false
    }
  }, 3000)
  
  try {
    await cartStore.fetchCart()
  } catch (error) {
    console.error('Failed to load cart data:', error)
  } finally {
    // 确保加载状态被隐藏
    setTimeout(() => {
      isLoading.value = false
    }, 500)
  }

  try {
    const token = localStorage.getItem('token')
    if (token) {
      const res = await couponService.myCoupons()
      const list = res?.data || res || []
      myCoupons.value = Array.isArray(list) ? list : []
    }
  } catch {}
})

// 更新商品数量
const updateQuantity = async (itemId, newQuantity) => {
  if (newQuantity < 1) return
  
  // 获取当前商品信息以检查库存
  const currentItem = cartStore.cartItems.find(item => item.id === itemId)
  if (currentItem && currentItem.stock > 0 && newQuantity > currentItem.stock) {
    notifyError(`库存不足。当前库存：${currentItem.stock}`)
    return
  }
  
  // 如果商品已缺货，不允许增加数量
  if (currentItem && currentItem.stock === 0 && newQuantity > currentItem.quantity) {
    notifyError('该商品已缺货，无法增加数量')
    return
  }
  
  const result = await cartStore.updateQuantity(itemId, newQuantity)
  if (!result.success) {
    notifyError('更新数量失败')
  } else {
    setBanner('数量已更新', 'success')
  }
}

// 删除商品
const removeItem = async (itemId) => {
  if (confirm('确定要删除这个商品吗？')) {
    const result = await cartStore.removeFromCart(itemId)
    if (!result.success) {
      notifyError('删除失败')
    } else {
      setBanner('已移除商品', 'success')
    }
  }
}

// 清空购物车
const clearCart = async () => {
  if (confirm('确定要清空购物车吗？')) {
    const result = await cartStore.clearCart()
    if (!result.success) {
      notifyError('清空购物车失败')
    } else {
      setBanner('购物车已清空', 'success')
    }
  }
}

// 立即下单
const proceedToCheckout = async () => {
  if (cartStore.isEmpty || cartStore.loading) return
  
  // 检查购物车中的库存状态
  if (hasOutOfStockItems.value) {
    notifyError('购物车中包含已缺货商品，请移除后再下单')
    return
  }
  
  if (hasInvalidQuantity.value) {
    notifyError('部分商品数量超过库存，请调整后再下单')
    return
  }
  
  try {
    const { default: orderService } = await import('@/services/orderService')
    const token = localStorage.getItem('token')
    if (!token) {
      notifyInfo('请先登录再下单')
      router.push('/auth')
      return
    }
    
    // 获取用户地址信息
    let userAddress = null
    try {
      const { default: userService } = await import('@/services/userService')
      const addressResponse = await userService.getAddresses()
      if (addressResponse.success && addressResponse.data && addressResponse.data.length > 0) {
        // 使用默认地址或第一个地址
        userAddress = addressResponse.data.find(addr => addr.isDefault) || addressResponse.data[0]
      }
    } catch (addressError) {
      console.warn('获取用户地址失败，使用默认地址:', addressError)
    }
    
    // 构建订单数据，确保订单提交地址与个人信息地址区分存储
    const orderData = {
      RecipientName: (userAddress?.name) || '用户',
      Phone: (userAddress?.phone) || '13800138000',
      ShippingAddress: userAddress ? `${userAddress.province || ''}${userAddress.city || ''}${userAddress.district || ''}${userAddress.detailAddress || ''}` : '默认收货地址',
      PaymentMethod: 'online'
    }
    
    console.log('创建订单数据:', orderData)
    
    // 创建订单
    const result = await orderService.createOrder(orderData)
    
    console.log('订单创建结果:', result)
    const order = result?.data || result
    const orderId = order?.Id ?? order?.id
    const orderNumber = order?.OrderNumber ?? order?.orderNumber ?? orderId
    
    if (orderId) {
      notifySuccess('订单创建成功，订单号：' + orderNumber)
      setBanner('订单创建成功', 'success')
      await cartStore.clearCart()
      router.push(`/orders/${orderId}`)
    } else {
      notifyError(result?.message || '订单创建失败')
      setBanner('订单创建失败', 'error')
    }
  } catch (error) {
    console.error('创建订单失败:', error)
    notifyError('订单创建失败：' + (error.response?.data?.message || error.message || '请重试'))
    setBanner('订单创建失败', 'error')
  }
}

const applyCoupon = async () => {
  if (!couponCode.value) return
  applyingCoupon.value = true
  try {
    const amount = cartStore.cartTotal
    const res = await couponService.validate(couponCode.value, amount)
    const data = res?.data || res || {}
    const d = Number(data.discount || 0)
    if (d > 0) {
      couponDiscount.value = d
      appliedCoupon.value = couponCode.value
      couponMessage.value = `已应用，减少 ¥${d.toFixed(2)}`
      setBanner('优惠券已应用', 'success')
    } else {
      couponDiscount.value = 0
      appliedCoupon.value = null
      couponMessage.value = '未达到使用门槛或优惠无效'
    }
  } catch (e) {
    couponDiscount.value = 0
    appliedCoupon.value = null
    couponMessage.value = e?.response?.data?.message || '优惠券验证失败'
  } finally {
    applyingCoupon.value = false
  }
}

const clearCoupon = () => {
  couponDiscount.value = 0
  appliedCoupon.value = null
  couponMessage.value = ''
}

const useMyCoupon = (c) => {
  couponCode.value = c.Code || c.code
  applyCoupon()
}

// 图片错误处理
const handleImageError = (event) => {
  event.target.src = '/placeholder-flower.jpg'
}
</script>

<style scoped>
.cart-page {
  padding: 2rem 0;
  min-height: 60vh;
}

.banner {
  background: #f1f5f9;
  color: #334155;
}
.banner.success { background: #ecfdf5; color: #065f46; }
.banner.error { background: #fef2f2; color: #991b1b; }
.banner.info { background: #eff6ff; color: #1e40af; }

.page-header {
  text-align: center;
  margin-bottom: 2rem;
}

.page-header h1 {
  color: #2c3e50;
  font-size: 2.5rem;
  margin-bottom: 0.5rem;
}

.error-message {
  background-color: #fee;
  color: #c33;
  padding: 1rem;
  border-radius: 8px;
  margin-bottom: 1rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #c33;
}

.empty-cart {
  text-align: center;
  padding: 3rem;
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.empty-cart h3 {
  color: #2c3e50;
  margin-bottom: 1rem;
}

.empty-cart p {
  color: #666;
  margin-bottom: 2rem;
}

.cart-content {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 2rem;
}

/* 响应式设计优化 */
@media (max-width: 768px) {
  .cart-content {
    grid-template-columns: 1fr;
  }
  
  .cart-item {
    grid-template-columns: 80px 1fr;
    gap: 1rem;
    padding: 1rem;
  }
  
  .item-image img {
    height: 80px;
  }
  
  .item-header {
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .item-header h3 {
    font-size: 1rem;
  }
  
  .item-footer {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
  
  .quantity-control {
    justify-content: center;
  }
  
  .item-price-info {
    text-align: center;
  }
  
  .summary-actions {
    flex-direction: column;
  }
  
  .checkout-tips {
    flex-direction: column;
    gap: 0.5rem;
  }
  
  .tip-item {
    justify-content: center;
  }
}

.cart-items {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  overflow: hidden;
}

.loading {
  text-align: center;
  padding: 3rem;
}

.spinner {
  border: 3px solid #f3f3f3;
  border-top: 3px solid #e91e63;
  border-radius: 50%;
  width: 40px;
  height: 40px;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 购物车商品项样式优化 */
.cart-item {
  display: grid;
  grid-template-columns: 120px 1fr;
  gap: 1.5rem;
  padding: 1.5rem;
  border: 1px solid #f0f0f0;
  border-radius: 12px;
  background: white;
  transition: all 0.3s ease;
  position: relative;
}

.cart-item.out-of-stock {
  opacity: 0.7;
  background: #fdf2f2;
  border-color: #fecaca;
}

.cart-item.out-of-stock::before {
  content: '已售罄';
  position: absolute;
  top: 50%;
  right: 20px;
  transform: translateY(-50%) rotate(30deg);
  background: #ef4444;
  color: white;
  padding: 0.5rem 4rem;
  border-radius: 4px;
  font-weight: 600;
  z-index: 1;
  opacity: 0.9;
}

.cart-item:hover {
  border-color: #27ae60;
  box-shadow: 0 4px 12px rgba(39, 174, 96, 0.1);
  transform: translateY(-2px);
}

.item-image {
  position: relative;
  border-radius: 8px;
  overflow: hidden;
  background: #f9f9f9;
}

.item-image img {
  width: 100%;
  height: 120px;
  object-fit: cover;
  transition: transform 0.3s ease;
}

.item-image:hover img {
  transform: scale(1.05);
}

.item-badges {
  position: absolute;
  top: 8px;
  left: 8px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.badge {
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 500;
  color: white;
  backdrop-filter: blur(10px);
}

.badge.hot {
  background: linear-gradient(135deg, #e74c3c, #c0392b);
}

.badge.new {
  background: linear-gradient(135deg, #f39c12, #e67e22);
}

.item-details {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.item-header h3 {
  margin: 0;
  font-size: 1.125rem;
  color: #333;
  font-weight: 600;
  line-height: 1.4;
  flex: 1;
}

.remove-btn {
  background: none;
  border: none;
  color: #e74c3c;
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 50%;
  transition: all 0.2s ease;
  font-size: 0.875rem;
}

.remove-btn:hover {
  background: #fee;
  transform: scale(1.1);
}

.item-meta {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.stock-info, .sales-info {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.875rem;
  color: #666;
}

.stock-info.low-stock {
  color: #e74c3c;
  font-weight: 500;
}

.stock-info.out-of-stock {
  color: #ff6b6b;
  font-weight: 600;
}

.urgent-tip {
  font-size: 0.75rem;
  color: #ff6b6b;
  font-weight: 600;
}

.invalid-quantity {
  color: #e74c3c;
  font-weight: 700;
  animation: pulse 1s infinite;
}

@keyframes pulse {
  0% { opacity: 1; }
  50% { opacity: 0.6; }
  100% { opacity: 1; }
}

.stock-warning {
  font-size: 0.75rem;
  color: #e74c3c;
  margin-top: 0.5rem;
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.stock-info i, .sales-info i {
  font-size: 0.75rem;
}

.item-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
}

.quantity-control {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: #f8f9fa;
  border-radius: 8px;
  padding: 0.25rem;
}

.quantity-btn {
  width: 32px;
  height: 32px;
  border: none;
  background: white;
  color: #333;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  font-size: 0.75rem;
}

.quantity-btn:hover:not(:disabled) {
  background: #27ae60;
  color: white;
  transform: scale(1.05);
}

.quantity-btn:disabled {
  background: #e9ecef;
  color: #adb5bd;
  cursor: not-allowed;
}

.quantity-display {
  min-width: 40px;
  text-align: center;
  font-weight: 600;
  color: #333;
}

.item-price-info {
  text-align: right;
}

.price-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  margin-bottom: 0.25rem;
}

.price-row:last-child {
  margin-bottom: 0;
}

.price-label {
  font-size: 0.875rem;
  color: #666;
}

.unit-price {
  font-size: 0.875rem;
  color: #666;
}

.item-total {
  font-size: 1.125rem;
  font-weight: 700;
  color: #e74c3c;
}

.price-row.total .item-total {
  font-size: 1.25rem;
}

/* 订单总结样式优化 */
.cart-summary {
  background: white;
  border-radius: 16px;
  padding: 2rem;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
  border: 1px solid #f0f0f0;
}

.summary-title {
  margin: 0 0 1.5rem 0;
  font-size: 1.25rem;
  color: #333;
  font-weight: 600;
  text-align: center;
  padding-bottom: 1rem;
  border-bottom: 2px solid #f0f0f0;
}

.summary-content {
  margin-bottom: 1.5rem;
}

.summary-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem 0;
  transition: all 0.2s ease;
}

.summary-row:hover {
  background: #f8f9fa;
  margin: 0 -0.5rem;
  padding: 0.75rem 0.5rem;
  border-radius: 8px;
}

.summary-label {
  font-size: 0.95rem;
  color: #666;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.summary-value {
  font-weight: 600;
  color: #333;
  font-size: 0.95rem;
}

.summary-row.shipping .shipping-fee {
  color: #27ae60;
  font-weight: 700;
}

.summary-row.discount {
  color: #e74c3c;
}

.discount-amount {
  color: #e74c3c !important;
  font-weight: 700;
}

.divider {
  height: 1px;
  background: linear-gradient(to right, transparent, #e0e0e0, transparent);
  margin: 1rem 0;
}

.summary-row.total {
  padding: 1rem 0;
  border-top: 2px solid #f0f0f0;
  border-bottom: 2px solid #f0f0f0;
  margin: 1rem 0;
}

.summary-row.total .summary-label {
  font-size: 1.125rem;
  font-weight: 600;
  color: #333;
}

.total-price {
  font-size: 1.5rem;
  font-weight: 700;
  color: #e74c3c;
}

.savings-info {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.75rem;
  background: linear-gradient(135deg, #d4edda, #c3e6cb);
  border-radius: 8px;
  color: #155724;
  font-weight: 500;
  margin-top: 1rem;
}

.savings-info i {
  color: #27ae60;
}

.summary-actions {
  display: flex;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.checkout-tips {
  display: flex;
  justify-content: space-around;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 8px;
  border: 1px solid #e9ecef;
}

.tip-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: #666;
}

.tip-item i {
  color: #27ae60;
  font-size: 1rem;
}

.btn {
  padding: 0.75rem 1rem;
  border: none;
  border-radius: 6px;
  text-decoration: none;
  text-align: center;
  cursor: pointer;
  font-size: 1rem;
  transition: all 0.3s ease;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-outline {
  background: white;
  color: #e91e63;
  border: 2px solid #e91e63;
}

.btn-outline:hover:not(:disabled) {
  background: #e91e63;
  color: white;
}

.btn-primary {
  background: #e91e63;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #d81b60;
}

.btn.disabled {
  opacity: 0.6;
  cursor: not-allowed;
  pointer-events: none;
}
</style>
