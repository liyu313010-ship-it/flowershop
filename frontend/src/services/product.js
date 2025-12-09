import api from './api'

export const productService = {
  // 获取商品列表
  async getProducts(params = {}) {
    try {
      const response = await api.get('/Products', { params })
      return response
    } catch (error) {
      try {
        const response2 = await api.get('/products', { params })
        return response2
      } catch (e) {
        return []
      }
    }
  },

  // 获取商品详情
  async getProductById(id) {
    try {
      const response = await api.get(`/Products/${id}`)
      return response
    } catch (error) {
      throw error
    }
  },

  // 搜索/过滤商品（服务端过滤）
  async searchProducts(params = {}) {
    try {
      const response = await api.get('/Products/search', { params, silent: true })
      return response
    } catch (error) {
      try {
        const response2 = await api.get('/products/search', { params, silent: true })
        return response2
      } catch (e) {
        return { data: { items: [], totalCount: 0, page: 1, pageSize: params.PageSize || 12 } }
      }
    }
  },

  // 获取商品分类
  async getCategories() {
    try {
      const response = await api.get('/Categories')
      return response
    } catch (error) {
      throw error
    }
  },

  // 获取热门商品
  async getPopularProducts(limit = 8) {
    try {
      const response = await api.get('/Products/featured', {
        params: { limit }
      })
      return response
    } catch (error) {
      throw error
    }
  },

  // 获取推荐商品
  async getRecommendedProducts(limit = 8) {
    try {
      const response = await api.get('/Products/home', {
        params: { limit }
      })
      return response
    } catch (error) {
      throw error
    }
  },

  // 获取新品上架
  async getNewProducts(limit = 8) {
    try {
      const response = await api.get('/Products/home', {
        params: { limit }
      })
      return response
    } catch (error) {
      throw error
    }
  },

  // 根据分类获取商品
  async getProductsByCategory(category, params = {}) {
    try {
      const response = await api.get(`/Products/category/${category}`, { params })
      return response
    } catch (error) {
      try {
        const response2 = await api.get(`/products/category/${category}`, { params })
        return response2
      } catch (e) {
        return []
      }
    }
  },

  // 获取特色商品
  async getFeaturedProducts() {
    try {
      const response = await api.get('/Products/featured')
      return response
    } catch (error) {
      throw error
    }
  },

  // 获取首页商品（前4个）
  async getHomeProducts() {
    try {
      const response = await api.get('/Products/home', { silent: true })
      return response
    } catch (error) {
      throw error
    }
  },

  // 获取商品评价
  async getProductReviews(productId, params = {}) {
    try {
      const response = await api.get(`/ProductReview/product/${productId}`, { params })
      return response
    } catch (error) {
      throw error
    }
  },

  // 获取当前用户对指定产品的评价
  async getUserReviewForProduct(productId) {
    try {
      const response = await api.get(`/ProductReview/user/product/${productId}`)
      return response
    } catch (error) {
      throw error
    }
  },

  async getReviewById(reviewId) {
    try {
      const response = await api.get(`/ProductReview/${reviewId}`, { silent: true })
      return response
    } catch (error) {
      throw error
    }
  },
  
  // 获取所有评价（管理员功能）
  // 在短时间内对同一参数的并发调用进行去重，缓解 429
  async getAllReviews(params = {}, opts = {}) {
    const now = Date.now()
    const key = JSON.stringify(params || {})
    productService.__reviewsInflight = productService.__reviewsInflight || new Map()
    const entry = productService.__reviewsInflight.get(key)
    if (entry && (now - entry.ts) < 1500) {
      return entry.promise
    }
    const p = api.get(`/ProductReview/all`, { params, silent: true, signal: opts.signal }).finally(() => {
      // 仅在完成后清理，避免内存泄漏
      setTimeout(() => {
        productService.__reviewsInflight && productService.__reviewsInflight.delete(key)
      }, 0)
    })
    productService.__reviewsInflight.set(key, { promise: p, ts: now })
    return p
  },

  // 添加商品评价
  async addProductReview(productId, reviewData) {
    try {
      // 确保数据格式符合后端要求，使用大写开头的属性名
      const formattedData = {
        ProductId: productId,
        Rating: reviewData.rating,
        Comment: reviewData.comment
      }
      const response = await api.post(`/ProductReview`, formattedData)
      return response
    } catch (error) {
      throw error
    }
  },

  // 删除商品评价
  async deleteProductReview(reviewId) {
    try {
      const response = await api.delete(`/ProductReview/${reviewId}`)
      return response
    } catch (error) {
      throw error
    }
  },

  // 管理员API - 创建商品
  async createProduct(productData) {
    try {
      const response = await api.post('/Products', productData)
      return response
    } catch (error) {
      throw error
    }
  },

  // 管理员API - 更新商品
  async updateProduct(id, productData) {
    try {
      const response = await api.put(`/Products/${id}`, productData)
      return response
    } catch (error) {
      throw error
    }
  },

  // 管理员API - 删除商品
  async deleteProduct(id) {
    try {
      const response = await api.delete(`/Products/${id}`)
      return response
    } catch (error) {
      throw error
    }
  },

  // 管理员API - 获取商品详情
  async getAdminProductById(id) {
    try {
      const response = await api.get(`/admin/products/${id}`)
      return response
    } catch (error) {
      throw error
    }
  }
}
