import api from './api'

export const recommendationService = {
  async getGlobal(limit = 8) {
    const res = await api.get('/Recommendations/global', { params: { limit } })
    return res
  }
  ,async getUser(limit = 8) {
    const res = await api.get('/Recommendations/user', { params: { limit } })
    return res
  }
}
