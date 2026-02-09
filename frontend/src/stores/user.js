import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { authService } from '@/services/auth'
import userService from '@/services/userService.js'

export const useUserStore = defineStore('user', () => {
  // 状态定义
  const user = ref(null)
  const token = ref(localStorage.getItem('token') || '')
  const isLoading = ref(false)
  const error = ref('')

  // 计算属性
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const isLoggedIn = computed(() => isAuthenticated.value) // 保持向后兼容
  const isAdmin = computed(() => user.value?.role === 'admin')
  const isUser = computed(() => ['user', 'vip'].includes(user.value?.role))
  const userName = computed(() => user.value?.username || '')

  // 初始化用户信息
  const initializeUser = () => {
    const storedUser = localStorage.getItem('user')
    if (storedUser && token.value) {
      try {
        user.value = JSON.parse(storedUser)
      } catch (e) {
        logout()
      }
    }
  }

  // 登录功能
  const login = async (credentials) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await authService.login({
        username: credentials.username,
        password: credentials.password
        // Remove role from payload since backend LoginDto doesn't support it
      })

      // 响应拦截器已经返回了response.data，所以直接从response中获取token和user
      const authToken = response.token || response.Token
      const userData = response.user || response.User

      if (!authToken || !userData) {
        throw new Error('登录响应格式错误，缺少token或user信息')
      }

      // 验证用户角色是否匹配：
      // 管理员登录必须是 admin；普通用户登录允许 user 或 vip
      if (credentials.role) {
        if (credentials.role === 'admin' && userData.role !== 'admin') {
          return { success: false, message: '该账号不是管理员账号' }
        }
        if (credentials.role === 'user' && !['user', 'vip'].includes(userData.role)) {
          return { success: false, message: '该账号不是普通用户账号' }
        }
      }

      token.value = authToken
      const mappedUser = {
        id: userData.Id || userData.id,
        username: userData.Username || userData.username,
        name: userData.FullName || userData.name || userData.Username || userData.username || '',
        email: userData.Email || userData.email || '',
        phone: userData.Phone || userData.phone || '',
        address: userData.Address || userData.address || '',
        avatar: userData.Avatar || userData.avatar || null,
        role: userData.Role || userData.role,
        status: userData.Status || userData.status,
        lastLoginAt: userData.LastLoginAt || userData.lastLoginAt || null
      }
      user.value = mappedUser
      localStorage.setItem('token', authToken)
      localStorage.setItem('user', JSON.stringify(mappedUser))

      return {
        success: true,
        message: '登录成功',
        data: response
      }
    } catch (err) {
      // 处理不同的错误响应
      if (err.response?.status === 401) {
        return {
          success: false,
          message: '用户名或密码错误'
        }
      } else if (err.response?.status === 403) {
        return {
          success: false,
          message: '你的账户被封禁，请联系管理员'
        }
      } else if (err.response?.status === 404) {
        return {
          success: false,
          message: '用户不存在'
        }
      } else {
        return {
          success: false,
          message: err.response?.data?.message || err.message || '登录失败，请稍后重试'
        }
      }
    } finally {
      isLoading.value = false
    }
  }

  // 注册功能
  const register = async (userData) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await authService.register({
        username: userData.username,
        email: userData.email,
        password: userData.password,
        phone: userData.phone || '',
        role: userData.role || 'user'
      })

      return {
        success: true,
        message: '注册成功',
        data: response
      }
    } catch (err) {
      // console.error('Register error:', err)
      
      // 处理不同的错误响应
      if (err.response?.status === 400) {
        const errorMessage = err.response.data.message
        if (errorMessage.includes('username')) {
          return {
            success: false,
            message: '用户名已存在'
          }
        } else if (errorMessage.includes('email')) {
          return {
            success: false,
            message: '邮箱已被注册'
          }
        } else {
          return {
            success: false,
            message: errorMessage || '注册信息有误'
          }
        }
      } else {
        return {
          success: false,
          message: err.response?.data?.message || '注册失败，请稍后重试'
        }
      }
    } finally {
      isLoading.value = false
    }
  }

  // 登出功能
  const logout = async () => {
    try {
      if (token.value) {
        await authService.logout()
      }
    } catch (err) {
      // console.error('Logout error:', err)
    } finally {
      // 清除本地状态
      token.value = ''
      user.value = null
      localStorage.removeItem('token')
      localStorage.removeItem('user')
    }
  }

  // 获取用户信息
  const fetchUserProfile = async () => {
    if (!token.value) {
      if (import.meta.env.DEV) {
        // console.warn('无Token，跳过获取用户信息')
      }
      return
    }

    isLoading.value = true
    error.value = ''

    try {
      if (import.meta.env.DEV) {
        // console.log('开始获取用户信息...')
      }
      
      const userData = await authService.getCurrentUser()
      
      if (import.meta.env.DEV) {
        // console.log('用户信息获取成功')
      }
      
      // 转换后端UserDto格式到前端user格式
      const mappedUser = {
        id: userData.Id || userData.id,
        username: userData.Username || userData.username,
        name: userData.FullName || userData.name || '',
        email: userData.Email || userData.email,
        phone: userData.Phone || userData.phone || '',
        address: userData.Address || userData.address || '',
        avatar: userData.Avatar || userData.avatar || null,
        role: userData.Role || userData.role,
        status: userData.Status || userData.status,
        lastLoginAt: userData.LastLoginAt || userData.lastLoginAt || null
      }
      
      user.value = mappedUser
      localStorage.setItem('user', JSON.stringify(mappedUser))
      return mappedUser
    } catch (err) {
      // 优化错误处理和日志输出
      if (import.meta.env.DEV) {
        // console.warn('Fetch profile error:', err.message)
        // console.warn('错误详情:', err.data || err.response?.data || '无详细信息')
        // console.warn('错误状态:', err.status || err.response?.status)
      }
      
      // 统一错误信息处理
      const errorMessage = err.data?.message || err.response?.data?.message || err.message || '获取用户信息失败'
      error.value = errorMessage
      
      // 添加降级机制：使用默认游客用户
      // console.log('API调用失败，使用默认游客用户')
      const defaultUser = {
        id: null,
        name: '游客',
        email: '',
        avatar: null,
        isGuest: true,  // 明确标记为游客用户
        isTemporary: true,  // 临时用户标记，防止被误注册
        preventRegistration: true,  // 防止注册的标记
        createdAt: new Date().toISOString()
      }
      user.value = defaultUser
      // 不保存游客用户到localStorage，防止下次加载时被误当作有效用户
      localStorage.removeItem('user')
      
      // 处理Token无效的情况
      if (err.status === 401 || err.response?.status === 401) {
        // Token无效，清除本地数据
        if (import.meta.env.DEV) {
          // console.log('Token无效，执行登出操作')
        }
        logout()
        ElMessage.error('登录已过期，请重新登录')
      } else if (import.meta.env.DEV) {
          // 在开发环境显示警告而不是错误
          ElMessage({ message: '使用临时游客模式，不会创建账号', type: 'warning' })
      }
      
      return defaultUser // 返回默认用户而不是抛出错误
    } finally {
      isLoading.value = false
    }
  }

  // 向后兼容的方法
  const fetchUserInfo = fetchUserProfile

  // 更新用户信息
  const updateProfile = async (profileData) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await userService.updateUserProfile(profileData)
      user.value = response
      localStorage.setItem('user', JSON.stringify(response))

      return {
        success: true,
        message: '更新成功',
        data: response
      }
    } catch (err) {
      // console.error('Update profile error:', err)
      error.value = err.response?.data?.message || '更新失败，请稍后重试'
      return {
        success: false,
        message: err.response?.data?.message || '更新失败，请稍后重试'
      }
    } finally {
      isLoading.value = false
    }
  }

  // 修改密码
  const changePassword = async (passwordData) => {
    isLoading.value = true
    error.value = ''

    try {
      await authService.changePassword(passwordData)

      return {
        success: true,
        message: '密码修改成功'
      }
    } catch (err) {
      // console.error('Change password error:', err)
      
      if (err.response?.status === 400) {
        return {
          success: false,
          message: '原密码错误'
        }
      } else {
        return {
          success: false,
          message: err.response?.data?.message || '密码修改失败，请稍后重试'
        }
      }
    } finally {
      isLoading.value = false
    }
  }

  // 检查用户名是否可用
  const checkUsernameAvailability = async (username) => {
    try {
      // 这里需要后端提供对应的API，暂时返回true
      return true
    } catch (err) {
      // console.error('Check username error:', err)
      return false
    }
  }

  // 检查邮箱是否可用
  const checkEmailAvailability = async (email) => {
    try {
      // 这里需要后端提供对应的API，暂时返回true
      return true
    } catch (err) {
      // console.error('Check email error:', err)
      return false
    }
  }

  // 检查认证状态
  const checkAuth = async () => {
    if (!token.value) return
    
    try {
      await fetchUserProfile()
    } catch (err) {
      // console.error('Check auth failed:', err)
      logout()
    }
  }

  // 初始化
  initializeUser()

  return {
    // 状态
    user,
    token,
    isLoading,
    error,
    loading: isLoading, // 向后兼容
    
    // 计算属性
    isAuthenticated,
    isLoggedIn,
    isAdmin,
    isUser,
    userName,
    
    // 方法
    login,
    register,
    logout,
    fetchUserProfile,
    fetchUserInfo, // 向后兼容
    updateProfile,
    changePassword,
    checkUsernameAvailability,
    checkEmailAvailability,
    checkAuth,
    initializeUser
  }
})
