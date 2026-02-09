<template>
  <div class="profile-container">
    <div class="max-w-7xl mx-auto py-12 px-4 sm:px-6 lg:px-8">
      <h1 class="text-3xl font-bold text-gray-900 mb-8">个人资料</h1>
      
      <!-- 标签页导航 -->
      <div class="border-b border-gray-200 mb-8">
        <nav class="flex space-x-8">
          <button 
            @click="activeSection = 'info'" 
            :class="['py-4 px-1 border-b-2 font-medium text-sm', activeSection === 'info' ? 'border-huanyu-pink-500 text-huanyu-pink-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300']"
          >
            基本信息
          </button>
          <button 
            v-if="!userStore.isAdmin"
            @click="activeSection = 'addresses' " 
            :class="['py-4 px-1 border-b-2 font-medium text-sm', activeSection === 'addresses' ? 'border-huanyu-pink-500 text-huanyu-pink-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300']"
          >
            收货地址
          </button>
          <button 
            v-if="!userStore.isAdmin"
            @click="activeSection = 'security' " 
            :class="['py-4 px-1 border-b-2 font-medium text-sm', activeSection === 'security' ? 'border-huanyu-pink-500 text-huanyu-pink-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300']"
          >
            安全设置
          </button>
          <button 
            v-if="!userStore.isAdmin"
            @click="activeSection = 'coupons' " 
            :class="['py-4 px-1 border-b-2 font-medium text-sm', activeSection === 'coupons' ? 'border-huanyu-pink-500 text-huanyu-pink-600' : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300']"
          >
            优惠券
          </button>
        </nav>
      </div>
      
      <!-- 基本信息 -->
      <div v-if="activeSection === 'info'" class="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <!-- 左侧：用户信息卡片 -->
        <div class="lg:col-span-1">
          <div class="bg-white rounded-lg shadow p-6">
            <div class="text-center">
              <div class="relative inline-block">
                <div class="w-24 h-24 bg-gray-200 rounded-full mx-auto mb-4 overflow-hidden">
                  <img
                    :src="userInfo?.avatar ? getAvatarUrlLocal() : getDefaultAvatarUrl()"
                    :alt="userInfo?.name || '用户'"
                    class="w-full h-full object-cover"
                    @error="handleAvatarError"
                    :key="avatarKey"
                  />
                </div>
                <div class="absolute bottom-0 right-0">
                  <button 
                    @click="triggerAvatarUpload" 
                    class="bg-huanyu-pink-600 text-white p-2 rounded-full hover:bg-huanyu-pink-700 transition-colors"
                    title="更换头像"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"></path>
                    </svg>
                  </button>
                  <input 
                    ref="avatarInput" 
                    type="file" 
                    accept="image/*" 
                    class="hidden"
                    @change="handleAvatarChange"
                  />
                </div>
              </div>
              <h2 class="text-xl font-semibold mb-2">{{ userInfo?.name || '未设置' }}</h2>
              <div v-if="userInfo?.role === 'vip'" class="flex justify-center mb-2">
                <span class="px-3 py-1 bg-yellow-100 text-yellow-800 rounded-full text-sm">VIP</span>
              </div>
              <p class="text-gray-600 mb-4">{{ userInfo?.email || userInfo?.Email || '未设置' }}</p>
              <p class="text-gray-600 mb-4">性别：{{ getGenderText(userInfo?.gender || userInfo?.Gender || '') }}</p>
              <div class="flex justify-center space-x-4">
                <span :class="['px-3 py-1 rounded-full text-sm', getRoleClass(userInfo?.role || '')]">
                  {{ getRoleText(userInfo?.role || '') }}
                </span>
                <span class="px-3 py-1 bg-gray-100 text-gray-800 rounded-full text-sm">
                  注册时间: {{ formatDate(userInfo?.createdAt || '') }}
                </span>
              </div>
            </div>
            <div class="mt-6 space-y-4">
              <div v-if="userInfo?.role === 'vip'" class="bg-yellow-50 border border-yellow-200 rounded-md p-3">
                <div class="text-sm font-medium text-yellow-800 mb-2">VIP权益</div>
                <ul class="text-sm text-yellow-700 space-y-1">
                  <li>专属标识与优先客服</li>
                  <li>结算页专享优惠与推荐</li>
                  <li>订单处理优先级提升</li>
                </ul>
              </div>
              <div>
                <h3 class="text-sm font-medium text-gray-700 mb-1">用户ID</h3>
                <p class="text-gray-600">{{ userInfo?.id || '未设置' }}</p>
              </div>
              <div>
                <h3 class="text-sm font-medium text-gray-700 mb-1">联系电话</h3>
                <p class="text-gray-600">{{ userInfo?.phone || '未设置' }}</p>
              </div>
              <div>
                <h3 class="text-sm font-medium text-gray-700 mb-1">地址</h3>
                <p class="text-gray-600">{{ (userInfo?.address || '').trim() || '未设置' }}</p>
              </div>
              <div>
                <h3 class="text-sm font-medium text-gray-700 mb-1">上次登录</h3>
                <p class="text-gray-600">{{ formatDate(userInfo?.lastLoginAt || '') || '从未登录' }}</p>
              </div>
            </div>
          </div>
        </div>
        
        <!-- 右侧：详细信息编辑 -->
        <div class="lg:col-span-2">
          <div class="bg-white rounded-lg shadow p-6">
            <h2 class="text-xl font-semibold mb-6">编辑个人信息</h2>
            <form @submit.prevent="updateUserInfo" class="space-y-6">
              <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div>
                  <label for="name" class="block text-sm font-medium text-gray-700 mb-1">姓名</label>
                  <input 
                    type="text" 
                    id="name" 
                    v-model="userInfo.name" 
                    @blur="updateUserInfo"
                    class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
                  />
                </div>
                <div>
                  <label for="email" class="block text-sm font-medium text-gray-700 mb-1">邮箱</label>
                  <input 
                    type="email" 
                    id="email" 
                    v-model="userInfo.email" 
                    class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
                  />
                </div>
              </div>
              <div>
                <label for="gender" class="block text-sm font-medium text-gray-700 mb-1">性别</label>
                <select 
                  id="gender" 
                  v-model="userInfo.gender"
                  class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
                >
                  <option value="">未设置</option>
                  <option value="男">男</option>
                  <option value="女">女</option>
                  <option value="其他">其他</option>
                </select>
              </div>
              <div>
                <label for="phone" class="block text-sm font-medium text-gray-700 mb-1">联系电话</label>
                <input 
                  type="tel" 
                  id="phone" 
                  v-model="userInfo.phone" 
                  class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
                />
              </div>
              <div v-if="!userStore.isAdmin">
                  <label for="address" class="block text-sm font-medium text-gray-700 mb-1">地址</label>
                  <textarea 
                    id="address" 
                    v-model="userInfo.address" 
                    rows="3" 
                    class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
                  ></textarea>
                </div>
              <div class="flex justify-end">
                <button 
                  type="submit" 
                  :disabled="loading" 
                  class="px-6 py-2 bg-huanyu-pink-600 text-white rounded-md hover:bg-huanyu-pink-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {{ loading ? '保存中...' : '保存修改' }}
                </button>
              </div>
            </form>
          </div>
        </div>
      </div>

      <!-- 我的优惠券 -->
      <div v-if="activeSection === 'coupons' && !userStore.isAdmin" class="space-y-6">
        <div class="flex justify-between items-center">
          <h2 class="text-xl font-semibold">我的优惠券</h2>
          <button 
            @click="refreshCoupons" 
            class="px-4 py-2 bg-huanyu-pink-600 text-white rounded-md hover:bg-huanyu-pink-700 transition-colors"
          >
            刷新
          </button>
        </div>
        <div v-if="myCoupons.length > 0" class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div v-for="uc in myCoupons" :key="uc.Id || uc.id" class="bg-white rounded-lg shadow p-6 border-l-4" :class="(uc.Status||uc.status)==='used' ? 'border-gray-300 opacity-70' : 'border-huanyu-pink-500'">
            <div class="flex justify-between items-start mb-2">
              <div>
                <h3 class="font-medium text-gray-900">{{ uc.Code || uc.code }}</h3>
                <p class="text-gray-600 text-sm">类型：{{ (uc.DiscountType||uc.discountType)==='percent' ? '百分比' : '固定金额' }}</p>
              </div>
              <span class="px-3 py-1 rounded-full text-sm" :class="(uc.Status||uc.status)==='used' ? 'bg-gray-100 text-gray-600' : 'bg-green-100 text-green-800'">
                {{ (uc.Status||uc.status)==='used' ? '已使用' : '可使用' }}
              </span>
            </div>
            <div class="text-gray-700 mb-2">
              <span>面值：{{ (uc.DiscountType||uc.discountType)==='percent' ? (uc.Value||uc.value)+'%' : ('¥'+(uc.Value||uc.value)) }}</span>
              <span class="ml-4">门槛：¥{{ uc.MinOrderAmount || uc.minOrderAmount }}</span>
            </div>
            <div class="text-gray-500 text-sm">
              <span>领取：{{ formatDate(uc.ClaimedAt || uc.claimedAt) }}</span>
              <span v-if="uc.UsedAt || uc.usedAt" class="ml-4">使用：{{ formatDate(uc.UsedAt || uc.usedAt) }}</span>
              <span class="ml-4">有效期：{{ formatDate(uc.StartAt || uc.startAt) }} - {{ formatDate(uc.EndAt || uc.endAt) }}</span>
            </div>
          </div>
        </div>
        <div v-else class="text-center py-8 text-gray-500">暂无优惠券</div>
      </div>
      
      <!-- 收货地址 -->
      <div v-if="activeSection === 'addresses' && !userStore.isAdmin" class="space-y-6">
        <div class="flex justify-between items-center">
          <h2 class="text-xl font-semibold">收货地址</h2>
          <button 
            @click="openAddModal" 
            class="px-4 py-2 bg-huanyu-pink-600 text-white rounded-md hover:bg-huanyu-pink-700 transition-colors"
          >
            添加地址
          </button>
        </div>
        
        <!-- 地址列表 -->
        <div v-if="addresses.length > 0" class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div 
            v-for="address in addresses" 
            :key="address.id" 
            class="bg-white rounded-lg shadow p-6 border-l-4" 
            :class="address.isDefault ? 'border-huanyu-pink-500' : 'border-gray-200'"
          >
            <div class="flex justify-between items-start mb-4">
              <div>
                <h3 class="font-medium text-gray-900">{{ address.recipientName }}</h3>
                <p class="text-gray-600">{{ address.phoneNumber }}</p>
              </div>
              <div class="flex space-x-2">
                <button 
                  @click="openEditModal(address)" 
                  class="text-sm text-huanyu-pink-600 hover:text-huanyu-pink-800"
                >
                  编辑
                </button>
                <button 
                  @click="deleteAddress(address.id || address.Id)" 
                  class="text-sm text-red-600 hover:text-red-800"
                >
                  删除
                </button>
                <button 
                  v-if="!address.isDefault" 
                  @click="setDefault(address.id)" 
                  class="text-sm text-gray-600 hover:text-gray-800"
                >
                  设为默认
                </button>
              </div>
            </div>
            <p class="text-gray-700 mb-2">
              {{ address.province }} {{ address.city }} {{ address.district }} {{ address.detailAddress }}
            </p>
            <div class="flex items-center space-x-2">
              <span v-if="address.isDefault" class="text-xs bg-huanyu-pink-100 text-huanyu-pink-800 px-2 py-0.5 rounded">
                默认地址
              </span>
              <span class="text-xs text-gray-500">{{ address.postalCode || '无邮编' }}</span>
            </div>
          </div>
        </div>
        <div v-else class="text-center py-8 text-gray-500">
          暂无收货地址，请点击「添加地址」创建
        </div>
        
        <!-- 添加地址模态框 -->
        <div v-if="showAddAddress" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div class="bg-white rounded-lg p-8 max-w-md w-full mx-4">
            <h3 class="text-xl font-semibold mb-6">添加收货地址</h3>
            <div class="space-y-4">
              <div class="flex items-center space-x-2">
                <span class="text-gray-500 w-20">所在地区</span>
                <div class="grid grid-cols-3 gap-2 flex-1">
                  <select v-model="addressForm.province" @change="handleProvinceChange($event.target.value)" class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent">
                    <option value="">选择省</option>
                    <option v-for="item in provinces" :key="item.code" :value="item.code">{{ item.name }}</option>
                  </select>
                  <select v-model="addressForm.city" @change="handleCityChange($event.target.value)" class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent">
                    <option value="">选择市</option>
                    <option v-for="item in cities" :key="item.code" :value="item.code">{{ item.name }}</option>
                  </select>
                  <select v-model="addressForm.district" class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent">
                    <option value="">选择区/县</option>
                    <option v-for="item in districts" :key="item.code" :value="item.code">{{ item.name }}</option>
                  </select>
                </div>
              </div>
              <div class="flex items-center space-x-2">
                <span class="text-gray-500 w-20">详细地址</span>
                <textarea v-model="addressForm.detailAddress" class="flex-1 border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent" placeholder="请输入街道、门牌号等"></textarea>
              </div>
              <div class="flex items-center space-x-2">
                <span class="text-gray-500 w-20">收货人</span>
                <input v-model="addressForm.name" type="text" class="flex-1 border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent" placeholder="请填写收货人姓名">
              </div>
              <div class="flex items-center space-x-2">
                <span class="text-gray-500 w-20">手机号码</span>
                <input v-model="addressForm.phone" type="tel" class="flex-1 border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent" placeholder="请填写手机号码">
              </div>
              <div class="flex items-center space-x-2">
                <span class="w-20"></span>
                <label class="flex items-center">
                  <input v-model="addressForm.isDefault" type="checkbox" class="form-checkbox text-huanyu-pink-500 rounded border-gray-300 focus:ring-huanyu-pink-500">
                  <span class="ml-2 text-sm text-gray-600">设为默认地址</span>
                </label>
              </div>
            </div>
            <div class="mt-6 flex justify-end space-x-4">
              <button @click="showAddAddress = false" class="px-6 py-2 border border-gray-300 rounded-md text-gray-600 hover:bg-gray-50 transition-colors">
                取消
              </button>
              <button @click="submitAddressForm" class="px-6 py-2 bg-gradient-to-r from-huanyu-pink-500 to-huanyu-red-500 text-white rounded-md hover:shadow-lg transition-all transform hover:-translate-y-0.5">
                {{ loading ? '保存中...' : '保存' }}
              </button>
            </div>
          </div>
        </div>
        
        <!-- 编辑地址模态框 -->
        <div v-if="showEditAddress" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div class="bg-white rounded-lg p-8 max-w-md w-full mx-4">
            <h3 class="text-xl font-semibold mb-6">编辑收货地址</h3>
            <div class="space-y-4">
              <div class="flex items-center space-x-2">
                <span class="text-gray-500 w-20">所在地区</span>
                <div class="grid grid-cols-3 gap-2 flex-1">
                  <select v-model="addressForm.province" @change="handleProvinceChange($event.target.value)" class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent">
                    <option value="">选择省</option>
                    <option v-for="item in provinces" :key="item.code" :value="item.code">{{ item.name }}</option>
                  </select>
                  <select v-model="addressForm.city" @change="handleCityChange($event.target.value)" class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent">
                    <option value="">选择市</option>
                    <option v-for="item in cities" :key="item.code" :value="item.code">{{ item.name }}</option>
                  </select>
                  <select v-model="addressForm.district" class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent">
                    <option value="">选择区/县</option>
                    <option v-for="item in districts" :key="item.code" :value="item.code">{{ item.name }}</option>
                  </select>
                </div>
              </div>
              <div class="flex items-center space-x-2">
                <span class="text-gray-500 w-20">详细地址</span>
                <textarea v-model="addressForm.detailAddress" class="flex-1 border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent" placeholder="请输入街道、门牌号等"></textarea>
              </div>
              <div class="flex items-center space-x-2">
                <span class="text-gray-500 w-20">收货人</span>
                <input v-model="addressForm.name" type="text" class="flex-1 border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent" placeholder="请填写收货人姓名">
              </div>
              <div class="flex items-center space-x-2">
                <span class="text-gray-500 w-20">手机号码</span>
                <input v-model="addressForm.phone" type="tel" class="flex-1 border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent" placeholder="请填写手机号码">
              </div>
              <div class="flex items-center space-x-2">
                <span class="w-20"></span>
                <label class="flex items-center">
                  <input v-model="addressForm.isDefault" type="checkbox" class="form-checkbox text-huanyu-pink-500 rounded border-gray-300 focus:ring-huanyu-pink-500">
                  <span class="ml-2 text-sm text-gray-600">设为默认地址</span>
                </label>
              </div>
            </div>
            <div class="mt-6 flex justify-end space-x-4">
              <button @click="showEditAddress = false" class="px-6 py-2 border border-gray-300 rounded-md text-gray-600 hover:bg-gray-50 transition-colors">
                取消
              </button>
              <button @click="submitAddressForm" class="px-6 py-2 bg-gradient-to-r from-huanyu-pink-500 to-huanyu-red-500 text-white rounded-md hover:shadow-lg transition-all transform hover:-translate-y-0.5">
                {{ loading ? '保存中...' : '保存' }}
              </button>
            </div>
          </div>
        </div>
      </div>
      
      <!-- 安全设置 -->
      <div v-if="activeSection === 'security' && !userStore.isAdmin" class="space-y-6">
        <h2 class="text-xl font-semibold">安全设置</h2>
        <div class="bg-white rounded-lg shadow p-6">
          <h3 class="text-lg font-medium mb-4">修改密码</h3>
          <form @submit.prevent="changePassword" class="space-y-4">
            <div>
              <label for="currentPassword" class="block text-sm font-medium text-gray-700 mb-1">当前密码</label>
              <input 
                type="password" 
                id="currentPassword" 
                v-model="passwordForm.currentPassword" 
                class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
              />
            </div>
            <div>
              <label for="newPassword" class="block text-sm font-medium text-gray-700 mb-1">新密码</label>
              <input 
                type="password" 
                id="newPassword" 
                v-model="passwordForm.newPassword" 
                class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
              />
            </div>
            <div>
              <label for="confirmPassword" class="block text-sm font-medium text-gray-700 mb-1">确认新密码</label>
              <input 
                type="password" 
                id="confirmPassword" 
                v-model="passwordForm.confirmPassword" 
                class="w-full border border-gray-300 rounded-md px-4 py-2 focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
              />
            </div>
            <div class="flex justify-end">
              <button 
                type="submit" 
                :disabled="loading" 
                class="px-6 py-2 bg-huanyu-pink-600 text-white rounded-md hover:bg-huanyu-pink-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {{ loading ? '修改中...' : '修改密码' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, watch, computed } from 'vue'
