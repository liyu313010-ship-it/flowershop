import api from './api'

// 使用全局 api 实例（已统一 token 与错误处理）



// 订单相关API
const orderApi = {
  // 获取订单列表
  getOrders: async (params = {}) => {
    const response = await api.get('/admin/orders', { params })
    // 返回分页数据中的data字段
    return response.data
  },

  // 获取订单详情
  getAdminOrderById: async (orderId) => {
    const response = await api.get(`/admin/orders/${orderId}`)
    return response
  },

  // 更新订单状态
  updateOrderStatus: async (orderId, status) => {
    // 确保orderId是数字类型
    const numericOrderId = parseInt(orderId)
    if (isNaN(numericOrderId)) {
      throw new Error('无效的订单ID')
    }
    
    const response = await api.put(`/admin/orders/${numericOrderId}/status`, { status })
    // 明确返回成功标志，确保前端能正确识别
    return {
      success: true,
      status: response.status,
      data: response.data
    }
  },
  
  // 获取订单状态历史
  getOrderStatusHistory: async (orderId) => {
    try {
      // 确保orderId是数字类型
      const numericOrderId = parseInt(orderId)
      if (isNaN(numericOrderId)) {
        throw new Error('无效的订单ID')
      }
      const response = await api.get(`/OrderStatusHistory/order/${numericOrderId}`)
      return response.data
    } catch (error) {
      console.error('获取订单状态历史失败:', error)
      throw error
    }
  },

  // 发货
  shipOrder: async (orderId, shippingInfo) => {
    // 确保orderId是数字类型
    const numericOrderId = parseInt(orderId)
    if (isNaN(numericOrderId)) {
      throw new Error('无效的订单ID')
    }
    
    // 验证发货信息
    if (!shippingInfo || !shippingInfo.trackingNumber) {
      throw new Error('运单号不能为空')
    }
    
    const response = await api.put(`/admin/orders/${numericOrderId}/status`, { status: 'shipped', note: shippingInfo?.trackingNumber ? `发货: 物流单号 ${shippingInfo.trackingNumber}` : '' })
    // 明确返回成功标志，确保前端能正确识别
    return {
      success: true,
      status: response.status,
      data: response.data
    }
  },

  // 更新订单备注
  updateOrderNotes: async (orderId, notes) => {
    const response = await api.put(`/admin/orders/${orderId}/notes`, { notes })
    return response.data
  }
}

// 商品相关API
export const productApi = {
  // 获取商品列表
  getProducts: async (params = {}) => {
    const response = await api.get('/admin/products', { params })
    return response
  },

  // 创建商品
  createProduct: async (productData) => {
    const response = await api.post('/admin/products', productData)
    return response
  },

  // 更新商品
  updateProduct: async (productId, productData) => {
    const response = await api.put(`/admin/products/${productId}`, productData)
    return response
  },

  // 删除商品
  deleteProduct: async (productId) => {
    try {
      const response = await api.delete(`/admin/products/${productId}`)
      return response
    } catch (error) {
      throw error
    }
  },

  // 上传商品图片
  uploadProductImage: async (file) => {
    try {
      // 创建FormData对象
      const formData = new FormData()
      formData.append('file', file)
      
      // 发送图片上传请求到后端API
      const response = await api.post('/admin/upload-product-image', formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      })
      
      // 返回响应数据
      return response
    } catch (error) {
      console.error('上传商品图片失败:', error)
      throw error
    }
  }
}

// 用户相关API
export const userApi = {
  // 获取用户列表
  getUsers: async (params = {}) => {
    const response = await api.get('/admin/users', { params })
    return response
  }
}

// 数据统计相关API
export const statsApi = {
  // 获取仪表盘统计数据
  getDashboardStats: async () => {
    const response = await api.get('/admin/dashboard/stats', { silent: true })
    return response
  },

  // 获取销售统计数据
  getSalesStats: async (params = { days: 30 }) => {
    const response = await api.get(`/admin/dashboard/sales`, { params, silent: true })
    return response
  },

  // 获取商品销售排行
  getProductSalesRanking: async (limit = 10) => {
    const response = await api.get(`/admin/dashboard/product-ranking`, { params: { limit }, silent: true })
    return response
  },

  // 获取用户增长统计
  getUserGrowthStats: async (params = { days: 30 }) => {
    const response = await api.get(`/admin/dashboard/user-growth`, { params, silent: true })
    return response
  }
}

