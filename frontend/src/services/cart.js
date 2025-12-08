import api from './api'

export const cartService = {
  // 获取购物车列表
  async getCart() {
    try {
      const response = await api.get('/Carts')
      return response
    } catch (error) {
      throw error
    }
  },

  // 添加商品到购物车
  async addToCart(productId, quantity = 1) {
    try {
      const response = await api.post('/Carts', {
        productId,
        quantity
      })
      return response
    } catch (error) {
      throw error
    }
  },

  // 更新购物车商品数量
  async updateCartItem(itemId, quantity) {
    try {
      const response = await api.put(`/Carts/${itemId}`, { quantity })
      return response
    } catch (error) {
      throw error
    }
  },

  // 删除购物车商品
  async removeFromCart(itemId) {
    try {
      const response = await api.delete(`/Carts/${itemId}`)
      return response
    } catch (error) {
      throw error
    }
  },

  // 清空购物车
  async clearCart() {
    try {
      const response = await api.delete('/Carts')
      return response
    } catch (error) {
      throw error
    }
  },

  // 计算购物车总价
  calculateCartTotal(cartItems) {
    return cartItems.reduce((total, item) => total + (item.totalPrice || item.price * item.quantity), 0)
  },

  // 计算购物车商品总数
  calculateCartItemCount(cartItems) {
    return cartItems.reduce((total, item) => total + item.quantity, 0)
  }
}