import { getAvatarUrl, handleAvatarError, getDefaultAvatarUrl } from '@/utils/avatar.js'
import userService from '@/services/userService.js'
import { useUserStore } from '@/stores/user'
import { useAddress } from '@/composables/useAddress'
import { notifySuccess, notifyError, notifyInfo } from '@/utils/notify'
import { couponService } from '@/services/coupon'

// 获取用户存储
const userStore = useUserStore()

// 地址管理组合式函数
const { 
  addresses, 
  loading: addressLoading, 
  showAddModal: showAddAddress, 
  showEditModal: showEditAddress,
  form: addressForm, 
  provinces, 
  cities, 
  districts, 
  initRegionData, 
  loadAddresses, 
  openAddModal, 
  openEditModal, 
  handleProvinceChange, 
  handleCityChange, 
  submitForm: submitAddressForm, 
  removeAddress: deleteAddress, 
  setAsDefault: setDefault 
} = useAddress()

// 响应式数据
const activeSection = ref('info')
const showChangePassword = ref(false)
const loading = ref(false)
const error = ref('')
const uploadingAvatar = ref(false)
const avatarInput = ref(null)
const refreshInterval = ref(null)
let storageHandler = null

// 用户信息
const userInfo = ref({
  id: '',
  name: '',
  email: '',
  phone: '',
  avatar: '',
  role: '',
  createdAt: '',
  lastLoginAt: '',
  address: '',
  gender: ''
})

