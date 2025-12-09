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
    let status;
    if (typeof statusData === 'string') {
      // 处理字符串参数，如 'paid'、'delivered' 等
      status = statusData;
    } else {
      // 处理对象参数，如 { status: 'delivered' } 或 { Status: 'delivered' }
      status = statusData.status || statusData.Status || statusData;
    }
    
    const formattedData = {
      Status: status
    };
    const data = await api.put(`/Orders/${orderId}/status`, formattedData)
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
      const data = await api.put(`/Orders/${orderId}/payment`, paymentInfo)
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
      const data = await api.post(`/Orders/${orderId}/repay`)
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
      // 如果API调用失败，返回模拟数据
      return [
        {
          status: 'created',
          description: '订单创建',
          timestamp: new Date().toISOString()
        }
      ]
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
    try {
      const data = await api.put(`/Orders/${orderId}/shipping`, shippingInfo)
      return { success: true, data }
    } catch (error) {
      console.error('更新物流信息失败:', error)
      throw error
    }
  }
}

export default orderService
