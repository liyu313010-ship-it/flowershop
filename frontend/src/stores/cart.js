import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { cartService } from '@/services/cart'

// 购物车状态管理store
export const useCartStore = defineStore('cart', () => {
  // 状态定义
  const cartItems = ref([])
  const isLoading = ref(false)
  const error = ref('')
  
  // 计算属性
  const cartCount = computed(() => {
    return cartItems.value.reduce((total, item) => total + item.quantity, 0)
  })

  const cartTotal = computed(() => {
    return cartItems.value.reduce((total, item) => {
      const price = item.price || 0
      return total + (price * item.quantity)
    }, 0)
  })

  const isEmpty = computed(() => cartItems.value.length === 0)

  // 向后兼容的计算属性
  const items = computed(() => cartItems.value)
  const loading = computed(() => isLoading.value)
  const itemCount = computed(() => cartCount.value)
  const totalPrice = computed(() => cartTotal.value)
  const formattedTotalPrice = computed(() => `¥${totalPrice.value.toFixed(2)}`)
  
  // 初始化购物车（向后兼容方法）
  const initializeCart = async () => {
    await fetchCart()
  }
  
  // 获取购物车列表
  const fetchCart = async () => {
    isLoading.value = true
    error.value = ''

    try {
      // 仅在开发环境输出日志
      if (import.meta.env.DEV) {
        console.log('开始获取购物车数据...')
      }
      
      const response = await cartService.getCart()
      
      if (import.meta.env.DEV) {
        console.log('购物车数据获取成功:', response?.length || 0, '个商品')
      }
      
      // 确保cartItems始终是数组
      if (Array.isArray(response)) {
        cartItems.value = response
      } else if (response && response.items) {
        // 支持嵌套items结构
        cartItems.value = Array.isArray(response.items) ? response.items : []
      } else {
        cartItems.value = []
      }
      return { items: cartItems.value }
    } catch (err) {
      // 优化错误处理和日志输出
        if (import.meta.env.DEV) {
          console.warn('Fetch cart error:', err.message)
          console.warn('错误详情:', err.data || err.response?.data || '无详细信息')
          console.warn('错误状态:', err.status || err.response?.status)
      }
      
      // 统一错误信息处理
      const errorMessage = err.data?.message || err.response?.data?.message || err.message || '获取购物车失败'
      error.value = errorMessage
      
      // 添加降级机制：使用默认空购物车数据
      console.log('API调用失败，使用默认购物车数据')
      cartItems.value = [] // 空购物车作为默认状态
      
      if (import.meta.env.DEV) {
        ElMessage({ message: '使用默认空购物车', type: 'warning' })
      }
      
      return { items: cartItems.value } // 返回默认数据而不是抛出错误
    } finally {
      isLoading.value = false
    }
  }
  
  // 添加商品到购物车
  const addToCart = async (product, quantity = 1) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await cartService.addToCart(product.id, quantity)

      // 重新加载购物车以获取最新数据
      await fetchCart()

      return { success: true, message: '已添加到购物车' }
    } catch (err) {
      console.error('Add to cart error:', err)
      error.value = err.response?.data?.message || '添加失败，请重试'
      return { 
        success: false, 
        message: error.value
      }
    } finally {
      isLoading.value = false
    }
  }
  
  // 更新购物车商品数量
  const updateQuantity = async (itemId, quantity) => {
    if (quantity <= 0) {
      return removeFromCart(itemId)
    }

    isLoading.value = true
    error.value = ''

    try {
      await cartService.updateCartItem(itemId, quantity)

      // 重新加载购物车
      await fetchCart()

      return { success: true }
    } catch (err) {
      console.error('Update cart error:', err)
      error.value = err.response?.data?.message || '更新失败'
      return { success: false, message: error.value }
    } finally {
      isLoading.value = false
    }
  }
  
  // 从购物车移除商品
  const removeFromCart = async (itemId) => {
    isLoading.value = true
    error.value = ''

    try {
      await cartService.removeFromCart(itemId)
      
      // 重新加载购物车
      await fetchCart()

      return { success: true }
    } catch (err) {
      console.error('Remove from cart error:', err)
      error.value = err.response?.data?.message || '移除失败'
      return { success: false, message: error.value }
    } finally {
      isLoading.value = false
    }
  }
  
  // 清空购物车
  const clearCart = async () => {
    isLoading.value = true
    error.value = ''

    try {
      await cartService.clearCart()
      cartItems.value = []
      return { success: true }
    } catch (err) {
      console.error('Clear cart error:', err)
      error.value = err.response?.data?.message || '清空失败'
      return { success: false, message: error.value }
    } finally {
      isLoading.value = false
    }
  }
  
  return {
    // 状态
    cartItems,
    isLoading,
    error,
    
    // 计算属性
    cartCount,
    cartTotal,
    isEmpty,
    
    // 向后兼容的计算属性
    items,
    loading,
    itemCount,
    totalPrice,
    formattedTotalPrice,
    
    // 方法
    initializeCart,
    fetchCart,
    addToCart,
    updateQuantity,
    removeFromCart,
    clearCart
  }
})