// 地址相关
const myCoupons = ref([])

const defaultAddressText = computed(() => {
  const list = addresses.value || []
  const def = list.find(a => a.isDefault) || list[0]
  if (!def) return ''
  const p = def.province || ''
  const c = def.city || ''
  const d = def.district || ''
  const det = def.detailAddress || ''
  return `${p} ${c} ${d} ${det}`.trim()
})

watch(defaultAddressText, (val) => {
  userInfo.value.address = val || ''
}, { immediate: true })

// 密码修改
const passwordForm = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

// 头像相关
const avatarKey = ref(0)

// 初始化
  onMounted(async () => {
  if (userStore.user) {
    userInfo.value = {
      id: userStore.user.id,
      name: userStore.user.name || userStore.user.FullName || userStore.user.username || '',
      email: userStore.user.email || '',
      phone: userStore.user.phone || '',
      avatar: userStore.user.avatar || '',
      role: userStore.user.role || '',
      createdAt: userStore.user.createdAt || '',
      lastLoginAt: userStore.user.lastLoginAt || '',
      gender: userStore.user.gender || userStore.user.Gender || ''
    }
  } else {
    try {
      const cached = localStorage.getItem('user')
      if (cached) {
        const u = JSON.parse(cached)
        if (u) {
          userInfo.value = {
            id: u.id || u.Id,
            name: u.name || u.FullName || u.username || u.Username || '',
            email: u.email || u.Email || '',
            phone: u.phone || u.Phone || '',
            avatar: u.avatar || u.Avatar || '',
            role: u.role || u.Role || '',
            createdAt: u.createdAt || '',
            lastLoginAt: u.lastLoginAt || u.LastLoginAt || '',
            gender: u.gender || u.Gender || ''
          }
        }
      }
    } catch {}
  }
  await loadUserInfo()
  await initRegionData()
  await loadAddresses()
  await refreshCoupons()
  refreshInterval.value = setInterval(async () => {
    await loadUserInfo()
  }, 60000)

  const handleStorageChange = (e) => {
    if (e.key === 'userProfileUpdated' || e.key === 'user') {
      loadUserInfo()
    }
  }
  window.addEventListener('storage', handleStorageChange)
  storageHandler = handleStorageChange
})

