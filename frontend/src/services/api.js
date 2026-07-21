import axios from 'axios'
import { useUserStore } from '@/stores/user'
import { notifyError, notifyInfo } from '@/utils/notify'
import router from '@/router'

// 创建axios实例
const api = axios.create({
  baseURL: '/api', // 后端API基础URL
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 请求拦截器 - 添加认证token
api.interceptors.request.use(
  (config) => {
    if (typeof FormData !== 'undefined' && config.data instanceof FormData) {
      // 让浏览器自动生成 multipart boundary，不能沿用实例默认的 application/json。
      delete config.headers['Content-Type']
    }
    // 从localStorage直接获取token，避免在请求拦截器中创建store实例
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    // 只在开发环境输出请求日志
    if (import.meta.env.DEV) {
      console.log('[API请求]', {
        url: config.url,
        method: config.method,
        fullURL: (config.baseURL || '') + config.url,
        headers: { ...config.headers, Authorization: config.headers.Authorization ? 'Bearer ***' : undefined },
        data: config.data
      })
    }
    return config
  },
  (error) => {
    console.error('API请求错误:', error)
    return Promise.reject(error)
  }
)

// 响应拦截器 - 处理通用错误
api.interceptors.response.use(
  (response) => {
    // 只在开发环境输出响应日志
    if (import.meta.env.DEV) {
      console.log('[API响应]', {
        url: response.config.url,
        method: response.config.method,
        status: response.status,
        statusText: response.statusText,
        data: response.data
      })
    }
    return response.data
  },
  (error) => {
    const isSilent = !!(error && error.config && error.config.silent)
    // 只在开发环境输出详细的错误日志；静默请求不输出控制台错误
    if (import.meta.env.DEV && !isSilent) {
      console.error('[API错误]', {
        message: error.message,
        config: { ...error.config, headers: error.config?.headers ? { ...error.config.headers, Authorization: 'Bearer ***' } : undefined },
        responseStatus: error.response?.status,
        responseStatusText: error.response?.statusText,
        responseData: error.response?.data
      })
    }
    
    const userStore = useUserStore()
    
    if (error.response) {
      switch (error.response.status) {
        case 401:
          // 未授权：静默请求不触发跳转，避免导航中断导致的 ERR_ABORTED
          if (isSilent) {
            return Promise.reject(error)
          }
          userStore.logout()
          notifyInfo('请先登录')
          try {
            router.push({ path: '/auth', query: { redirect: window.location.pathname + window.location.search } })
          } catch {
            window.location.href = '/auth'
          }
          return Promise.reject(error)
        case 403:
          // 权限不足
          if (!isSilent) notifyError('权限不足')
          return Promise.reject(error)
        case 404:
          // 资源不存在
          if (!isSilent) notifyError('资源不存在')
          return Promise.reject(error)
        case 500:
          // 服务器错误 - 优化错误处理，避免首页刷新时弹错
          const serverError = new Error(error.response?.data?.message || '服务器内部错误')
          serverError.status = 500
          serverError.response = error.response
          serverError.data = error.response?.data
          
          if (!isSilent) notifyError(serverError.message)
          return Promise.reject(serverError)
        default:
          if (import.meta.env.DEV && !isSilent) {
            // 避免在导航导致的中断请求上报错误
            const msg = error.response.data?.message || '未知错误'
            // if (!/ERR_ABORTED/i.test(error.message || '')) {
            //   console.error('请求错误:', msg)
            // }
          }
    }
    } else if (error.request) {
      // 网络错误
      if (!isSilent && !/ERR_ABORTED/i.test(error.message || '')) notifyError('网络连接错误')
    } else {
      // 其他错误
      if (!isSilent) notifyError(error.message || '请求配置错误')
    }
    
    // 简单重试机制：网络错误或 500 时重试 2 次
    const config = error.config || {}
    config.__retryCount = config.__retryCount || 0
    const shouldRetry = (!error.response) || (error.response && error.response.status >= 500)
    if (shouldRetry && config.__retryCount < 2) {
      config.__retryCount += 1
      return new Promise((resolve) => setTimeout(resolve, 300 * config.__retryCount)).then(() => api(config))
    }
    return Promise.reject(error)
  }
)

export default api
