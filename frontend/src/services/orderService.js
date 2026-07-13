import api from './api'

const orderService = {
  // 获取用户订单列表
  async getUserOrders() {
    const data = await api.get('/Orders')
    return { success: true, data }
  },

  // 获取订单详情
  async getOrderById(orderId, options = {}) {
    const data = await api.get(`/Orders/${orderId}`, { ...options, silent: true })
    return { success: true, data }
  },

  // 创建订单
  async createOrder(orderData) {
    const data = await api.post('/Orders', orderData)
    return { success: true, data }
  },

  // 更新订单状态
  async updateOrderStatus(orderId, statusData) {
    // 确保状态数据格式正确，后端期望的是 { Status: 'delivered' } 格式
    const status = typeof statusData === 'string'
      ? statusData
      : (statusData?.status || statusData?.Status)
    if (!status) throw new Error('订单状态不能为空')
    const formattedData = {
      Status: status
    };
    const data = await api.put(`/Orders/${orderId}/status`, formattedData)
    return { success: true, data }
  },

  async confirmReceipt(orderId) {
    const data = await api.put(`/Orders/${orderId}/confirm-receipt`)
    return { success: true, data }
  },

  /**
   * 更新订单支付信息
   * @param {string} orderId - 订单ID
   * @param {Object} paymentInfo - 支付信息
   * @param {string} paymentInfo.paymentId - 支付ID
   * @param {string} paymentInfo.paymentMethod - 支付方式
   * @param {string} paymentInfo.paymentStatus - 支付状态
   * @returns {Promise} - 更新结果
   */
  async updatePaymentInfo(orderId, paymentInfo) {
    try {
      // 支付信息统一通过 payment-status 端点更新，/payment 并不存在。
      const data = await api.put(`/Orders/${orderId}/payment-status`, {
        paymentStatus: paymentInfo.paymentStatus ?? paymentInfo.PaymentStatus,
        paymentMethod: paymentInfo.paymentMethod ?? paymentInfo.PaymentMethod,
        paymentReference: paymentInfo.paymentReference ?? paymentInfo.PaymentReference
      })
      return { success: true, data }
    } catch (error) {
      console.error('更新支付信息失败:', error)
      throw error
    }
  },

  /**
   * 获取订单支付状态
   * @param {string} orderId - 订单ID
   * @returns {Promise} - 支付状态信息
   */
  async getOrderPaymentStatus(orderId) {
    try {
      const data = await api.get(`/Orders/${orderId}/payment-status`)
      return { success: true, data }
    } catch (error) {
      console.error('获取支付状态失败:', error)
      throw error
    }
  },

  /**
   * 重新支付订单
   * @param {string} orderId - 订单ID
   * @returns {Promise} - 支付链接信息
   */
  async repayOrder(orderId) {
    try {
      // 后端没有 /repay 路由；在线支付由服务端生成支付链接。
      const data = await api.get(`/Orders/${orderId}/payment-link`)
      return { success: true, data }
    } catch (error) {
      console.error('重新支付失败:', error)
      throw error
    }
  },

  // 取消订单
  async cancelOrder(orderId) {
    try {
      const data = await api.put(`/Orders/${orderId}/cancel`, null, { silent: true })
      return { success: true, data }
    } catch (error) {
      return { success: false, message: error.response?.data || error.message }
    }
  },

  // 获取所有订单（管理员）
  async getAllOrders() {
    const data = await api.get('/admin/orders')
    return { success: true, data }
  },

  /**
   * 获取订单状态历史
   * @param {string} orderId - 订单ID
   * @returns {Promise} - 订单状态历史记录
   */
  async getOrderStatusHistory(orderId, options = {}) {
    try {
      const data = await api.get(`/Orders/${orderId}/status-history`, { ...options, silent: true })
      return { success: true, data }
    } catch (error) {
      // 不伪造订单轨迹；由调用方决定如何展示错误和重试
      return { success: false, data: [], message: error.response?.data?.message || error.message || '获取订单状态失败' }
    }
  },

  // 更新支付状态（简化支付流程用）
  async updatePaymentStatus(orderId, { paymentStatus, paymentMethod = 'online', paymentReference = '' }) {
    const payload = { PaymentStatus: paymentStatus, PaymentMethod: paymentMethod, PaymentReference: paymentReference }
    const data = await api.put(`/Orders/${orderId}/payment-status`, payload)
    return { success: true, data }
  },

  /**
   * 更新订单物流信息
   * @param {string} orderId - 订单ID
   * @param {Object} shippingInfo - 物流信息
   * @returns {Promise} - 更新结果
   */
  async updateShippingInfo(orderId, shippingInfo) {
    const company = shippingInfo?.company || shippingInfo?.Company || ''
    const tracking = shippingInfo?.trackingNumber || shippingInfo?.TrackingNumber || ''
    if (!company.trim() || !tracking.trim()) throw new Error('物流公司和运单号不能为空')
    const data = await api.put(`/admin/orders/${orderId}/shipping`, {
      company: company.trim(),
      trackingNumber: tracking.trim()
    })
    return { success: true, data }
  },

  // 通过服务端生成支付单/支付链接，不在客户端伪造支付成功。
  async processPayment(orderId, paymentMethod = 'online') {
    const data = await api.post(`/Orders/${orderId}/process-payment`, { paymentMethod })
    return { success: true, data }
  },

  async generatePaymentLink(orderId) {
    const data = await api.get(`/Orders/${orderId}/payment-link`)
    return { success: true, data }
  },

  async verifyPayment(orderId, paymentReference) {
    const data = await api.post(`/Orders/${orderId}/verify-payment`, { paymentReference })
    return { success: true, data }
  }
}

export default orderService