// 清理
onBeforeUnmount(() => {
  // 清除定时器
  if (refreshInterval.value) {
    clearInterval(refreshInterval.value)
  }
  if (storageHandler) {
    window.removeEventListener('storage', storageHandler)
  }
})

// 加载用户信息
const loadUserInfo = async () => {
  try {
    loading.value = true
    const res = await userService.getUserInfo()
    if (res.success) {
      const userData = res.data
      if (userData) {
        const normalized = {
          id: userData.id || userData.Id || userInfo.value.id,
          name: userData.name || userData.FullName || userInfo.value.name || '',
          email: userData.email || userData.Email || userInfo.value.email || '',
          phone: userData.phone || userData.Phone || userInfo.value.phone || '',
          avatar: userData.avatar || userData.Avatar || userInfo.value.avatar || '',
          role: userData.role || userData.Role || userInfo.value.role || '',
          createdAt: userData.createdAt || userInfo.value.createdAt || '',
          lastLoginAt: userData.lastLoginAt || userData.LastLoginAt || userInfo.value.lastLoginAt || '',
          address: userData.address || userData.Address || userInfo.value.address || '',
          gender: userData.gender || userData.Gender || userInfo.value.gender || ''
        }
        userInfo.value = normalized
        try { localStorage.setItem('user', JSON.stringify(normalized)) } catch {}
      }
    }
  } catch (err) {
    console.error('获取用户信息失败:', err.message)
  } finally {
    loading.value = false
  }
}

