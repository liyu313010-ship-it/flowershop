<template>
  <div class="min-h-screen bg-gradient-to-br from-pink-50 via-white to-purple-50 flex items-center justify-center px-4 py-12">
    <!-- 背景装饰 -->
    <div class="absolute inset-0 overflow-hidden">
      <div class="absolute -top-40 -right-40 w-80 h-80 bg-pink-200 rounded-full opacity-20 blur-3xl"></div>
      <div class="absolute -bottom-40 -left-40 w-80 h-80 bg-purple-200 rounded-full opacity-20 blur-3xl"></div>
    </div>

    <div class="relative w-full max-w-md">
      <!-- Logo和标题 -->
      <div class="text-center mb-8">
        <div class="inline-flex items-center justify-center w-20 h-20 bg-transparent rounded-full mb-4 shadow-lg">
          <img src="/images/logo.png" alt="logo" class="w-10 h-10 object-contain" />
        </div>
        <h1 class="text-3xl font-bold text-gray-800 mb-2">欢雨flower</h1>
        <p class="text-gray-600">您的专属花店</p>
      </div>

      <!-- 登录/注册表单容器 -->
      <div class="bg-white/80 backdrop-blur-sm rounded-2xl shadow-xl p-8 border border-white/50 card-beauty">
        <!-- 标签切换 -->
        <div class="flex mb-6 bg-gray-100 rounded-lg p-1">
          <button
            @click="activeTab = 'login'"
            :class="[
              'flex-1 py-2 px-4 rounded-md text-sm font-medium transition-all duration-200',
              activeTab === 'login' 
                ? 'bg-white text-pink-600 shadow-sm' 
                : 'text-gray-600 hover:text-gray-800'
            ]"
          >
            登录
          </button>
          <button
            @click="activeTab = 'register'"
            :class="[
              'flex-1 py-2 px-4 rounded-md text-sm font-medium transition-all duration-200',
              activeTab === 'register' 
                ? 'bg-white text-pink-600 shadow-sm' 
                : 'text-gray-600 hover:text-gray-800'
            ]"
          >
            注册
          </button>
        </div>

        <!-- 登录表单 -->
        <form v-if="activeTab === 'login'" @submit.prevent="handleLogin" class="space-y-4">
          <!-- 用户类型选择 -->
          <div class="mb-4">
            <label class="block text-sm font-medium text-gray-700 mb-2">用户类型</label>
            <div class="grid grid-cols-2 gap-2">
              <button
                type="button"
                @click="loginType = 'user'"
                :class="[
                  'py-2 px-4 rounded-lg border-2 text-sm font-medium transition-all duration-200 hover:scale-[1.02] hover:shadow-sm',
                  loginType === 'user'
                    ? 'border-pink-500 bg-pink-50 text-pink-700'
                    : 'border-gray-200 text-gray-600 hover:border-gray-300'
                ]"
              >
                普通用户
              </button>
              <button
                type="button"
                @click="loginType = 'admin'"
                :class="[
                  'py-2 px-4 rounded-lg border-2 text-sm font-medium transition-all duration-200 hover:scale-[1.02] hover:shadow-sm',
                  loginType === 'admin'
                    ? 'border-pink-500 bg-pink-50 text-pink-700'
                    : 'border-gray-200 text-gray-600 hover:border-gray-300'
                ]"
              >
                管理员
              </button>
            </div>
          </div>

          <!-- 用户名 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">用户名</label>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
                </svg>
              </div>
              <input
                v-model="loginForm.username"
                type="text"
                required
                class="pl-10 w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 focus:border-transparent"
                placeholder="请输入用户名"
                @input="validateLoginField('username')"
                @blur="validateLoginField('username')"
                :class="{ 'border-red-500': validationErrors.login.username }"
              />
            </div>
            <span v-if="validationErrors.login.username" class="text-red-500 text-sm mt-1 block">
              {{ validationErrors.login.username }}
            </span>
          </div>

          <!-- 密码 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">密码</label>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
                </svg>
              </div>
              <input
                v-model="loginForm.password"
                :type="showPassword ? 'text' : 'password'"
                required
                class="pl-10 pr-10 w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 focus:border-transparent"
                placeholder="请输入密码"
                @input="validateLoginField('password')"
                @blur="validateLoginField('password')"
                :class="{ 'border-red-500': validationErrors.login.password }"
              />
              <button
                type="button"
                @click="showPassword = !showPassword"
                class="absolute inset-y-0 right-0 pr-3 flex items-center"
              >
                <svg v-if="showPassword" class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
                </svg>
                <svg v-else class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"/>
                </svg>
              </button>
            </div>
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">验证码</label>
            <div class="flex items-center space-x-3">
              <div class="px-3 py-2 rounded-lg bg-gray-100 text-gray-800 font-mono tracking-widest select-none captcha-box">
                {{ captchaCode }}
              </div>
              <button type="button" @click="refreshCaptcha" class="px-3 py-2 border rounded hover:bg-gray-50 text-sm">刷新</button>
            </div>
            <input
              v-model="captchaInput"
              type="text"
              required
              class="mt-2 w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 focus:border-transparent"
              placeholder="请输入上方验证码"
            />
          </div>

          <!-- 记住我和忘记密码 -->
          <div class="flex items-center justify-between">
            <label class="flex items-center">
              <input type="checkbox" v-model="rememberMe" class="rounded border-gray-300 text-pink-600 focus:ring-pink-500">
              <span class="ml-2 text-sm text-gray-600">记住我</span>
            </label>
            <a href="#" class="text-sm text-pink-600 hover:text-pink-700">忘记密码？</a>
          </div>

          <!-- 登录按钮 -->
          <button
            type="submit"
            :disabled="loading"
            class="w-full bg-gradient-to-r from-pink-500 to-pink-600 text-white py-2 px-4 rounded-lg font-medium hover:from-pink-600 hover:to-pink-700 focus:outline-none focus:ring-2 focus:ring-pink-500 focus:ring-offset-2 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed btn-soft"
          >
            <span v-if="loading" class="flex items-center justify-center">
              <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              登录中...
            </span>
            <span v-else>登录</span>
          </button>
        </form>

        <!-- 注册表单 -->
        <form v-if="activeTab === 'register'" @submit.prevent="handleRegister" class="space-y-4">
          <!-- 用户名 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">用户名</label>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
                </svg>
              </div>
              <input
                v-model="registerForm.username"
                type="text"
                required
                class="pl-10 w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 focus:border-transparent"
                placeholder="请输入用户名"
                @input="validateRegisterField('username')"
                @blur="validateRegisterField('username')"
                :class="{ 'border-red-500': validationErrors.register.username }"
              />
            </div>
            <span v-if="validationErrors.register.username" class="text-red-500 text-sm mt-1 block">
              {{ validationErrors.register.username }}
            </span>
          </div>

          <!-- 邮箱 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">邮箱</label>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/>
                </svg>
              </div>
              <input
                v-model="registerForm.email"
                type="email"
                required
                class="pl-10 w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 focus:border-transparent"
                placeholder="请输入邮箱"
                @input="validateRegisterField('email')"
                @blur="validateRegisterField('email')"
                :class="{ 'border-red-500': validationErrors.register.email }"
              />
            </div>
            <span v-if="validationErrors.register.email" class="text-red-500 text-sm mt-1 block">
              {{ validationErrors.register.email }}
            </span>
          </div>

          <!-- 密码 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">密码</label>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
                </svg>
              </div>
              <input
                v-model="registerForm.password"
                :type="showRegisterPassword ? 'text' : 'password'"
                required
                class="pl-10 pr-10 w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 focus:border-transparent"
                placeholder="请输入密码（至少6位）"
                @input="validateRegisterField('password')"
                @blur="validateRegisterField('password')"
                :class="{ 'border-red-500': validationErrors.register.password }"
              />
              <button
                type="button"
                @click="showRegisterPassword = !showRegisterPassword"
                class="absolute inset-y-0 right-0 pr-3 flex items-center"
              >
                <svg v-if="showRegisterPassword" class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
                </svg>
                <svg v-else class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"/>
                </svg>
              </button>
            </div>
            <span v-if="validationErrors.register.password" class="text-red-500 text-sm mt-1 block">
              {{ validationErrors.register.password }}
            </span>
            <!-- 密码强度指示器 -->
            <div v-if="registerForm.password" class="mt-2">
              <div class="flex items-center justify-between mb-1">
                <span class="text-xs text-gray-600">密码强度</span>
                <span class="text-xs" :class="passwordStrength.class">{{ passwordStrength.text }}</span>
              </div>
              <div class="w-full bg-gray-200 rounded-full h-2">
                <div 
                  class="h-2 rounded-full transition-all duration-300" 
                  :class="passwordStrength.class"
                  :style="{ width: passwordStrength.percentage + '%' }"
                ></div>
              </div>
            </div>
          </div>

          <!-- 确认密码 -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">确认密码</label>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
                </svg>
              </div>
              <input
                v-model="registerForm.confirmPassword"
                :type="showConfirmPassword ? 'text' : 'password'"
                required
                class="pl-10 pr-10 w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 focus:border-transparent"
                placeholder="请再次输入密码"
                @input="validateRegisterField('confirmPassword')"
                @blur="validateRegisterField('confirmPassword')"
                :class="{ 'border-red-500': validationErrors.register.confirmPassword }"
              />
              <button
                type="button"
                @click="showConfirmPassword = !showConfirmPassword"
                class="absolute inset-y-0 right-0 pr-3 flex items-center"
              >
                <svg v-if="showConfirmPassword" class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
                </svg>
                <svg v-else class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21"/>
                </svg>
              </button>
            </div>
            <span v-if="validationErrors.register.confirmPassword" class="text-red-500 text-sm mt-1 block">
              {{ validationErrors.register.confirmPassword }}
            </span>
          </div>

          <!-- 手机号（可选） -->
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">手机号（可选）</label>
            <div class="relative">
              <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg class="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"/>
                </svg>
              </div>
              <input
                v-model="registerForm.phone"
                type="tel"
                class="pl-10 w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-pink-500 focus:border-transparent"
                placeholder="请输入手机号"
                @input="validateRegisterField('phone')"
                @blur="validateRegisterField('phone')"
                :class="{ 'border-red-500': validationErrors.register.phone }"
              />
            </div>
            <span v-if="validationErrors.register.phone" class="text-red-500 text-sm mt-1 block">
              {{ validationErrors.register.phone }}
            </span>
          </div>

          <!-- 注册按钮 -->
          <button
            type="submit"
            :disabled="loading"
            class="w-full bg-gradient-to-r from-pink-500 to-pink-600 text-white py-2 px-4 rounded-lg font-medium hover:from-pink-600 hover:to-pink-700 focus:outline-none focus:ring-2 focus:ring-pink-500 focus:ring-offset-2 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <span v-if="loading" class="flex items-center justify-center">
              <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              注册中...
            </span>
            <span v-else>注册</span>
          </button>
        </form>

        <!-- 错误提示 -->
        <div v-if="error" class="mt-4 p-3 bg-red-50 border border-red-200 rounded-lg">
          <p class="text-sm text-red-600">{{ error }}</p>
        </div>

        <!-- 成功提示 -->
        <div v-if="success" class="mt-4 p-3 bg-green-50 border border-green-200 rounded-lg">
          <p class="text-sm text-green-600">{{ success }}</p>
        </div>
      </div>

      <!-- 返回首页链接 -->
      <div class="text-center mt-6">
        <router-link 
          to="/" 
          class="inline-flex items-center text-pink-600 hover:text-pink-700 text-sm font-medium"
        >
          <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18"/>
          </svg>
          返回首页
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useUserStore } from '@/stores/user'
import { 
  validateLoginForm, 
  validateRegisterForm, 
  getPasswordStrength 
} from '@/utils/validation'

