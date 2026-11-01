import { ElMessage } from 'element-plus'

export const notifySuccess = (message) => ElMessage({ type: 'success', message, duration: 2000 })
export const notifyError = (message) => ElMessage({ type: 'error', message, duration: 2500 })
export const notifyInfo = (message) => ElMessage({ type: 'info', message, duration: 2000 })