// 地址管理逻辑已迁移至 useAddress composable

// 更新用户信息
const updateUserInfo = async () => {
  try {
    loading.value = true
    // 发送完整的用户信息，包括头像
    const res = await userService.updateUserInfo({
      name: userInfo.value.name,
      email: userInfo.value.email,
      phone: userInfo.value.phone,
      address: userInfo.value.address,
      gender: userInfo.value.gender,
      avatar: userInfo.value.avatar
    })
    if (res.success) {
      notifySuccess('用户信息更新成功')
      // 将后端返回的数据与当前用户信息合并，确保头像不会丢失
      const updatedUser = {
        ...userInfo.value,
        ...res.data
      }
      userInfo.value = updatedUser
      // 使用正确的方法更新用户存储
      await userStore.fetchUserProfile()
      localStorage.setItem('user', JSON.stringify(updatedUser))
      await loadUserInfo()
    } else {
      notifyError(res.data?.message || '更新用户信息失败')
    }
  } catch (err) {
    notifyError('更新用户信息失败')
  } finally {
    loading.value = false
  }
}

// 修改密码
const changePassword = async () => {
  try {
    if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
      notifyError('两次输入的密码不一致')
      return
    }
    
    loading.value = true
    const res = await userService.changePassword({
      CurrentPassword: passwordForm.value.currentPassword,
      NewPassword: passwordForm.value.newPassword
    })
    if (res.success) {
      notifySuccess('密码修改成功')
      passwordForm.value = {
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      }
    } else {
      notifyError(res.data?.message || '修改密码失败')
    }
  } catch (err) {
    notifyError('修改密码失败')
  } finally {
    loading.value = false
  }
}