const router = useRouter()
const userStore = useUserStore()
const loginLogoUrl = ref('/images/login-logo.png')

// 响应式数据
const activeTab = ref('login') // login 或 register
const loginType = ref('user') // user 或 admin
const showPassword = ref(false)
const showRegisterPassword = ref(false)
const showConfirmPassword = ref(false)
const loading = ref(false)
const error = ref('')
const success = ref('')

// 表单验证错误
const validationErrors = reactive({
  login: {},
  register: {}
})

// 登录表单数据
const loginForm = reactive({
  username: '',
  password: ''
})
const rememberMe = ref(false)
const captchaCode = ref('')
const captchaInput = ref('')

const refreshCaptcha = () => {
  const digits = '123456789'
  const letters = 'ABCDEFGHJKLMNPQRSTUVWXYZ'
  const pick = (s) => s[Math.floor(Math.random() * s.length)]
  const arr = [pick(digits), pick(digits), pick(letters), pick(letters)]
  for (let i = arr.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1))
    ;[arr[i], arr[j]] = [arr[j], arr[i]]
  }
  captchaCode.value = arr.join('')
  captchaInput.value = ''
}

// 注册表单数据
const registerForm = reactive({
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
  phone: ''
})

// 计算密码强度
const passwordStrength = computed(() => {
  return getPasswordStrength(registerForm.password)
})

