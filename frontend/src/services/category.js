import api from './api'

export const categoryService = {
  // 获取所有分类
  async getCategories() {
    try {
      const response = await api.get('/Categories', { silent: true })
      return response
    } catch (error) {
      try {
        const response2 = await api.get('/Categories', { silent: true })
        return response2
      } catch (e) {
        return []
      }
    }
  },

  // 获取分类详情
  async getCategoryById(id) {
    try {
      const response = await api.get(`/Categories/${id}`)
      return response
    } catch (error) {
      throw error
    }
  },

  // 根据分类获取商品
  async getProductsByCategory(categoryId, params = {}) {
    try {
      const response = await api.get(`/products/category/${categoryId}`, { params })
      return response
    } catch (error) {
      throw error
    }
  },

  // 管理员API - 创建分类
  async createCategory(categoryData) {
    try {
      const response = await api.post('/Categories', categoryData)
      return response
    } catch (error) {
      throw error
    }
  },

  // 管理员API - 更新分类
  async updateCategory(id, categoryData) {
    try {
      const response = await api.put(`/Categories/${id}`, categoryData)
      return response
    } catch (error) {
      throw error
    }
  },

  // 管理员API - 删除分类
  async deleteCategory(id) {
    try {
      const response = await api.delete(`/Categories/${id}`)
      return response
    } catch (error) {
      throw error
    }
  }
}
