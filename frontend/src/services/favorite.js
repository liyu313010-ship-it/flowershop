import api from './api'

export const favoriteService = {
  async add(productId) {
    const response = await api.post('/Favorite/add', { productId })
    return response
  },
  async remove(productId) {
    const response = await api.delete(`/Favorite/remove/${productId}`)
    return response
  },
  async check(productId) {
    const response = await api.get(`/Favorite/check/${productId}`)
    // 后端当前返回 PascalCase（IsFavorited），统一成前端使用的字段，
    // 同时保留原始响应以兼容旧页面。
    const isFavorite = response?.isFavorite ?? response?.IsFavorited ?? false
    return { ...response, isFavorite, IsFavorited: isFavorite }
  },
  async list(page = 1, pageSize = 20) {
    try {
      const token = localStorage.getItem('token')
      if (!token) return []
      const res = await api.get(`/Favorite/list`, { params: { page, pageSize }, silent: true })
      // api 拦截器已返回 response.data，这里不能再读取 res.data。
      // 兼容后端 PascalCase 与 camelCase 两种序列化配置。
      return res?.favorites ?? res?.Favorites ?? res?.data ?? []
    } catch (error) {
      return []
    }
  }
}