// 实时验证登录表单
const validateLoginField = (field) => {
  const result = validateLoginForm(loginForm)
  if (!result.isValid) {
    validationErrors.login[field] = result.message
  } else {
    delete validationErrors.login[field]
  }
}

// 实时验证注册表单
const validateRegisterField = (field) => {
  const result = validateRegisterForm(registerForm)
  if (!result.isValid) {
    validationErrors.register[field] = result.message
  } else {
    delete validationErrors.register[field]
  }
}

// 清除验证错误
const clearValidationErrors = () => {
  Object.keys(validationErrors.login).forEach(key => {
    delete validationErrors.login[key]
  })
  Object.keys(validationErrors.register).forEach(key => {
    delete validationErrors.register[key]
  })
  error.value = ''
}

// 处理登录
const handleLogin = async () => {
  // 清除之前的错误
  clearValidationErrors()
  
  // 表单验证
  const validation = validateLoginForm(loginForm)
  if (!validation.isValid) {
    error.value = validation.message
    return
  }
  const inCode = (captchaInput.value || '').trim().toUpperCase()
  const okCode = (captchaCode.value || '').trim().toUpperCase()
  if (!inCode || inCode !== okCode) {
    error.value = '验证码错误'
    refreshCaptcha()
    return
  }
  
  loading.value = true
  
  try {
    // 调用登录API，包含用户类型信息
    const result = await userStore.login({
      username: loginForm.username,
      password: loginForm.password,
      role: loginType.value // 添加用户类型
    })
    
    if (result.success) {
      success.value = '登录成功，正在跳转...'
      if (rememberMe.value) {
        try {
          if (loginType.value === 'admin') {
            localStorage.setItem('remember_admin', '1')
            localStorage.setItem('remember_username_admin', loginForm.username)
          } else {
            localStorage.setItem('remember_user', '1')
            localStorage.setItem('remember_username_user', loginForm.username)
          }
        } catch {}
      } else {
        try {
          if (loginType.value === 'admin') {
            localStorage.removeItem('remember_admin')
            localStorage.removeItem('remember_username_admin')
          } else {
            localStorage.removeItem('remember_user')
            localStorage.removeItem('remember_username_user')
          }
        } catch {}
      }
      
      // 跳转逻辑：优先回跳redirect，其次按角色跳转
      setTimeout(() => {
        const redirect = router.currentRoute.value?.query?.redirect
        if (redirect && typeof redirect === 'string') {
          router.push(redirect)
          return
        }
        if (result.data?.user?.role === 'admin') {
          router.push('/admin')
        } else {
          router.push('/profile')
        }
      }, 1500)
    } else {
      error.value = result.message || '登录失败，请检查用户名和密码'
      refreshCaptcha()
    }
  } catch (err) {
    // 处理不同类型的错误
    if (err.response?.status === 401) {
      error.value = '用户名或密码错误'
    } else if (err.response?.status === 403) {
      error.value = '账户已被禁用，请联系管理员'
    } else if (err.response?.status === 429) {
      error.value = '登录尝试次数过多，请稍后再试'
    } else if (err.code === 'NETWORK_ERROR') {
      error.value = '网络连接失败，请检查网络设置'
    } else {
      error.value = '登录失败，请稍后重试'
    }
    console.error('Login error:', err)
    refreshCaptcha()
  } finally {
    loading.value = false
  }
}