// 触发头像上传
const triggerAvatarUpload = () => {
  avatarInput.value?.click()
}

// 处理头像变化
const handleAvatarChange = async (event) => {
  const file = event.target.files?.[0]
  if (!file) return
  
  try {
    uploadingAvatar.value = true
    const formData = new FormData()
    formData.append('file', file)
    
    // 1. 上传头像到后端
    const res = await userService.uploadAvatar(formData)
    if (res.success) {
      // 2. 更新本地状态
      userInfo.value.avatar = res.data.avatarUrl
      userStore.user = userInfo.value
      localStorage.setItem('user', JSON.stringify(userInfo.value))
      notifySuccess('头像上传成功')
      avatarKey.value++
    }
  } catch (err) {
    // 静默处理错误，避免过多日志
    console.error('头像上传失败:', err.message)
  } finally {
    uploadingAvatar.value = false
    // 清空文件输入
    if (avatarInput.value) {
      avatarInput.value.value = ''
    }
  }
}

// 从localStorage获取头像URL
const getAvatarUrlLocal = () => {
  try {
    // 使用getAvatarUrl函数统一处理头像URL，确保正确返回用户上传的头像
    return getAvatarUrl(userInfo.value.avatar)
  } catch (error) {
    return getDefaultAvatarUrl()
  }
}

