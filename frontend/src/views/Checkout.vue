<template>
  <div class="min-h-screen bg-gray-50">
    <!-- 页面标题 -->
    <div class="bg-white shadow-sm border-b">
      <div class="container mx-auto px-4 py-4">
        <h1 class="text-2xl font-bold text-gray-800">结算</h1>
      </div>
    </div>

    <div class="container mx-auto px-4 py-8">
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- 结算表单 -->
        <div class="lg:col-span-2 space-y-8">
          <!-- 地址选择 -->
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-lg font-semibold mb-4 flex items-center">
              <i class="fas fa-map-marker-alt text-pink-500 mr-2"></i>
              收货地址
            </h2>
            
            <div v-if="isLoadingAddresses" class="p-4 bg-gray-50 rounded-lg text-gray-500 flex justify-center items-center">
              <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-gray-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              正在加载最新收货地址...
            </div>
            <div v-else-if="addresses.length > 0" class="space-y-4">
              <div 
                v-for="address in addresses" 
                :key="address.id" 
                class="border rounded-lg p-4 cursor-pointer transition-all"
                :class="{ 'border-pink-500 bg-pink-50': selectedAddress.id === address.id }"
                @click="selectAddress(address)"
              >
                <div class="flex items-start justify-between">
                  <div class="flex-1">
                    <div class="flex items-center">
                      <span class="font-medium">{{ address.name }}</span>
                      <span class="mx-2 text-gray-400">|</span>
                      <span>{{ address.phone }}</span>
                      <span v-if="address.isDefault" class="ml-2 text-xs text-white bg-pink-500 px-1.5 py-0.5 rounded">默认</span>
                    </div>
                    <div class="mt-1 text-gray-600">
                      {{ address.province }} {{ address.city }} {{ address.district }} {{ address.detailAddress }}
                    </div>
                  </div>
                  <div class="ml-4 flex items-center space-x-2">
                    <i v-if="selectedAddress.id === address.id" class="fas fa-check-circle text-pink-500"></i>
                    <button 
                      v-if="address.id && address.id > 0 && !address.isDefault" 
                      @click.stop="setDefault(address.id)" 
                      class="text-sm px-2 py-1 border rounded hover:bg-gray-50"
                    >设为默认</button>
                    <button 
                      v-if="address.id && address.id > 0" 
                      @click.stop="deleteAddress(address.id)" 
                      class="text-sm px-2 py-1 border rounded hover:bg-gray-50 text-red-600 border-red-300"
                    >删除</button>
                  </div>
                </div>
              </div>
            </div>
            
            <div v-else class="text-center py-4 text-gray-500">
              暂无收货地址，请添加地址
            </div>
            
            <button @click="showAddAddress = true" class="mt-4 px-4 py-2 border border-pink-500 text-pink-500 rounded hover:bg-pink-50 transition-colors">
              <i class="fas fa-plus mr-1"></i> 添加新地址
            </button>
          </div>

          <!-- 配送方式 -->
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-lg font-semibold mb-4 flex items-center">
              <i class="fas fa-truck text-pink-500 mr-2"></i>
              配送方式
            </h2>
            
            <div class="space-y-4">
              <div 
                class="border rounded-lg p-4 cursor-pointer transition-all"
                :class="{ 'border-pink-500 bg-pink-50': selectedShippingMethod.type === 'standard' }"
                @click="selectShippingMethod('standard')"
              >
                <div class="flex items-center justify-between">
                  <div>
                    <div class="font-medium">标准配送</div>
                    <div class="text-sm text-gray-500">预计1-3个工作日送达</div>
                  </div>
                  <div>
                    <span v-if="cartTotal >= 299" class="text-pink-500">免费</span>
                    <span v-else class="text-gray-600">¥20.00</span>
                  </div>
                  <div v-if="selectedShippingMethod.type === 'standard'" class="ml-4 text-pink-500">
                    <i class="fas fa-check-circle"></i>
                  </div>
                </div>
              </div>
              
              <div 
                class="border rounded-lg p-4 cursor-pointer transition-all"
                :class="{ 'border-pink-500 bg-pink-50': selectedShippingMethod.type === 'express' }"
                @click="selectShippingMethod('express')"
              >
                <div class="flex items-center justify-between">
                  <div>
                    <div class="font-medium">定时配送</div>
                    <div class="text-sm text-gray-500">当日或指定日期时间送达</div>
                  </div>
                  <div class="text-gray-600">¥35.00</div>
                  <div v-if="selectedShippingMethod.type === 'express'" class="ml-4 text-pink-500">
                    <i class="fas fa-check-circle"></i>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="mt-4">
              <label class="block text-sm font-medium text-gray-700 mb-1">配送时间</label>
              <select v-model="selectedShippingMethod.deliveryTime" class="w-full border rounded-md p-2">
                <option value="any">任意时间</option>
                <option value="morning">上午 (10:00-12:00)</option>
                <option value="afternoon">下午 (14:00-17:00)</option>
                <option value="evening">晚上 (18:00-20:00)</option>
              </select>
            </div>
          </div>

          <!-- 支付方式 -->
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-lg font-semibold mb-4 flex items-center">
              <i class="fas fa-credit-card text-pink-500 mr-2"></i>
              支付方式
            </h2>
            
            <div class="space-y-4">
              <div 
                class="border rounded-lg p-4 cursor-pointer transition-all flex items-center"
                :class="{ 'border-pink-500 bg-pink-50': selectedPaymentMethod === 'online' }"
                @click="selectedPaymentMethod = 'online'"
              >
                <i class="fas fa-credit-card text-blue-500 text-xl mr-3"></i>
                <span class="flex-1">在线支付</span>
                <div v-if="selectedPaymentMethod === 'online'" class="text-pink-500">
                  <i class="fas fa-check-circle"></i>
                </div>
              </div>
              
              <div 
                class="border rounded-lg p-4 cursor-pointer transition-all flex items-center"
                :class="{ 'border-pink-500 bg-pink-50': selectedPaymentMethod === 'cod' }"
                @click="selectedPaymentMethod = 'cod'"
              >
                <i class="fas fa-money-bill-wave text-green-500 text-xl mr-3"></i>
                <span class="flex-1">货到付款</span>
                <div v-if="selectedPaymentMethod === 'cod'" class="text-pink-500">
                  <i class="fas fa-check-circle"></i>
                </div>
              </div>
            </div>
          </div>

          <!-- 订单备注 -->
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-lg font-semibold mb-4 flex items-center">
              <i class="fas fa-comment-alt text-pink-500 mr-2"></i>
              订单备注
            </h2>
            <textarea 
              v-model="orderNotes" 
              class="w-full border rounded-md p-2 h-24 resize-none"
              placeholder="选填，请输入订单备注信息（如卡片留言等）"
            ></textarea>
          </div>
        </div>

        <!-- 订单汇总 -->
        <div class="lg:col-span-1">
  <div class="bg-white rounded-lg shadow p-6 sticky top-8">
            <h2 class="text-lg font-semibold mb-4">订单汇总</h2>
            
            <div class="space-y-3">
              <div v-for="item in cartItems" :key="item.id" class="flex justify-between text-sm">
                <span class="text-gray-600">{{ item.name }}</span>
                <span>¥{{ (item.price * item.quantity).toFixed(2) }}</span>
              </div>
              
              <div class="border-t pt-3 mt-4">
                <div class="flex justify-between">
                  <span class="text-gray-600">商品小计</span>
                  <span>¥{{ cartSubtotal.toFixed(2) }}</span>
                </div>
                <div class="mt-3">
                  <label class="block text-sm text-gray-700 mb-1">优惠券</label>
                  <div class="flex items-center space-x-2">
                    <select v-model="selectedCouponCode" @change="previewCoupon" class="border rounded-md p-2 flex-1">
                      <option value="">不使用优惠券</option>
                      <option v-for="c in availableCoupons" :key="c.Id || c.id" :value="c.Code || c.code">
                        {{ (c.Code || c.code) + '（' + ((c.DiscountType || c.discountType) === 'percent' ? (c.Value || c.value) + '%': ('¥' + (c.Value || c.value))) + '）' }}
                      </option>
                    </select>
                    <input v-model="manualCouponCode" placeholder="输入优惠码" class="border rounded-md p-2 flex-1" />
                    <button @click="previewCoupon(true)" class="px-3 py-2 border rounded hover:bg-gray-50">验证</button>
                  </div>
                  <p v-if="discount > 0" class="text-xs text-gray-500 mt-1">已应用优惠：-¥{{ discount.toFixed(2) }}</p>
                </div>
                <div class="flex justify-between mt-2">
                  <span class="text-gray-600">配送费</span>
                  <span>¥{{ shippingFee.toFixed(2) }}</span>
                </div>
                <div v-if="discount > 0" class="flex justify-between mt-2">
                  <span class="text-gray-600">优惠券</span>
                  <span class="text-red-500">-¥{{ discount.toFixed(2) }}</span>
                </div>
                <div class="border-t pt-3 mt-4">
                  <div class="flex justify-between font-semibold text-lg">
                    <span>总计</span>
                    <span class="text-huanyu-pink-600">¥{{ orderTotal.toFixed(2) }}</span>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="mt-6">
              <button 
                @click="submitOrder" 
                class="w-full btn-primary" 
                :disabled="!selectedAddress.id || submitting"
              >
                {{ submitting ? '提交中...' : '提交订单' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 支付模态框 -->
    <el-dialog
      v-model="showPaymentModal"
      title="支付订单"
      width="520px"
      :show-close="false"
      :close-on-click-modal="false"
    >
      <div class="py-8">
        <div class="text-center mb-6">
          <h3 class="text-xl font-semibold mb-2">请选择支付方式</h3>
          <p class="text-gray-600">订单金额：<span class="text-pink-600 font-semibold text-lg">¥{{ orderTotal.toFixed(2) }}</span></p>
        </div>
        
        <div class="grid grid-cols-3 gap-4 px-4">
          <div 
            class="border rounded-xl p-4 cursor-pointer transition-all flex flex-col items-center hover:shadow-md"
            :class="{ 'border-pink-500 bg-pink-50': paymentModalMethod === 'alipay' }"
            @click="paymentModalMethod = 'alipay'"
          >
            <svg viewBox="0 0 48 48" class="w-12 h-12 mb-2">
              <rect x="4" y="8" width="40" height="32" rx="6" fill="#1677ff"/>
              <path d="M18 22h12M16 28c6 4 18 4 24 0" stroke="white" stroke-width="2" stroke-linecap="round"/>
              <text x="24" y="20" text-anchor="middle" font-size="10" fill="white">ALIPAY</text>
            </svg>
            <span class="font-medium">支付宝</span>
          </div>
          <div 
            class="border rounded-xl p-4 cursor-pointer transition-all flex flex-col items-center hover:shadow-md"
            :class="{ 'border-pink-500 bg-pink-50': paymentModalMethod === 'wechat' }"
            @click="paymentModalMethod = 'wechat'"
          >
            <svg viewBox="0 0 48 48" class="w-12 h-12 mb-2">
              <circle cx="20" cy="20" r="12" fill="#09bb07"/>
              <circle cx="30" cy="28" r="12" fill="#09bb07"/>
              <circle cx="16" cy="18" r="2" fill="white"/>
              <circle cx="22" cy="18" r="2" fill="white"/>
            </svg>
            <span class="font-medium">微信支付</span>
          </div>
          <div 
            class="border rounded-xl p-4 cursor-pointer transition-all flex flex-col items-center hover:shadow-md"
            :class="{ 'border-pink-500 bg-pink-50': paymentModalMethod === 'cod' }"
            @click="paymentModalMethod = 'cod'"
          >
            <svg viewBox="0 0 48 48" class="w-12 h-12 mb-2" fill="none" stroke="#10b981">
              <rect x="6" y="12" width="36" height="24" rx="6" stroke-width="2"/>
              <path d="M12 24h12m0 0h12" stroke-width="2" stroke-linecap="round"/>
            </svg>
            <span class="font-medium">货到付款</span>
          </div>
        </div>
        
        <div class="text-center mt-6 text-gray-700">
          <span v-if="paymentModalMethod === 'alipay'">确认使用支付宝支付？</span>
          <span v-else-if="paymentModalMethod === 'wechat'">确认使用微信支付？</span>
          <span v-else>确认选择货到付款？</span>
        </div>
      </div>
      
      <template #footer>
        <div class="flex justify-center space-x-4">
          <button 
            @click="cancelPayment" 
            class="px-4 py-2 border border-gray-300 rounded hover:bg-gray-100"
          >
            取消
          </button>
          <button 
            @click="confirmPayment" 
            class="px-4 py-2 bg-pink-500 text-white rounded hover:bg-pink-600"
            :disabled="processingPayment"
          >
            {{ processingPayment ? '处理中...' : '确认支付' }}
          </button>
        </div>
      </template>
    </el-dialog>

    <!-- 支付成功提示 -->
    <el-message-box
      v-model="showPaymentSuccess"
      title="支付成功"
      :show-close="false"
      :close-on-click-modal="false"
      :close-on-press-escape="false"
    >
      <div class="text-center py-4">
        <i class="fas fa-check-circle text-5xl text-green-500 mb-4"></i>
        <p class="text-lg font-semibold mb-2">支付成功！</p>
        <p class="text-gray-600">您的订单已支付完成，我们将尽快为您发货</p>
      </div>
      
      <template #footer>
        <div class="flex justify-center">
          <button 
            @click="viewOrderDetail"
            class="px-6 py-2 bg-pink-500 text-white rounded hover:bg-pink-600"
          >
            查看订单详情
          </button>
        </div>
      </template>
    </el-message-box>

    <!-- 添加地址模态框 -->
    <div v-if="showAddAddress" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-8 max-w-md w-full mx-4">
        <div class="flex justify-between items-center mb-6">
          <h3 class="text-xl font-semibold">添加收货地址</h3>
          <button @click="showAddAddress = false" class="text-gray-400 hover:text-gray-600">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
            </svg>
          </button>
        </div>
        <form @submit.prevent="addAddress" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">收货人</label>
            <input v-model="newAddress.name" type="text" required class="w-full border rounded-lg px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">手机号</label>
            <input v-model="newAddress.phone" type="tel" required class="w-full border rounded-lg px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent" />
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">所在地区</label>
            <div class="grid grid-cols-3 gap-2">
              <select v-model="newAddress.province" @change="handleProvinceChange(newAddress.province)" required class="w-full border rounded-lg px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent">
                <option value="">选择省</option>
                <option v-for="p in provinces" :key="p.code" :value="p.code">{{ p.name }}</option>
              </select>
              <select v-model="newAddress.city" @change="handleCityChange(newAddress.city)" required class="w-full border rounded-lg px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent">
                <option value="">选择市</option>
                <option v-for="c in cities" :key="c.code" :value="c.code">{{ c.name }}</option>
              </select>
              <select v-model="newAddress.district" required class="w-full border rounded-lg px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent">
                <option value="">选择区/县</option>
                <option v-for="d in districts" :key="d.code" :value="d.code">{{ d.name }}</option>
              </select>
            </div>
          </div>
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-2">详细地址</label>
            <textarea v-model="newAddress.detailAddress" rows="2" required class="w-full border rounded-lg px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"></textarea>
          </div>
          <div class="flex items-center">
            <input v-model="newAddress.isDefault" type="checkbox" id="ck-default" class="mr-2" />
            <label for="ck-default" class="text-sm">设为默认地址</label>
          </div>
          <div class="flex justify-end space-x-3">
            <button type="button" @click="showAddAddress = false" class="px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors">取消</button>
            <button type="submit" class="px-4 py-2 bg-huanyu-pink-600 text-white rounded-lg hover:bg-huanyu-pink-700 transition-colors">保存</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import orderService from '@/services/orderService'
import { couponService } from '@/services/coupon'

import userService from '@/services/userService'
import { notifySuccess, notifyError, notifyInfo } from '@/utils/notify'
import { getProvinces, getCities, getDistricts, getProvinceName, getCityName, getDistrictName } from '@/utils/regionData'

const router = useRouter()

// 状态变量
const submitting = ref(false)
const showPaymentModal = ref(false)
const showPaymentSuccess = ref(false)
const processingPayment = ref(false)
const currentOrderId = ref(null)

// 表单数据
const orderNotes = ref('')
const selectedPaymentMethod = ref('online')
const paymentModalMethod = ref('alipay')

// 地址数据
const addresses = ref([])
const selectedAddress = ref({})
const selectedAddressId = ref(null)
const showAddAddress = ref(false)
const newAddress = ref({
  name: '',
  phone: '',
  province: '',
  city: '',
  district: '',
  detailAddress: '',
  isDefault: true
})
const isLoadingAddresses = ref(false)
const provinces = ref([])
const cities = ref([])
const districts = ref([])

// 配送方式
const selectedShippingMethod = ref({
  type: 'standard',
  deliveryTime: 'any'
})

// 模拟购物车数据
const cartItems = ref([
  {
    id: 1,
    name: '红玫瑰束',
    description: '11枝装，含包装',
    price: 299,
    quantity: 1,
    image: 'https://images.unsplash.com/photo-1536244935254-e0e2b0ab4a3c?w=200&h=200&auto=format&fit=crop'
  },
  {
    id: 2,
    name: '粉康乃馨',
    description: '新鲜粉康乃馨',
    price: 199,
    quantity: 2,
    image: 'https://images.unsplash.com/photo-1561181347-ce5a53a7db5d?w=200&h=200&auto=format&fit=crop'
  }
])

// 计算属性
const cartSubtotal = computed(() => {
  return cartItems.value.reduce((sum, item) => sum + (item.price * item.quantity), 0)
})

const shippingFee = computed(() => {
  if (selectedShippingMethod.value.type === 'express') {
    return 35
  }
  return cartSubtotal.value >= 299 ? 0 : 20
})

const availableCoupons = ref([])
const selectedCouponCode = ref('')
const manualCouponCode = ref('')
const discountValue = ref(0)
const discount = computed(() => discountValue.value || 0)

const orderTotal = computed(() => {
  return cartSubtotal.value + shippingFee.value - discount.value
})

const cartTotal = computed(() => {
  return cartSubtotal.value
})

// 方法
const selectAddress = (address) => {
  console.log('选择地址:', address)
  selectedAddress.value = address
  if (address) {
    selectedAddressId.value = address.id || address.Id
  }
  console.log('已选择地址ID:', selectedAddressId.value)
}

const selectShippingMethod = (type) => {
  selectedShippingMethod.value.type = type
}

const submitOrder = async () => {
  if (!selectedAddress.value.id && !selectedAddress.value.detailAddress) {
    ElMessage.warning('请选择或添加收货地址')
    return
  }

  submitting.value = true
  try {
    console.log('准备提交订单，从数据库获取的地址信息:', selectedAddress.value)
    
    // 转换为后端 CreateOrderDto，确保字段名称与后端一致
    const orderData = {
      RecipientName: selectedAddress.value.name || selectedAddress.value.RecipientName || '用户',
      Phone: selectedAddress.value.phone || selectedAddress.value.PhoneNumber || '13800138000',
      ShippingAddress: selectedAddress.value.fullAddress || `${selectedAddress.value.province || ''}${selectedAddress.value.city || ''}${selectedAddress.value.district || ''}${selectedAddress.value.detailAddress || ''}`,
      PaymentMethod: selectedPaymentMethod.value === 'cod' ? 'cash_on_delivery' : 'online',
      CouponCode: (manualCouponCode?.value || selectedCouponCode?.value || '').trim() || null
    }

    console.log('提交订单数据:', orderData)
    
    const result = await orderService.createOrder(orderData)
    
    // 处理不同格式的响应
    const isSuccess = result.success || result.Success || false
    const order = result?.data || result?.Data || result
    currentOrderId.value = order?.Id ?? order?.id ?? null

    if (!currentOrderId.value) {
      const errorMsg = result.message || result.Message || '订单创建失败'
      ElMessage.error(errorMsg)
      return
    }

    showPaymentModal.value = true
    paymentModalMethod.value = selectedPaymentMethod.value === 'cod' ? 'cod' : 'alipay'
  } catch (error) {
    console.error('创建订单失败:', error)
    ElMessage.error('创建订单失败，请稍后重试')
  } finally {
    submitting.value = false
  }
}

const confirmPayment = async () => {
  if (!currentOrderId.value) {
    ElMessage.error('订单信息错误')
    return
  }

  processingPayment.value = true
  try {
    await new Promise(resolve => setTimeout(resolve, 1500))
    showPaymentModal.value = false
    showPaymentSuccess.value = true
    const paymentStatus = paymentModalMethod.value === 'cod' ? 'unpaid' : 'paid'
    await orderService.updateOrderStatus(currentOrderId.value, paymentStatus)
  } catch (error) {
    console.error('支付过程错误:', error)
    ElMessage.error('支付处理过程中出现错误')
  } finally {
    processingPayment.value = false
  }
}

const cancelPayment = () => {
  showPaymentModal.value = false
}

const viewOrderDetail = () => {
  showPaymentSuccess.value = false
  router.push(`/order/${currentOrderId.value}`)
}

// 组件挂载时的操作
onMounted(async () => {
  const { ensureRegionDataLoaded } = await import('@/utils/regionData')
  await ensureRegionDataLoaded()
  provinces.value = getProvinces()
  loadAddresses()
  try {
    const res = await couponService.getAvailable(cartSubtotal.value)
    availableCoupons.value = res?.data || res || []
  } catch {}
})

// 监听页面可见性变化，实现页面重新聚焦时自动刷新地址
onMounted(() => {
  document.addEventListener('visibilitychange', handleVisibilityChange)
})

onBeforeUnmount(() => {
  document.removeEventListener('visibilitychange', handleVisibilityChange)
})

const handleVisibilityChange = async () => {
  if (!document.hidden) {
    // 页面重新聚焦时刷新地址列表
    await loadAddresses()
  }
}

const loadAddresses = async () => {
  isLoadingAddresses.value = true
  try {
    console.log('开始从数据库加载最新收货地址...')
    const res = await userService.getAddresses()
    
    // 处理响应数据，确保格式统一
    const list = res?.data || res?.Data || []
    console.log('从数据库获取的地址列表:', list)
    
    // 格式化地址数据，确保字段名称一致性
    addresses.value = Array.isArray(list) ? list.map(addr => ({
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
    
    // 如果有地址，默认选择默认地址或第一个地址
    if (addresses.value.length > 0) {
      const defaultAddress = addresses.value.find(addr => addr.isDefault)
      if (defaultAddress) {
        selectAddress(defaultAddress)
      } else {
        selectAddress(addresses.value[0])
      }
    } else {
      // 不使用回退地址；当没有地址时，保持为空，提示用户添加
      selectedAddress.value = {}
    }
    
    console.log('地址加载完成，共加载', addresses.value.length, '个地址')
  } catch (error) {
    console.error('加载收货地址失败:', error)
    addresses.value = []
  } finally {
    isLoadingAddresses.value = false
  }
}

const handleProvinceChange = (code) => {
  cities.value = []
  districts.value = []
  if (code) cities.value = getCities(code)
}

const handleCityChange = (code) => {
  districts.value = []
  if (newAddress.value.province && code) districts.value = getDistricts(newAddress.value.province, code)
}

const addAddress = async () => {
  try {
    const payload = {
      RecipientName: newAddress.value.name,
      PhoneNumber: newAddress.value.phone,
      Province: getProvinceName(newAddress.value.province),
      City: getCityName(newAddress.value.province, newAddress.value.city),
      District: getDistrictName(newAddress.value.province, newAddress.value.city, newAddress.value.district),
      DetailAddress: newAddress.value.detailAddress,
      PostalCode: null,
      IsDefault: newAddress.value.isDefault
    }
    const res = await userService.addAddress(payload)
    if (res.success) {
      notifySuccess('地址添加成功')
      showAddAddress.value = false
      newAddress.value = { name: '', phone: '', province: '', city: '', district: '', detailAddress: '', isDefault: true }
      cities.value = []
      districts.value = []
      await loadAddresses()
    } else {
      notifyError(res.data?.message || '添加地址失败')
    }
  } catch (e) {
    notifyError('添加地址失败')
  }
}
</script>

<style scoped>
.btn-primary {
  background-color: #f06292;
  color: white;
  padding: 0.75rem 1.5rem;
  border-radius: 0.375rem;
  font-weight: 500;
  transition: background-color 0.2s;
  text-align: center;
  display: block;
  width: 100%;
}

.btn-primary:hover {
  background-color: #ec407a;
}

.btn-primary:disabled {
  background-color: #fecdd3;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: white;
  color: #f06292;
  padding: 0.75rem 1.5rem;
  border-radius: 0.375rem;
  font-weight: 500;
  border: 1px solid #f06292;
  transition: all 0.2s;
}

.btn-secondary:hover {
  background-color: #fce4ec;
}
</style>
const previewCoupon = async (manual = false) => {
  try {
    const code = manual ? (manualCouponCode.value || '').trim() : (selectedCouponCode.value || '').trim()
    if (!code) { discountValue.value = 0; return }
    const res = await couponService.validate(code, cartSubtotal.value)
    const data = res?.data || res
    const d = data?.discount ?? data
    discountValue.value = typeof d === 'number' ? d : 0
    if (!manual) manualCouponCode.value = ''
  } catch (e) {
    discountValue.value = 0
  }
}
