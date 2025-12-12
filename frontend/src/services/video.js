import api from './api'

export const videoService = {
  async getHome() {
    return api.get('/Videos/home', { silent: true })
  },
  async getBySlot(slot) {
    return api.get(`/Videos/slot/${encodeURIComponent(slot)}`, { silent: true })
  },
  async upload(formData) {
    return api.post('/Videos/upload', formData, { headers: { 'Content-Type': 'multipart/form-data' } })
  }
}
