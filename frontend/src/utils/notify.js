import { ElMessage } from 'element-plus'

const recentMessages = new Map()

const show = (type, message, duration) => {
  const text = String(message || '').trim()
  if (!text) return
  const now = Date.now()
  const last = recentMessages.get(`${type}:${text}`) || 0
  if (now - last < 1600) return
  recentMessages.set(`${type}:${text}`, now)
  ElMessage({ type, message: text, duration, grouping: true, showClose: true })
}

export const notifySuccess = (message) => show('success', message, 2000)
export const notifyError = (message) => show('error', message, 2500)
export const notifyInfo = (message) => show('info', message, 2000)