// 初始化记住我与默认类型根据路由
try {
  const path = router.currentRoute.value.path || ''
  if (path.startsWith('/admin/login')) {
    loginType.value = 'admin'
    const savedAdminUser = localStorage.getItem('remember_username_admin')
    const savedAdminFlag = localStorage.getItem('remember_admin')
    if (savedAdminUser) { loginForm.username = savedAdminUser }
    if (savedAdminFlag) { rememberMe.value = true }
  } else {
    loginType.value = 'user'
    const savedUserUser = localStorage.getItem('remember_username_user')
    const savedUserFlag = localStorage.getItem('remember_user')
    if (savedUserUser) { loginForm.username = savedUserUser }
    if (savedUserFlag) { rememberMe.value = true }
  }
} catch {}

refreshCaptcha()

// 根据用户类型切换动态加载各自的“记住我”信息
watch(loginType, (type) => {
  try {
    if (type === 'admin') {
      const savedAdminUser = localStorage.getItem('remember_username_admin')
      const savedAdminFlag = localStorage.getItem('remember_admin')
      loginForm.username = savedAdminUser || ''
      rememberMe.value = !!savedAdminFlag
    } else {
      const savedUserUser = localStorage.getItem('remember_username_user')
      const savedUserFlag = localStorage.getItem('remember_user')
      loginForm.username = savedUserUser || ''
      rememberMe.value = !!savedUserFlag
    }
    // 切换类型时不保留密码，提升安全性
    loginForm.password = ''
  } catch {}
})

