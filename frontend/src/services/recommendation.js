import api from './api'
import { isUsableToken } from '@/utils/session'

export const recommendationService = {
  async getGlobal(limit = 8) {
    const res = await api.get('/Recommendations/global', { params: { limit }, silent: true })
    return res
  }
  ,async getUser(limit = 8) {
    if (!isUsableToken(localStorage.getItem('token'))) return this.getGlobal(limit)
    try {
      return await api.get('/Recommendations/user', { params: { limit }, silent: true })
    } catch (error) {
      if (error.response?.status === 401 || error.code === 'ERR_CANCELED') return this.getGlobal(limit)
      throw error
    }
  }
}