// 导出所有API
export default {
  ...orderApi,
  ...productApi,
  ...userApi,
  ...statsApi,
  // 确保uploadProductImage方法在默认导出中可用
  uploadProductImage: productApi.uploadProductImage,
  getAdminProducts: async (params = {}) => {
    const response = await api.get('/admin/products', { params, silent: true })
    return response
  },
  getAdminProductById: async (id) => {
    const response = await api.get(`/admin/products/${id}`)
    return response
  },
  getCategories: async () => {
    const response = await api.get('/Categories')
    return response
  },
  createCategory: async (data) => {
    const payload = {
      name: data.name,
      description: data.description || '',
      sortOrder: typeof data.sortOrder === 'number' ? data.sortOrder : 0,
      isActive: data.isActive !== false
    }
    const res = await api.post('/Categories', payload)
    return res
  },
  updateCategory: async (id, data) => {
    const payload = {
      name: data.name,
      description: data.description || '',
      sortOrder: typeof data.sortOrder === 'number' ? data.sortOrder : 0,
      isActive: data.isActive !== false
    }
    const res = await api.put(`/Categories/${id}`, payload)
    return res
  },
  deleteCategory: async (id) => {
    const res = await api.delete(`/Categories/${id}`)
    return res
  },
  // 优惠券管理
  getCoupons: async (params = {}) => {
    const res = await api.get('/Coupons', { params })
    return res
  },
  createCoupon: async (data) => {
    // 确保发送的数据是有效的 JSON 格式
    const validData = {
      Code: data.Code,
      DiscountType: data.DiscountType,
      Value: parseFloat(data.Value),
      MinOrderAmount: parseFloat(data.MinOrderAmount),
      UsageLimitPerUser: data.UsageLimitPerUser ? parseInt(data.UsageLimitPerUser) : null,
      UsageLimit: data.UsageLimit ? parseInt(data.UsageLimit) : null,
      Status: data.Status,
      StartAt: data.StartAt,
      EndAt: data.EndAt
    }
    const res = await api.post('/Coupons', validData)
    return res
  },
  updateCoupon: async (id, data) => {
    // 确保发送的数据是有效的 JSON 格式
    const validData = {
      Code: data.Code,
      DiscountType: data.DiscountType,
      Value: parseFloat(data.Value),
      MinOrderAmount: parseFloat(data.MinOrderAmount),
      UsageLimitPerUser: data.UsageLimitPerUser ? parseInt(data.UsageLimitPerUser) : null,
      UsageLimit: data.UsageLimit ? parseInt(data.UsageLimit) : null,
      Status: data.Status,
      StartAt: data.StartAt,
      EndAt: data.EndAt
    }
    const res = await api.put(`/Coupons/${id}`, validData)
    return res
  },
  deleteCoupon: async (id) => {
    const res = await api.delete(`/Coupons/${id}`)
    return res
  },
  getCouponClaims: async (id, params = {}) => {
    const res = await api.get(`/Coupons/${id}/claims`, { params })
    return res
  },
  getAdminUsers: async (params = {}) => {
    const response = await api.get('/admin/users', { params, silent: true })
    return response
  },
  getAdminUserById: async (id) => {
    const response = await api.get(`/admin/users/${id}`)
    return response
  },
  updateUserStatus: async (id, status) => {
    const response = await api.put(`/admin/users/${id}/status`, { status }, { silent: true })
    return response
  },
  updateUserRole: async (id, role) => {
    const response = await api.put(`/admin/users/${id}/role`, { role }, { silent: true })
    return response
  },
  resetUserPassword: async (id, newPassword = '123456') => {
    const response = await api.post(`/admin/users/${id}/reset-password`, { newPassword }, { silent: true })
    return response
  },
  deleteUser: async (id) => {
    const response = await api.delete(`/admin/users/${id}`)
    return response
  },
  getAdminOrders: async (params = {}) => {
    const response = await api.get('/admin/orders', { params })
    return response
  },
  updateOrderStatus: async (orderId, status, note = '') => {
    const numericOrderId = parseInt(orderId)
    if (isNaN(numericOrderId)) {
      throw new Error('无效的订单ID')
    }
    const payload = { status }
    if (note && note.trim()) { payload.note = note; payload.reason = note }
    const response = await api.put(`/admin/orders/${numericOrderId}/status`, payload)
    return response
  },
  getAdminOrderById: async (orderId) => {
    const response = await api.get(`/admin/orders/${orderId}`)
    return response
  },
  getLatestOrderHistory: async (orderId) => {
    const response = await api.get(`/admin/orders/${orderId}/history/latest`)
    return response
  },
  deleteAdminOrder: async (orderId) => {
    const response = await api.delete(`/admin/orders/${orderId}`)
    return response
  },
  deleteAdminUserAddress: async (userId, addressId) => {
    const response = await api.delete(`/admin/users/${userId}/addresses/${addressId}`)
    return response
  }
}