// 处理注册
const handleRegister = async () => {
  // 清除之前的错误
  clearValidationErrors()
  
  // 表单验证
  const validation = validateRegisterForm(registerForm)
  if (!validation.isValid) {
    error.value = validation.message
    return
  }
  
  loading.value = true
  
  try {
    // 调用注册API
    const result = await userStore.register({
      username: registerForm.username,
      email: registerForm.email,
      password: registerForm.password,
      phone: registerForm.phone,
      role: 'user' // 普通用户注册
    })
    
    if (result.success) {
      success.value = '注册成功！请使用注册的账号登录'
      
      // 清空注册表单
      Object.keys(registerForm).forEach(key => {
        registerForm[key] = ''
      })
      
      // 切换到登录标签
      setTimeout(() => {
        activeTab.value = 'login'
      }, 2000)
    } else {
      error.value = result.message || '注册失败，请稍后重试'
    }
  } catch (err) {
    // 处理不同类型的错误
    if (err.response?.status === 409) {
      const errorMessage = err.response.data.message
      if (errorMessage.includes('username')) {
        error.value = '用户名已存在，请选择其他用户名'
      } else if (errorMessage.includes('email')) {
        error.value = '邮箱已被注册，请使用其他邮箱'
      } else {
        error.value = '注册信息冲突，请检查后重试'
      }
    } else if (err.response?.status === 400) {
      error.value = '注册信息格式不正确，请检查后重试'
    } else if (err.code === 'NETWORK_ERROR') {
      error.value = '网络连接失败，请检查网络设置'
    } else {
      error.value = '注册失败，请稍后重试'
    }
    console.error('Register error:', err)
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.card-beauty {
  position: relative;
  overflow: hidden;
}
.card-beauty::before {
  content: '';
  position: absolute;
  inset: -1px;
  border-radius: 16px;
  padding: 1px;
  background: linear-gradient(135deg, rgba(236,72,153,0.35), rgba(147,51,234,0.25));
  -webkit-mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
  mask: linear-gradient(#000 0 0) content-box, linear-gradient(#000 0 0);
  -webkit-mask-composite: xor;
          mask-composite: exclude;
  pointer-events: none;
}
.card-beauty:hover {
  transform: translateY(-1px);
  box-shadow: 0 12px 28px rgba(236,72,153,0.12);
  will-change: transform, box-shadow;
}
.btn-soft {
  box-shadow: 0 6px 16px rgba(236,72,153,0.2);
}
.btn-soft:hover {
  box-shadow: 0 10px 24px rgba(236,72,153,0.28);
  transform: translateY(-0.5px);
}
.captcha-box {
  background-image: repeating-linear-gradient(45deg, rgba(0,0,0,0.04) 0 4px, transparent 4px 8px);
  letter-spacing: 0.25rem;
  text-shadow: 0 1px 0 rgba(255,255,255,0.4);
}
/* 自定义动画 */
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

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

@keyframes shake {
  0%, 100% {
    transform: translateX(0);
  }
  10%, 30%, 50%, 70%, 90% {
    transform: translateX(-2px);
  }
  20%, 40%, 60%, 80% {
    transform: translateX(2px);
  }
}

/* 玻璃效果 */
.glass-effect {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

/* 输入框样式 */
.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 12px;
  width: 20px;
  height: 20px;
  color: #9ca3af;
  z-index: 10;
}

.input-wrapper input {
  padding-left: 40px;
  width: 100%;
  padding: 12px 16px;
  border: 2px solid #e5e7eb;
  border-radius: 8px;
  font-size: 16px;
  transition: all 0.3s ease;
  background: white;
}

.input-wrapper input:focus {
  outline: none;
  border-color: #ec4899;
  box-shadow: 0 0 0 3px rgba(236, 72, 153, 0.1);
}

.input-wrapper input.error {
  border-color: #ef4444;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
  animation: shake 0.5s ease-in-out;
}

/* 密码切换按钮 */
.password-toggle {
  position: absolute;
  right: 12px;
  background: none;
  border: none;
  color: #9ca3af;
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  transition: all 0.3s ease;
}

.password-toggle:hover {
  color: #6b7280;
  background: rgba(107, 114, 128, 0.1);
}

/* 错误文本样式 */
.error-text {
  color: #ef4444;
  font-size: 14px;
  margin-top: 4px;
  display: block;
  animation: slideIn 0.3s ease-out;
}

/* 密码强度指示器 */
.password-strength {
  margin-top: 8px;
}

.strength-bar {
  width: 100%;
  height: 4px;
  background: #e5e7eb;
  border-radius: 2px;
  overflow: hidden;
  margin-bottom: 4px;
}

.strength-fill {
  height: 100%;
  transition: all 0.3s ease;
  border-radius: 2px;
}

.strength-fill.weak {
  background: #ef4444;
  width: 33.33%;
}

.strength-fill.medium {
  background: #f59e0b;
  width: 66.66%;
}

.strength-fill.strong {
  background: #10b981;
  width: 100%;
}

.strength-text {
  font-size: 12px;
  font-weight: 500;
}

.strength-text.weak {
  color: #ef4444;
}

.strength-text.medium {
  color: #f59e0b;
}

.strength-text.strong {
  color: #10b981;
}

/* 按钮悬停效果 */
.btn-hover {
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.btn-hover::before {
  content: '';
  position: absolute;
  top: 0;
  left: -100%;
  width: 100%;
  height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
  transition: left 0.5s ease;
}

.btn-hover:hover::before {
  left: 100%;
}

/* 标签页切换动画 */
.tab-content {
  animation: fadeIn 0.5s ease-out;
}

/* 加载动画 */
.loading-spinner {
  animation: spin 1s linear infinite;
}

/* 表单组动画 */
.form-group {
  animation: slideIn 0.5s ease-out;
  animation-fill-mode: both;
}

.form-group:nth-child(1) { animation-delay: 0.1s; }
.form-group:nth-child(2) { animation-delay: 0.2s; }
.form-group:nth-child(3) { animation-delay: 0.3s; }
.form-group:nth-child(4) { animation-delay: 0.4s; }
.form-group:nth-child(5) { animation-delay: 0.5s; }
.form-group:nth-child(6) { animation-delay: 0.6s; }

/* 响应式设计 */
@media (max-width: 640px) {
  .input-wrapper input {
    font-size: 16px; /* 防止iOS缩放 */
  }
  
  .form-group {
    animation-delay: 0s !important; /* 移动端移除延迟 */
  }
}

/* 焦点可访问性 */
.input-wrapper input:focus,
.password-toggle:focus,
button:focus {
  outline: 2px solid #ec4899;
  outline-offset: 2px;
}

/* 错误和成功消息动画 */
.error-message,
.success-message {
  animation: slideIn 0.3s ease-out;
}

/* 标签页按钮动画 */
.tab-button {
  position: relative;
  transition: all 0.3s ease;
}

.tab-button::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 50%;
  width: 0;
  height: 2px;
  background: #ec4899;
  transition: all 0.3s ease;
  transform: translateX(-50%);
}

.tab-button.active::after {
  width: 100%;
}

/* 复选框自定义样式 */
input[type="checkbox"] {
  width: 16px;
  height: 16px;
  accent-color: #ec4899;
  cursor: pointer;
}

/* 链接悬停效果 */
.link-hover {
  position: relative;
  transition: color 0.3s ease;
}

.link-hover::after {
  content: '';
  position: absolute;
  bottom: -2px;
  left: 0;
  width: 0;
  height: 1px;
  background: #ec4899;
  transition: width 0.3s ease;
}

.link-hover:hover::after {
  width: 100%;
}

/* 保持原有的样式 */
.relative {
  animation: fadeIn 0.6s ease-out;
}

/* 玻璃效果增强 */
.bg-white\/80 {
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
}

/* 按钮悬停效果 */
button {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

/* 输入框聚焦效果 */
input:focus {
  box-shadow: 0 0 0 3px rgba(236, 72, 153, 0.1);
}

/* 标签切换动画 */
.transition-all {
  transition-property: all;
  transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
}
</style>
