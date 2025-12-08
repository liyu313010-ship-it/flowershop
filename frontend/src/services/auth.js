import api from './api'

export const authService = {
  // 用户登录
  async login(credentials) {
    try {
      return await api.post('/auth/login', credentials)
    } catch (error) {
      throw error
    }
  },

  // 用户注册 - 添加额外验证确保只有用户明确操作才会触发注册
  async register(userData) {
    try {
      // 验证用户数据完整性
      if (!userData || !userData.username || !userData.email || !userData.password) {
        throw new Error('注册失败：缺少必要的用户数据')
      }
      
      // 验证用户名和邮箱格式
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!emailRegex.test(userData.email)) {
        throw new Error('注册失败：邮箱格式不正确')
      }
      
      if (userData.username.length < 3) {
        throw new Error('注册失败：用户名长度不足')
      }
      
      if (userData.password.length < 6) {
        throw new Error('注册失败：密码长度不足')
      }
      
      // 添加请求来源标记，帮助后端识别是否为正常用户操作
      const enhancedUserData = {
        ...userData,
        requestSource: 'user_interface', // 明确标记为用户界面发起的请求
        timestamp: new Date().toISOString(), // 添加时间戳
        isAutomated: false // 明确标记为非自动请求
      }
      
      // console.log('用户注册请求验证通过，准备发送到服务器')
      return await api.post('/auth/register', enhancedUserData)
    } catch (error) {
      console.error('注册过程中发生错误:', error)
      throw error
    }
  },

  // 获取当前用户信息
  async getCurrentUser() {
    try {
      return await api.get('/auth/profile')
    } catch (error) {
      throw error
    }
  },

  // 刷新token
  async refreshToken() {
    try {
      return await api.post('/auth/refresh')
    } catch (error) {
      throw error
    }
  },

  // 退出登录
  async logout() {
    try {
      await api.post('/auth/logout')
    } catch (error) {
      // 即使退出登录失败，也清除本地token
    }
  },

  // 修改密码
  async changePassword(passwordData) {
    try {
      return await api.post('/auth/change-password', passwordData)
    } catch (error) {
      throw error
    }
  },

  // 忘记密码
  async forgotPassword(email) {
    try {
      return await api.post('/auth/forgot-password', { email })
    } catch (error) {
      throw error
    }
  },

  // 重置密码
  async resetPassword(resetData) {
    try {
      return await api.post('/auth/reset-password', resetData)
    } catch (error) {
      throw error
    }
  }
}