<template>
  <div class="min-h-screen bg-gradient-to-br from-huanyu-pink-50 to-white relative overflow-hidden">
    <!-- 装饰背景球 -->
    <div class="absolute top-[-10%] left-[-10%] w-[500px] h-[500px] bg-huanyu-pink-200 rounded-full blur-[100px] opacity-30 animate-pulse-slow pointer-events-none"></div>
    <div class="absolute bottom-[-10%] right-[-10%] w-[600px] h-[600px] bg-purple-200 rounded-full blur-[120px] opacity-30 animate-pulse-slow pointer-events-none" style="animation-delay: 2s"></div>

    <div class="container mx-auto px-4 py-12 relative z-10">
      <div class="page-header mb-8 text-center animate-fade-in-down">
        <h1 class="text-3xl font-bold text-gray-900">填写订单信息</h1>
        <p class="text-gray-500 mt-2">请确认您的收货地址和配送方式</p>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- 左侧：结算表单 -->
        <div class="lg:col-span-2 space-y-6 animate-fade-in-left">
          
          <!-- 地址选择 -->
          <div class="bg-white/80 backdrop-blur-md rounded-3xl p-6 shadow-xl border border-white/50">
            <h2 class="text-xl font-bold text-gray-800 mb-6 flex items-center border-b border-gray-100 pb-4">
              <i class="fas fa-map-marker-alt text-huanyu-pink-500 mr-3 text-2xl"></i>
              收货地址
            </h2>
            
            <div v-if="isLoadingAddresses" class="p-8 text-center text-gray-500">
              <div class="spinner border-4 border-huanyu-pink-200 border-t-huanyu-pink-600 rounded-full w-8 h-8 animate-spin mx-auto mb-3"></div>
              正在加载地址...
            </div>
            
            <div v-else-if="addresses.length > 0" class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div 
                v-for="address in addresses" 
                :key="address.id" 
                class="relative border-2 rounded-2xl p-4 cursor-pointer transition-all duration-300 group hover:shadow-md"
                :class="selectedAddress.id === address.id ? 'border-huanyu-pink-500 bg-huanyu-pink-50/50' : 'border-gray-100 bg-white/50 hover:border-huanyu-pink-200'"
                @click="selectAddress(address)"
              >
                <div class="flex justify-between items-start mb-2">
                  <div class="flex items-center space-x-2">
                    <span class="font-bold text-gray-800">{{ address.name }}</span>
                    <span v-if="address.isDefault" class="px-2 py-0.5 bg-huanyu-pink-500 text-white text-xs rounded-full shadow-sm">默认</span>
                  </div>
                  <i v-if="selectedAddress.id === address.id" class="fas fa-check-circle text-huanyu-pink-500 text-xl"></i>
                </div>
                
                <div class="text-sm text-gray-600 space-y-1 mb-3">
                  <p>{{ address.phone }}</p>
                  <p class="line-clamp-2 h-10">{{ address.province }} {{ address.city }} {{ address.district }} {{ address.detailAddress }}</p>
                </div>

                <div class="flex justify-end space-x-2 opacity-0 group-hover:opacity-100 transition-opacity">
                  <button 
                    v-if="!address.isDefault" 
                    @click.stop="setDefault(address.id)" 
                    class="text-xs px-2 py-1 text-gray-500 hover:text-huanyu-pink-600 hover:bg-huanyu-pink-50 rounded transition-colors"
                  >设为默认</button>
                  <button 
                    @click.stop="deleteAddress(address.id)" 
                    class="text-xs px-2 py-1 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded transition-colors"
                  >删除</button>
                </div>
              </div>

              <!-- 添加新地址按钮卡片 -->
              <div 
                @click="openAddModal" 
                class="border-2 border-dashed border-gray-300 rounded-2xl p-4 flex flex-col items-center justify-center text-gray-400 cursor-pointer hover:border-huanyu-pink-400 hover:text-huanyu-pink-500 hover:bg-huanyu-pink-50/30 transition-all min-h-[160px]"
              >
                <i class="fas fa-plus-circle text-3xl mb-2"></i>
                <span class="font-medium">添加新地址</span>
              </div>
            </div>
            
            <div v-else class="text-center py-8">
              <div class="text-gray-400 mb-4">暂无收货地址</div>
              <button @click="openAddModal" class="px-6 py-2 bg-huanyu-pink-500 text-white rounded-full hover:bg-huanyu-pink-600 transition-colors shadow-lg shadow-huanyu-pink-500/30">
                <i class="fas fa-plus mr-2"></i> 添加收货地址
              </button>
            </div>
          </div>

          <!-- 配送方式 -->
          <div class="bg-white/80 backdrop-blur-md rounded-3xl p-6 shadow-xl border border-white/50">
            <h2 class="text-xl font-bold text-gray-800 mb-6 flex items-center border-b border-gray-100 pb-4">
              <i class="fas fa-truck text-huanyu-pink-500 mr-3 text-2xl"></i>
              配送方式
            </h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div 
                class="border-2 rounded-2xl p-4 cursor-pointer transition-all flex items-center justify-between group"
                :class="selectedShippingMethod.type === 'standard' ? 'border-huanyu-pink-500 bg-huanyu-pink-50/50' : 'border-gray-100 hover:border-huanyu-pink-200'"
                @click="selectShippingMethod('standard')"
              >
                <div>
                  <div class="font-bold text-gray-800 mb-1">标准配送</div>
                  <div class="text-xs text-gray-500">预计1-3个工作日送达</div>
                </div>
                <div class="text-right">
                  <div class="font-bold" :class="cartTotal >= 299 ? 'text-green-500' : 'text-gray-800'">
                    {{ cartTotal >= 299 ? '免费' : '¥20.00' }}
                  </div>
                  <i v-if="selectedShippingMethod.type === 'standard'" class="fas fa-check-circle text-huanyu-pink-500 mt-1"></i>
                </div>
              </div>
              
              <div 
                class="border-2 rounded-2xl p-4 cursor-pointer transition-all flex items-center justify-between group"
                :class="selectedShippingMethod.type === 'express' ? 'border-huanyu-pink-500 bg-huanyu-pink-50/50' : 'border-gray-100 hover:border-huanyu-pink-200'"
                @click="selectShippingMethod('express')"
              >
                <div>
                  <div class="font-bold text-gray-800 mb-1">极速达</div>
                  <div class="text-xs text-gray-500">专人配送，指定时间送达</div>
                </div>
                <div class="text-right">
                  <div class="font-bold text-gray-800">¥35.00</div>
                  <i v-if="selectedShippingMethod.type === 'express'" class="fas fa-check-circle text-huanyu-pink-500 mt-1"></i>
                </div>
              </div>
            </div>
            
            <div class="mt-6 bg-gray-50 rounded-xl p-4">
              <label class="block text-sm font-bold text-gray-700 mb-2">期望送达时间</label>
              <select v-model="selectedShippingMethod.deliveryTime" class="w-full bg-white border border-gray-200 rounded-lg px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-200 focus:border-huanyu-pink-500 outline-none transition-all">
                <option value="any">任意时间 (09:00 - 20:00)</option>
                <option value="morning">上午 (09:00 - 12:00)</option>
                <option value="afternoon">下午 (14:00 - 18:00)</option>
                <option value="evening">晚上 (18:00 - 21:00)</option>
              </select>
            </div>
          </div>

          <!-- 支付方式 -->
          <div class="bg-white/80 backdrop-blur-md rounded-3xl p-6 shadow-xl border border-white/50">
            <h2 class="text-xl font-bold text-gray-800 mb-6 flex items-center border-b border-gray-100 pb-4">
              <i class="fas fa-wallet text-huanyu-pink-500 mr-3 text-2xl"></i>
              支付方式
            </h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div 
                class="border-2 rounded-2xl p-4 cursor-pointer transition-all flex items-center"
                :class="selectedPaymentMethod === 'online' ? 'border-huanyu-pink-500 bg-huanyu-pink-50/50' : 'border-gray-100 hover:border-huanyu-pink-200'"
                @click="selectedPaymentMethod = 'online'"
              >
                <div class="w-10 h-10 rounded-full bg-blue-100 flex items-center justify-center mr-3 text-blue-500">
                  <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 18h.01M8 21h8a2 2 0 002-2V5a2 2 0 00-2-2H8a2 2 0 00-2 2v14a2 2 0 002 2z"></path></svg>
                </div>
                <div class="flex-1">
                  <div class="font-bold text-gray-800">在线支付</div>
                  <div class="text-xs text-gray-500">支持微信、支付宝</div>
                </div>
                <i v-if="selectedPaymentMethod === 'online'" class="fas fa-check-circle text-huanyu-pink-500 text-xl"></i>
              </div>
              
              <div 
                class="border-2 rounded-2xl p-4 cursor-pointer transition-all flex items-center"
                :class="selectedPaymentMethod === 'cod' ? 'border-huanyu-pink-500 bg-huanyu-pink-50/50' : 'border-gray-100 hover:border-huanyu-pink-200'"
                @click="selectedPaymentMethod = 'cod'"
              >
                <div class="w-10 h-10 rounded-full bg-green-100 flex items-center justify-center mr-3 text-green-500">
                  <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z"></path></svg>
                </div>
                <div class="flex-1">
                  <div class="font-bold text-gray-800">货到付款</div>
                  <div class="text-xs text-gray-500">送货上门后再付款</div>
                </div>
                <i v-if="selectedPaymentMethod === 'cod'" class="fas fa-check-circle text-huanyu-pink-500 text-xl"></i>
              </div>
            </div>
          </div>

          <!-- 订单备注 -->
          <div class="bg-white/80 backdrop-blur-md rounded-3xl p-6 shadow-xl border border-white/50">
            <h2 class="text-xl font-bold text-gray-800 mb-6 flex items-center border-b border-gray-100 pb-4">
              <i class="fas fa-pen-fancy text-huanyu-pink-500 mr-3 text-2xl"></i>
              备注留言
            </h2>
            <textarea 
              v-model="orderNotes" 
              class="w-full bg-gray-50 border border-gray-200 rounded-xl p-4 h-32 resize-none focus:ring-2 focus:ring-huanyu-pink-200 focus:border-huanyu-pink-500 outline-none transition-all"
              placeholder="选填：请填写贺卡留言、配送特殊要求等..."
            ></textarea>
          </div>
        </div>

        <!-- 右侧：订单汇总 -->
        <div class="lg:col-span-1 animate-fade-in-right">
          <div class="bg-white/80 backdrop-blur-md rounded-3xl p-6 shadow-xl border border-white/50 sticky top-24">
            <h2 class="text-xl font-bold text-gray-800 mb-6 pb-4 border-b border-gray-100">订单详情</h2>
            
            <!-- 商品清单 -->
            <div class="space-y-4 mb-6 max-h-[300px] overflow-y-auto pr-2 custom-scrollbar">
              <div v-for="item in cartItems" :key="item.id" class="flex items-center space-x-3">
                <div class="w-12 h-12 rounded-lg bg-gray-100 overflow-hidden flex-shrink-0">
                  <img 
                    :src="getProductImageUrl(item.image || item.productImage)" 
                    @error="handleImageError"
                    class="w-full h-full object-cover"
                  >
                </div>
                <div class="flex-1 min-w-0">
                  <div class="text-sm font-medium text-gray-800 truncate">{{ item.productName || item.name }}</div>
                  <div class="text-xs text-gray-500">x{{ item.quantity }}</div>
                </div>
                <div class="font-medium text-gray-800">¥{{ (item.price * item.quantity).toFixed(2) }}</div>
              </div>
            </div>
            
            <!-- 费用明细 -->
            <div class="space-y-3 pt-4 border-t border-gray-100">
              <div class="flex justify-between text-gray-600">
                <span>商品小计</span>
                <span>¥{{ cartSubtotal.toFixed(2) }}</span>
              </div>
              
              <!-- 优惠券选择 -->
              <div class="py-2">
                <div class="flex items-center space-x-2 mb-2">
                  <select v-model="selectedCouponCode" @change="previewCoupon(false)" class="flex-1 bg-gray-50 border border-gray-200 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-huanyu-pink-200 outline-none">
                    <option value="">选择优惠券</option>
                    <option v-for="c in availableCoupons" :key="c.Id || c.id" :value="c.Code || c.code">
                      {{ (c.Code || c.code) }} ({{ (c.DiscountType || c.discountType) === 'percent' ? (c.Value || c.value) + '% 折' : '-¥' + (c.Value || c.value) }})
                    </option>
                  </select>
                </div>
                <div class="flex items-center space-x-2">
                  <input v-model="manualCouponCode" placeholder="输入优惠码" class="flex-1 bg-gray-50 border border-gray-200 rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-huanyu-pink-200 outline-none" />
                  <button @click="previewCoupon(true)" class="px-3 py-2 bg-gray-800 text-white text-sm rounded-lg hover:bg-gray-700 transition-colors">验证</button>
                </div>
              </div>

              <div v-if="discount > 0" class="flex justify-between text-huanyu-pink-600">
                <span class="flex items-center"><i class="fas fa-ticket-alt mr-1"></i> 优惠抵扣</span>
                <span>-¥{{ discount.toFixed(2) }}</span>
              </div>
              
              <div class="flex justify-between text-gray-600">
                <span>运费</span>
                <span>¥{{ shippingFee.toFixed(2) }}</span>
              </div>
            </div>
            
            <div class="border-t border-gray-100 pt-6 mt-6">
              <div class="flex justify-between items-end mb-6">
                <span class="font-bold text-gray-800 text-lg">应付总额</span>
                <span class="text-3xl font-bold text-transparent bg-clip-text bg-gradient-to-r from-huanyu-pink-600 to-huanyu-red-500">
                  ¥{{ orderTotal.toFixed(2) }}
                </span>
              </div>
              
              <button 
                @click="submitOrder" 
                class="w-full py-4 bg-gradient-to-r from-huanyu-pink-500 to-huanyu-red-500 text-white rounded-xl font-bold shadow-lg hover:shadow-huanyu-pink-500/30 transition-all transform hover:-translate-y-0.5 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none flex items-center justify-center"
                :disabled="!selectedAddress.id || submitting"
              >
                <span v-if="submitting" class="flex items-center">
                  <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  处理中...
                </span>
                <span v-else>提交订单</span>
              </button>
              
              <p class="text-xs text-center text-gray-400 mt-4">
                提交订单即表示您同意我们的<a href="#" class="text-huanyu-pink-500 hover:underline">用户协议</a>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 支付模态框 -->
    <div v-if="showPaymentModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 backdrop-blur-sm bg-black/60">
      <div class="bg-white rounded-3xl w-full max-w-md overflow-hidden shadow-2xl animate-zoom-in">
        <div class="bg-gradient-to-r from-huanyu-pink-500 to-huanyu-red-500 p-6 text-white text-center">
          <h3 class="text-2xl font-bold mb-1">支付订单</h3>
          <p class="opacity-90">请尽快完成支付以确保发货</p>
        </div>
        
        <div class="p-8">
          <div class="text-center mb-8">
            <p class="text-gray-500 mb-2">支付金额</p>
            <div class="text-4xl font-bold text-gray-900">¥{{ orderTotal.toFixed(2) }}</div>
          </div>
          
          <div class="space-y-4 mb-8">
            <div 
              class="border rounded-xl p-4 cursor-pointer flex items-center transition-all hover:shadow-md"
              :class="paymentModalMethod === 'alipay' ? 'border-blue-500 bg-blue-50' : 'border-gray-200'"
              @click="paymentModalMethod = 'alipay'"
            >
              <i class="fab fa-alipay text-3xl text-blue-500 mr-4"></i>
              <span class="font-medium text-gray-800">支付宝支付</span>
              <i v-if="paymentModalMethod === 'alipay'" class="fas fa-check-circle text-blue-500 ml-auto text-xl"></i>
            </div>
            
            <div 
              class="border rounded-xl p-4 cursor-pointer flex items-center transition-all hover:shadow-md"
              :class="paymentModalMethod === 'wechat' ? 'border-green-500 bg-green-50' : 'border-gray-200'"
              @click="paymentModalMethod = 'wechat'"
            >
              <i class="fab fa-weixin text-3xl text-green-500 mr-4"></i>
              <span class="font-medium text-gray-800">微信支付</span>
              <i v-if="paymentModalMethod === 'wechat'" class="fas fa-check-circle text-green-500 ml-auto text-xl"></i>
            </div>

            <div 
              v-if="selectedPaymentMethod === 'cod'"
              class="border rounded-xl p-4 cursor-pointer flex items-center transition-all hover:shadow-md"
              :class="paymentModalMethod === 'cod' ? 'border-huanyu-pink-500 bg-huanyu-pink-50' : 'border-gray-200'"
              @click="paymentModalMethod = 'cod'"
            >
              <i class="fas fa-hand-holding-usd text-3xl text-huanyu-pink-500 mr-4"></i>
              <span class="font-medium text-gray-800">货到付款确认</span>
              <i v-if="paymentModalMethod === 'cod'" class="fas fa-check-circle text-huanyu-pink-500 ml-auto text-xl"></i>
            </div>
          </div>
          
          <div class="flex space-x-4">
            <button 
              @click="cancelPayment" 
              class="flex-1 py-3 border border-gray-300 rounded-xl text-gray-600 hover:bg-gray-50 transition-colors font-medium"
            >
              取消
            </button>
            <button 
              @click="confirmPayment" 
              class="flex-1 py-3 bg-gray-900 text-white rounded-xl hover:bg-gray-800 transition-colors font-medium shadow-lg"
              :disabled="processingPayment"
            >
              {{ processingPayment ? '处理中...' : '确认支付' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 支付成功提示 -->
    <div v-if="showPaymentSuccess" class="fixed inset-0 z-50 flex items-center justify-center p-4 backdrop-blur-sm bg-black/60">
      <div class="bg-white rounded-3xl w-full max-w-sm overflow-hidden shadow-2xl animate-zoom-in text-center p-8">
        <div class="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6">
          <i class="fas fa-check text-4xl text-green-500"></i>
        </div>
        <h3 class="text-2xl font-bold text-gray-900 mb-2">支付成功！</h3>
        <p class="text-gray-500 mb-8">您的订单已成功支付，我们将尽快为您安排发货。</p>
        <button 
          @click="viewOrderDetail"
          class="w-full py-3 bg-gradient-to-r from-huanyu-pink-500 to-huanyu-red-500 text-white rounded-xl font-bold shadow-lg hover:shadow-huanyu-pink-500/30 transition-all transform hover:-translate-y-0.5"
        >
          查看订单详情
        </button>
      </div>
    </div>

    <!-- 添加地址模态框 -->
    <div v-if="showAddModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 backdrop-blur-sm bg-black/60">
      <div class="bg-white rounded-3xl w-full max-w-lg overflow-hidden shadow-2xl animate-zoom-in">
        <div class="flex justify-between items-center px-6 py-4 border-b border-gray-100">
          <h3 class="text-xl font-bold text-gray-800">添加收货地址</h3>
          <button @click="showAddModal = false" class="text-gray-400 hover:text-gray-600 w-8 h-8 rounded-full hover:bg-gray-100 flex items-center justify-center transition-colors">
            <i class="fas fa-times"></i>
          </button>
        </div>
        
        <div class="p-6">
          <form @submit.prevent="submitAddressForm" class="space-y-4">
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-sm font-bold text-gray-700 mb-2">收货人</label>
                <input v-model="addressForm.name" type="text" required class="w-full bg-gray-50 border border-gray-200 rounded-xl px-4 py-2.5 focus:ring-2 focus:ring-huanyu-pink-200 focus:border-huanyu-pink-500 outline-none transition-all" placeholder="姓名" />
              </div>
              <div>
                <label class="block text-sm font-bold text-gray-700 mb-2">手机号</label>
                <input v-model="addressForm.phone" type="tel" required class="w-full bg-gray-50 border border-gray-200 rounded-xl px-4 py-2.5 focus:ring-2 focus:ring-huanyu-pink-200 focus:border-huanyu-pink-500 outline-none transition-all" placeholder="手机号码" />
              </div>
            </div>
            
            <div>
              <label class="block text-sm font-bold text-gray-700 mb-2">所在地区</label>
              <div class="grid grid-cols-3 gap-2">
                <select v-model="addressForm.province" @change="handleProvinceChange($event.target.value)" required class="w-full bg-gray-50 border border-gray-200 rounded-xl px-2 py-2.5 text-sm focus:ring-2 focus:ring-huanyu-pink-200 focus:border-huanyu-pink-500 outline-none transition-all">
                  <option value="">选择省</option>
                  <option v-for="item in provinces" :key="item.code" :value="item.code">{{ item.name }}</option>
                </select>
                <select v-model="addressForm.city" @change="handleCityChange($event.target.value)" required class="w-full bg-gray-50 border border-gray-200 rounded-xl px-2 py-2.5 text-sm focus:ring-2 focus:ring-huanyu-pink-200 focus:border-huanyu-pink-500 outline-none transition-all">
                  <option value="">选择市</option>
                  <option v-for="item in cities" :key="item.code" :value="item.code">{{ item.name }}</option>
                </select>
                <select v-model="addressForm.district" required class="w-full bg-gray-50 border border-gray-200 rounded-xl px-2 py-2.5 text-sm focus:ring-2 focus:ring-huanyu-pink-200 focus:border-huanyu-pink-500 outline-none transition-all">
                  <option value="">选择区/县</option>
                  <option v-for="item in districts" :key="item.code" :value="item.code">{{ item.name }}</option>
                </select>
              </div>
            </div>
            
            <div>
              <label class="block text-sm font-bold text-gray-700 mb-2">详细地址</label>
              <textarea v-model="addressForm.detailAddress" rows="3" required class="w-full bg-gray-50 border border-gray-200 rounded-xl px-4 py-2.5 focus:ring-2 focus:ring-huanyu-pink-200 focus:border-huanyu-pink-500 outline-none transition-all resize-none" placeholder="街道、小区、楼栋号、门牌号等"></textarea>
            </div>
            
            <div class="flex items-center">
              <label class="flex items-center cursor-pointer">
                <input v-model="addressForm.isDefault" type="checkbox" class="w-5 h-5 text-huanyu-pink-600 rounded border-gray-300 focus:ring-huanyu-pink-500" />
                <span class="ml-2 text-gray-700">设为默认收货地址</span>
              </label>
            </div>
            
            <div class="pt-4 flex justify-end space-x-3">
              <button type="button" @click="showAddModal = false" class="px-6 py-2.5 border border-gray-300 rounded-xl text-gray-600 hover:bg-gray-50 transition-colors font-medium">取消</button>
              <button type="submit" class="px-6 py-2.5 bg-huanyu-pink-500 text-white rounded-xl hover:bg-huanyu-pink-600 transition-colors font-medium shadow-lg shadow-huanyu-pink-500/30">保存地址</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import orderService from '@/services/orderService'
import { couponService } from '@/services/coupon'

import userService from '@/services/userService'
import { notifySuccess, notifyError, notifyInfo } from '@/utils/notify'
import { useAddress } from '@/composables/useAddress'
import { useCartStore } from '@/stores/cart'

import { getProductImageUrl } from '@/utils/avatar.js'

const router = useRouter()
const cartStore = useCartStore()

// 图片加载错误处理
const handleImageError = (e) => {
  e.target.src = '/images/product-placeholder.svg'
}

// 地址管理
const { 
  addresses, 
  loading: isLoadingAddresses, 
  showAddModal, 
  form: addressForm, 
  provinces, 
  cities, 
  districts, 
  initRegionData, 
  loadAddresses, 
  openAddModal, 
  handleProvinceChange, 
  handleCityChange, 
  submitForm: submitAddressForm, 
  removeAddress: deleteAddress, 
  setAsDefault: setDefault 
} = useAddress()

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
const selectedAddress = ref({})
const selectedAddressId = ref(null)

// 配送方式
const selectedShippingMethod = ref({
  type: 'standard',
  deliveryTime: 'any'
})

// 购物车数据
const cartItems = computed(() => cartStore.cartItems)

// 计算属性
const cartSubtotal = computed(() => {
  return cartStore.cartTotal
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

const submitOrder = async () => {
  if (!selectedAddress.value.id && !selectedAddress.value.detailAddress) {
    ElMessage.warning('请选择或添加收货地址')
    return
  }

  submitting.value = true
  try {
    console.log('准备提交订单，从数据库获取的地址信息:', selectedAddress.value)
    
    // 构建 CreateOrderDto
    const orderData = {
      RecipientName: selectedAddress.value.name || selectedAddress.value.RecipientName || '用户',
      Phone: selectedAddress.value.phone || selectedAddress.value.PhoneNumber || '13800138000',
      ShippingAddress: selectedAddress.value.fullAddress || `${selectedAddress.value.province || ''}${selectedAddress.value.city || ''}${selectedAddress.value.district || ''}${selectedAddress.value.detailAddress || ''}`,
      PaymentMethod: selectedPaymentMethod.value === 'cod' ? 'cash_on_delivery' : 'online',
      CouponCode: (manualCouponCode?.value || selectedCouponCode?.value || '').trim() || null,
      Items: cartStore.cartItems.map(item => ({
        ProductId: item.productId || item.id,
        Quantity: item.quantity
      }))
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

    if (selectedPaymentMethod.value === 'online') {
      await confirmPayment()
    } else {
      // 货到付款
      await orderService.updateOrderStatus(currentOrderId.value, 'unpaid')
      showPaymentSuccess.value = true
    }

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
    // 在线支付（支付宝）
    const res = await orderService.repayOrder(currentOrderId.value)
    if (res.success && res.data) {
      const paymentLink = res.data.paymentLink || res.data.PaymentLink
      
      if (paymentLink && paymentLink.startsWith('<form')) {
        // 支付宝返回的是HTML表单，需要渲染并提交
        const div = document.createElement('div')
        div.innerHTML = paymentLink
        document.body.appendChild(div)
        // 提交表单
        const form = div.querySelector('form')
        if (form) {
          form.submit()
        } else {
            ElMessage.error('支付表单格式错误')
        }
      } else if (paymentLink) {
        // 普通URL跳转
        window.location.href = paymentLink
      } else {
        ElMessage.error('获取支付链接为空')
      }
    } else {
      ElMessage.error('获取支付链接失败')
    }
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
  await initRegionData()
  
  // 确保购物车数据已加载
  if (cartStore.cartItems.length === 0) {
    await cartStore.fetchCart()
  }
  
  await loadAddresses()
  try {
    const res = await couponService.getAvailable(cartSubtotal.value)
    availableCoupons.value = res?.data || res || []
  } catch {}
})

// 监听地址列表变化，自动选择默认地址
watch(addresses, (newVal) => {
  if (newVal && newVal.length > 0) {
    const defaultAddress = newVal.find(addr => addr.isDefault)
    if (defaultAddress) {
      selectAddress(defaultAddress)
    } else {
      selectAddress(newVal[0])
    }
  } else {
    selectedAddress.value = {}
    selectedAddressId.value = null
  }
}, { immediate: true })

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
</script>

<style scoped>
/* 自定义滚动条 */
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: #e5e7eb;
  border-radius: 20px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background-color: #d1d5db;
}
</style>