// 格式化日期
const formatDate = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    timeZone: 'Asia/Shanghai' // 设置为北京时间
  })
}

// 获取角色样式类
const getRoleClass = (role) => {
  switch (role) {
    case 'admin':
      return 'bg-red-100 text-red-800'
    case 'user':
      return 'bg-green-100 text-green-800'
    case 'vip':
      return 'bg-yellow-100 text-yellow-800'
    default:
      return 'bg-gray-100 text-gray-800'
  }
}

const getGenderText = (gender) => {
  if (!gender) return '未设置'
  const m = { male: '男', female: '女', other: '其他', 男: '男', 女: '女', 其他: '其他' }
  return m[gender] || '未设置'
}

// 获取角色文本
const getRoleText = (role) => {
  switch (role) {
    case 'admin':
      return '管理员'
    case 'user':
      return '普通用户'
    case 'vip':
      return 'VIP用户'
    default:
      return '未知角色'
  }
}

// 将用户表的 address 文本保存为标准收货地址
const saveUserAddressAsShipping = async () => {
  try {
    const addr = userInfo.value.address || ''
    if (!addr.trim()) {
      notifyInfo('当前未设置地址')
      return
    }
    await ensureRegionDataLoaded()
    const parsed = userService.parseAddressText(addr)
    const payload = {
      RecipientName: userInfo.value.name || '用户',
      PhoneNumber: userInfo.value.phone || '',
      Province: parsed.province || '',
      City: parsed.city || '',
      District: parsed.district || '',
      DetailAddress: parsed.detail || addr,
      PostalCode: null,
      IsDefault: true
    }
    const res = await userService.addAddress(payload)
    if (res.success) {
      notifySuccess('已保存为收货地址')
      const addressesRes = await userService.getAddresses()
      if (addressesRes.success) {
        addresses.value = addressesRes.data || []
      }
    } else {
      notifyError(res.data?.message || '保存失败')
    }
  } catch (e) {
    notifyError('保存失败')
  }
}

// 我的优惠券刷新
const refreshCoupons = async () => {
  try {
    const res = await couponService.myCoupons()
    const list = res?.data || res || []
    myCoupons.value = Array.isArray(list) ? list : []
  } catch (e) {
    myCoupons.value = []
  }
}
</script>

<style scoped>
.profile-container {
  min-height: calc(100vh - 64px);
  background-color: #f9fafb;
}
</style>
