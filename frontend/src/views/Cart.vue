<template>
  <PageTransition>
    <div class="cart-page min-h-screen bg-gradient-to-br from-huanyu-pink-50 to-white relative overflow-hidden">
      <!-- 装饰背景球 -->
      <div class="absolute top-[-10%] left-[-10%] w-[500px] h-[500px] bg-huanyu-pink-200 rounded-full blur-[100px] opacity-30 animate-pulse-slow pointer-events-none"></div>
      <div class="absolute bottom-[-10%] right-[-10%] w-[600px] h-[600px] bg-purple-200 rounded-full blur-[120px] opacity-30 animate-pulse-slow pointer-events-none" style="animation-delay: 2s"></div>

      <!-- 页面加载动画 -->
      <LoadingSpinner v-if="isLoading" />
      
      <div class="container mx-auto px-4 py-12 relative z-10">
        <div class="page-header mb-8 text-center animate-fade-in-down">
          <h1 class="text-3xl font-bold text-gray-900">我的购物车</h1>
          <p class="text-gray-500 mt-2">共 {{ cartStore.cartCount }} 件商品</p>
        </div>

        <div v-if="bannerMessage" :class="['banner', bannerType]" class="mb-6 p-4 rounded-xl shadow-sm flex items-center justify-between animate-fade-in-up">
          <span class="flex items-center">
            <i class="fas fa-info-circle mr-2 text-lg"></i>
            {{ bannerMessage }}
          </span>
        </div>

        <!-- 错误提示 -->
        <div v-if="cartStore.error" class="bg-red-50 text-red-600 p-4 rounded-xl mb-6 flex justify-between items-center border border-red-100 animate-fade-in-up">
          <span class="flex items-center">
            <i class="fas fa-exclamation-circle mr-2"></i>
            {{ cartStore.error }}
          </span>
          <button @click="cartStore.clearError()" class="hover:bg-red-100 p-1 rounded-full transition-colors">
            <i class="fas fa-times"></i>
          </button>
        </div>

        <!-- 购物车为空 -->
        <div v-if="cartStore.isEmpty && !cartStore.loading" class="text-center py-20 bg-white/60 backdrop-blur-md rounded-3xl shadow-xl border border-white/50 animate-fade-in-up">
          <div class="text-6xl mb-6 animate-bounce-slow">🛒</div>
          <h3 class="text-2xl font-bold text-gray-800 mb-2">购物车是空的</h3>
          <p class="text-gray-500 mb-8">快去挑选心仪的花卉吧！</p>
          <router-link to="/products" class="inline-flex items-center px-8 py-3 bg-gradient-to-r from-huanyu-pink-500 to-huanyu-red-500 text-white rounded-full font-bold shadow-lg hover:shadow-huanyu-pink-500/30 transition-all transform hover:-translate-y-1">
            <i class="fas fa-shopping-bag mr-2"></i> 去购物
          </router-link>
        </div>

        <!-- 购物车商品列表 -->
        <div v-else class="grid grid-cols-1 lg:grid-cols-3 gap-8">
          <div class="lg:col-span-2 space-y-6 animate-fade-in-left">
            <div v-if="cartStore.loading" class="text-center py-12">
              <div class="spinner border-4 border-huanyu-pink-200 border-t-huanyu-pink-600 rounded-full w-10 h-10 animate-spin mx-auto mb-4"></div>
              <p class="text-gray-500">加载中...</p>
            </div>

            <div v-else class="space-y-4">
              <div v-for="item in cartStore.cartItems" :key="item.id" 
                class="group bg-white/60 backdrop-blur-sm rounded-2xl p-4 border border-white/50 shadow-sm hover:shadow-md transition-all duration-300"
              >
                <div class="flex gap-6">
                  <!-- 商品图片 -->
                  <div class="w-24 h-24 flex-shrink-0 rounded-xl overflow-hidden relative bg-gray-100">
                    <img :src="getProductImageUrl(item.productImage || '')" :alt="item.productName" @error="handleImageError" class="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500" />
                    <div class="absolute top-1 left-1 flex flex-col gap-1">
                      <span v-if="item.isHot" class="px-2 py-0.5 bg-red-500 text-white text-xs font-bold rounded-full shadow-sm">热卖</span>
                      <span v-if="item.isNew" class="px-2 py-0.5 bg-blue-500 text-white text-xs font-bold rounded-full shadow-sm">新品</span>
                    </div>
                  </div>

                  <!-- 商品详情 -->
                  <div class="flex-1 flex flex-col justify-between">
                    <div class="flex justify-between items-start">
                      <div>
                        <h3 class="font-bold text-gray-800 text-lg group-hover:text-huanyu-pink-600 transition-colors">{{ item.productName }}</h3>
                        <div class="flex items-center gap-3 mt-1 text-sm text-gray-500">
                          <span v-if="item.stock !== undefined" :class="{'text-red-500 font-bold': item.stock < 10, 'text-gray-400': item.stock === 0}">
                            <i class="fas fa-box mr-1"></i>
                            {{ item.stock > 0 ? `库存: ${item.stock}` : '已售罄' }}
                          </span>
                          <span v-if="item.salesCount" class="text-gray-400">
                            已售 {{ item.salesCount }}
                          </span>
                        </div>
                      </div>
                      <button @click="removeItem(item.id)" class="text-gray-400 hover:text-red-500 transition-colors p-2 hover:bg-red-50 rounded-full" title="移除商品">
                        <i class="fas fa-trash-alt"></i>
                      </button>
                    </div>

                    <div class="flex justify-between items-end mt-4">
                      <div class="flex flex-col">
                        <div class="flex items-center bg-white rounded-lg border border-gray-200 p-1 shadow-sm">
                          <button 
                            @click="updateQuantity(item.id, item.quantity - 1)"
                            :disabled="item.quantity <= 1 || item.stock === 0"
                            class="w-8 h-8 rounded bg-gray-50 hover:bg-gray-100 text-gray-600 flex items-center justify-center transition-colors disabled:opacity-50"
                          >
                            <i class="fas fa-minus text-xs"></i>
                          </button>
                          <span class="w-10 text-center font-bold text-gray-800">{{ item.quantity }}</span>
                          <button 
                            @click="updateQuantity(item.id, item.quantity + 1)"
                            :disabled="item.quantity >= item.stock || item.stock === 0"
                            class="w-8 h-8 rounded bg-gray-50 hover:bg-gray-100 text-gray-600 flex items-center justify-center transition-colors disabled:opacity-50"
                          >
                            <i class="fas fa-plus text-xs"></i>
                          </button>
                        </div>
                        <span v-if="item.quantity > item.stock && item.stock > 0" class="text-xs text-red-500 mt-1 font-medium">库存不足</span>
                      </div>
                      
                      <div class="text-right">
                        <div class="text-xs text-gray-500">单价: ¥{{ item.price.toFixed(2) }}</div>
                        <div class="text-xl font-bold text-huanyu-pink-600">¥{{ (item.price * item.quantity).toFixed(2) }}</div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 购物车总结 -->
          <div class="lg:col-span-1 animate-fade-in-right">
            <div class="bg-white/80 backdrop-blur-md rounded-3xl p-6 shadow-xl border border-white/50 sticky top-24">
              <h3 class="text-xl font-bold text-gray-800 mb-6 pb-4 border-b border-gray-100">订单总结</h3>
              
              <div class="space-y-4 mb-6">
                <div class="flex justify-between text-gray-600">
                  <span>商品数量</span>
                  <span class="font-medium">{{ cartStore.cartCount }} 件</span>
                </div>
                <div class="flex justify-between text-gray-600">
                  <span>商品总价</span>
                  <span class="font-medium">¥{{ cartStore.cartTotal.toFixed(2) }}</span>
                </div>
                <div class="flex justify-between text-gray-600">
                  <span>配送费用</span>
                  <span :class="cartStore.cartTotal >= 88 ? 'text-green-600 font-bold' : 'font-medium'">
                    {{ cartStore.cartTotal >= 88 ? '免运费' : '¥10.00' }}
                  </span>
                </div>
                <div v-if="cartStore.cartTotal >= 200" class="flex justify-between text-red-500">
                  <span class="flex items-center"><i class="fas fa-tag mr-1"></i> 满减优惠</span>
                  <span class="font-bold">-¥20.00</span>
                </div>
                
                <!-- 优惠券部分 -->
                <div class="pt-4 border-t border-gray-100">
                  <div class="flex gap-2 mb-2">
                    <input v-model="couponCode" placeholder="输入优惠券代码" class="flex-1 bg-gray-50 border border-gray-200 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-huanyu-pink-200 focus:outline-none transition-all" />
                    <button @click="applyCoupon" :disabled="applyingCoupon || !couponCode" class="px-3 py-2 bg-gray-800 text-white rounded-lg text-sm hover:bg-gray-700 transition-colors disabled:opacity-50">
                      {{ applyingCoupon ? '...' : '使用' }}
                    </button>
                  </div>
                  <div v-if="myCoupons.length" class="flex flex-wrap gap-2 mt-2">
                     <button v-for="c in myCoupons" :key="c.Id||c.id" @click="useMyCoupon(c)" class="px-2 py-1 bg-huanyu-pink-50 text-huanyu-pink-600 text-xs rounded border border-huanyu-pink-100 hover:bg-huanyu-pink-100 transition-colors">
                        {{ c.Code || c.code }}
                     </button>
                  </div>
                  <p v-if="couponMessage" :class="couponDiscount > 0 ? 'text-green-600' : 'text-red-500'" class="text-xs mt-1 font-medium flex items-center">
                    <i :class="couponDiscount > 0 ? 'fas fa-check-circle' : 'fas fa-times-circle'" class="mr-1"></i>
                    {{ couponMessage }}
                  </p>
                </div>

                <div v-if="couponDiscount > 0" class="flex justify-between text-red-500 pt-2">
                  <span class="flex items-center"><i class="fas fa-ticket-alt mr-1"></i> 优惠券折扣</span>
                  <span class="font-bold">-¥{{ couponDiscount.toFixed(2) }}</span>
                </div>
              </div>
              
              <div class="border-t border-gray-100 pt-6 mb-6">
                <div class="flex justify-between items-end mb-2">
                  <span class="text-gray-800 font-bold text-lg">应付总额</span>
                  <span class="text-3xl font-bold text-transparent bg-clip-text bg-gradient-to-r from-huanyu-pink-600 to-huanyu-red-500">
                    ¥{{ ( (cartStore.cartTotal >= 88 ? cartStore.cartTotal : cartStore.cartTotal + 10) - (cartStore.cartTotal >= 200 ? 20 : 0) - couponDiscount ).toFixed(2) }}
                  </span>
                </div>
                <div v-if="savingsAmount > 0" class="bg-green-50 text-green-700 text-sm py-2 px-3 rounded-lg flex items-center justify-center">
                  <i class="fas fa-piggy-bank mr-2"></i>
                  已为您节省 ¥{{ savingsAmount.toFixed(2) }}
                </div>
              </div>
              
              <div class="space-y-3">
                <button 
                  @click="proceedToCheckout" 
                  class="w-full py-4 bg-gradient-to-r from-huanyu-pink-500 to-huanyu-red-500 text-white rounded-xl font-bold shadow-lg hover:shadow-huanyu-pink-500/30 transition-all transform hover:-translate-y-0.5 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none flex items-center justify-center group"
                  :disabled="cartStore.isEmpty || cartStore.loading || hasOutOfStockItems || hasInvalidQuantity"
                >
                  <span>立即下单</span>
                  <i class="fas fa-arrow-right ml-2 group-hover:translate-x-1 transition-transform"></i>
                </button>
                
                <button 
                  @click="clearCart" 
                  class="w-full py-3 bg-white border border-gray-200 text-gray-500 rounded-xl hover:bg-gray-50 hover:text-red-500 transition-colors text-sm font-medium"
                  :disabled="cartStore.isEmpty || cartStore.loading"
                >
                  清空购物车
                </button>
              </div>

              <div class="mt-6 grid grid-cols-2 gap-3 text-xs text-gray-500 bg-gray-50 p-3 rounded-xl border border-gray-100">
                <div class="flex items-center justify-center gap-1">
                  <i class="fas fa-truck text-green-500"></i> 满88包邮
                </div>
                <div class="flex items-center justify-center gap-1">
                  <i class="fas fa-shield-alt text-blue-500"></i> 售后无忧
                </div>
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
import { getProductImageUrl } from '@/utils/avatar.js'

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
    // setBanner('数量已更新', 'success') // 减少干扰
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
  
  // 简单跳转，地址选择等逻辑在 Checkout 页面处理
  router.push('/checkout')
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
      // setBanner('优惠券已应用', 'success')
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
  event.target.src = '/images/product-placeholder.svg'
}
</script>

<style scoped>
.banner.info { @apply bg-blue-50 text-blue-600 border border-blue-100; }
.banner.success { @apply bg-green-50 text-green-600 border border-green-100; }
.banner.error { @apply bg-red-50 text-red-600 border border-red-100; }
</style>