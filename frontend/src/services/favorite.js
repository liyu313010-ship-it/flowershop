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
    return response
  },
  async list(page = 1, pageSize = 20) {
    try {
      const token = localStorage.getItem('token')
      if (!token) return []
      const res = await api.get(`/Favorite/list`, { params: { page, pageSize }, silent: true })
      return res
    } catch (error) {
      return []
    }
  }
}
