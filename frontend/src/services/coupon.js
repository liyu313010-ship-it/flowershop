import api from './api'

export const couponService = {
  async getAvailable(minAmount = 0) {
    const res = await api.get('/Coupons/available', { params: { minAmount } })
    return res
  },
  async claim(couponId) {
    const res = await api.post(`/Coupons/claim/${couponId}`)
    return res
  },
  async validate(code, orderAmount) {
    const res = await api.post('/Coupons/validate', { code, orderAmount })
    return res
  },
  async grantToUser(userId, couponId) {
    const res = await api.post('/Coupons/grant', { userId, couponId })
    return res
  },
  async myCoupons() {
    const res = await api.get('/Coupons/my')
    return res
  }
}
