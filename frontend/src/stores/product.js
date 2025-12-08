import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { productService } from '@/services/product'
import { categoryService } from '@/services/category'

// 商品状态管理store
export const useProductStore = defineStore('product', () => {
  // 状态定义
  const products = ref([])
  const categories = ref([])
  const currentProduct = ref(null)
  const searchResults = ref([])
  const featuredProducts = ref([])
  const newArrivals = ref([])
  const hotProducts = ref([])
  const isLoading = ref(false)
  const error = ref('')
  const pagination = ref({
    page: 1,
    limit: 12,
    total: 0,
    totalPages: 0
  })
  const filters = ref({
    category: '',
    priceRange: '',
    sortBy: '',
    searchQuery: ''
  })

  // 计算属性
  const hasProducts = computed(() => products.value.length > 0)
  const hasSearchResults = computed(() => searchResults.value.length > 0)
  const filteredProducts = computed(() => {
    let result = [...products.value]
    
    if (filters.value.category) {
      result = result.filter(product => product.category === filters.value.category)
    }
    
    if (filters.value.priceRange) {
      const [min, max] = filters.value.priceRange.split('-').map(Number)
      result = result.filter(product => {
        const price = product.discountPrice || product.price
        return price >= min && price <= max
      })
    }
    
    if (filters.value.sortBy) {
      switch (filters.value.sortBy) {
        case 'price-asc':
          result.sort((a, b) => (a.discountPrice || a.price) - (b.discountPrice || b.price))
          break
        case 'price-desc':
          result.sort((a, b) => (b.discountPrice || b.price) - (a.discountPrice || a.price))
          break
        case 'name-asc':
          result.sort((a, b) => a.name.localeCompare(b.name))
          break
        case 'sales-desc':
          result.sort((a, b) => (b.sales || 0) - (a.sales || 0))
          break
      }
    }
    
    return result
  })

  // 获取商品列表
  const fetchProducts = async (params = {}) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await productService.getProducts(params)
      // API返回的是直接数组，不是包装对象
      products.value = Array.isArray(response) ? response : (response.products || [])
      pagination.value = response.pagination || pagination.value
      console.log('获取到的商品数据:', products.value)
      return response
    } catch (err) {
      console.error('Fetch products error:', err)
      error.value = err.response?.data?.message || '获取商品列表失败'
      products.value = []
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 获取商品详情
  const fetchProductDetail = async (productId) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await productService.getProductById(productId)
      currentProduct.value = response
      return response
    } catch (err) {
      console.error('Fetch product detail error:', err)
      error.value = err.response?.data?.message || '获取商品详情失败'
      currentProduct.value = null
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 搜索商品
  const searchProducts = async (query, params = {}) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await productService.searchProducts(query, params)
      searchResults.value = response.products || []
      return response
    } catch (err) {
      console.error('Search products error:', err)
      error.value = err.response?.data?.message || '搜索商品失败'
      searchResults.value = []
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 获取商品分类
  const fetchCategories = async () => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await categoryService.getCategories()
      console.log('获取到的分类数据:', response)
      categories.value = response || []
      console.log('设置到categories的数据:', categories.value)
      return response
    } catch (err) {
      // 开发环境显示警告而不是错误
      if (import.meta.env.DEV) {
        console.warn('使用默认分类数据（API暂时不可用）')
      }
      // 不设置error.value，避免UI上显示错误
      // 使用默认分类数据
      categories.value = [
        { id: 1, name: '玫瑰', icon: '🌹' },
        { id: 2, name: '百合', icon: '🌸' },
        { id: 3, name: '康乃馨', icon: '🌷' },
        { id: 4, name: '向日葵', icon: '🌻' },
        { id: 5, name: '郁金香', icon: '🌷' },
        { id: 6, name: '组合花束', icon: '💐' }
      ]
      return categories.value // 返回默认数据
    } finally {
      isLoading.value = false
    }
  }

  // 获取热门商品
  const fetchHotProducts = async (limit = 8) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await productService.getPopularProducts(limit)
      hotProducts.value = response.products || []
      return response
    } catch (err) {
      console.error('Fetch hot products error:', err)
      error.value = err.response?.data?.message || '获取热门商品失败'
      // 添加降级机制：使用默认热门商品数据
      console.log('API调用失败，使用默认热门商品数据')
      hotProducts.value = [
        {
          id: 1,
          name: '红玫瑰花束',
          description: '11枝精选红玫瑰，象征永恒的爱情',
          price: 299,
          originalPrice: 399,
          image: '/images/flower-bouquet-1.svg',
          isHot: true,
          isNew: false,
          rating: 4.8,
          sales: 1234
        },
        {
          id: 2,
          name: '粉色康乃馨',
          description: '温馨的粉色康乃馨，适合送给母亲',
          price: 199,
          originalPrice: 259,
          image: '/images/pink-carnation.svg',
          isHot: true,
          isNew: true,
          rating: 4.9,
          sales: 856
        },
        {
          id: 3,
          name: '向日葵花束',
          description: '阳光明媚的向日葵，带来正能量',
          price: 259,
          originalPrice: 329,
          image: '/images/sunflower.svg',
          isHot: true,
          isNew: false,
          rating: 4.7,
          sales: 987
        }
      ]
      return { products: hotProducts.value } // 返回默认数据而不是抛出错误
    } finally {
      isLoading.value = false
    }
  }

  // 获取推荐商品
  const fetchFeaturedProducts = async (limit = 8) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await productService.getFeaturedProducts(limit)
      featuredProducts.value = response.products || []
      return response
    } catch (err) {
      console.error('Fetch featured products error:', err)
      error.value = err.response?.data?.message || '获取推荐商品失败'
      // 添加降级机制：使用默认推荐商品数据
      console.log('API调用失败，使用默认推荐商品数据')
      featuredProducts.value = [
        {
          id: 4,
          name: '百合花花束',
          description: '纯洁优雅的百合花，寓意百年好合',
          price: 399,
          originalPrice: 499,
          image: '/images/lily.svg',
          isHot: false,
          isNew: true,
          rating: 4.9,
          sales: 765
        },
        {
          id: 5,
          name: '混合花束',
          description: '多种鲜花组合，色彩丰富',
          price: 299,
          originalPrice: 359,
          image: '/images/flower-bouquet-4.svg',
          isHot: true,
          isNew: true,
          rating: 4.8,
          sales: 623
        },
        {
          id: 6,
          name: '蓝色妖姬',
          description: '11枝蓝色妖姬，神秘而浪漫',
          price: 399,
          originalPrice: 499,
          image: '/images/flower-bouquet-2.svg',
          isHot: false,
          isNew: false,
          rating: 4.7,
          sales: 546
        }
      ]
      return { products: featuredProducts.value } // 返回默认数据而不是抛出错误
    } finally {
      isLoading.value = false
    }
  }

  // 获取新品
  const fetchNewArrivals = async (limit = 8) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await productService.getNewProducts(limit)
      newArrivals.value = response.products || []
      return response
    } catch (err) {
      console.error('Fetch new arrivals error:', err)
      error.value = err.response?.data?.message || '获取新品失败'
      // 添加降级机制：使用默认新品数据
      console.log('API调用失败，使用默认新品数据')
      newArrivals.value = [
        {
          id: 7,
          name: '粉色玫瑰',
          description: '19枝粉色玫瑰，表达浪漫爱意',
          price: 329,
          originalPrice: 399,
          image: '/images/flower-bouquet-5.svg',
          isHot: false,
          isNew: true,
          rating: 4.9,
          sales: 412
        },
        {
          id: 8,
          name: '满天星花束',
          description: '纯满天星，代表思念与关怀',
          price: 199,
          originalPrice: 259,
          image: '/images/flower-bouquet-6.svg',
          isHot: false,
          isNew: true,
          rating: 4.7,
          sales: 356
        },
        {
          id: 9,
          name: '郁金香花束',
          description: '10枝郁金香，优雅与高贵的象征',
          price: 349,
          originalPrice: 429,
          image: '/images/flower-bouquet-7.svg',
          isHot: true,
          isNew: true,
          rating: 4.8,
          sales: 289
        }
      ]
      return { products: newArrivals.value } // 返回默认数据而不是抛出错误
    } finally {
      isLoading.value = false
    }
  }

  // 获取分类商品
  const fetchProductsByCategory = async (categoryId, params = {}) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await categoryService.getProductsByCategory(categoryId, params)
      products.value = response || []
      pagination.value = response.pagination || pagination.value
      return response
    } catch (err) {
      console.error('Fetch products by category error:', err)
      error.value = err.response?.data?.message || '获取分类商品失败'
      products.value = []
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 获取商品评价
  const fetchProductReviews = async (productId, params = {}) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await productService.getProductReviews(productId, params)
      return response
    } catch (err) {
      console.error('Fetch product reviews error:', err)
      error.value = err.response?.data?.message || '获取商品评价失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 添加商品评价
  const addProductReview = async (productId, reviewData) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await productService.addProductReview(productId, reviewData)
      return response
    } catch (err) {
      console.error('Add product review error:', err)
      error.value = err.response?.data?.message || '添加评价失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 设置过滤器
  const setFilters = (newFilters) => {
    filters.value = { ...filters.value, ...newFilters }
  }

  // 清除过滤器
  const clearFilters = () => {
    filters.value = {
      category: '',
      priceRange: '',
      sortBy: '',
      searchQuery: ''
    }
  }

  // 重置状态
  const resetState = () => {
    products.value = []
    categories.value = []
    currentProduct.value = null
    searchResults.value = []
    featuredProducts.value = []
    newArrivals.value = []
    hotProducts.value = []
    isLoading.value = false
    error.value = ''
    pagination.value = {
      page: 1,
      limit: 12,
      total: 0,
      totalPages: 0
    }
    clearFilters()
  }

  // 创建商品
  const createProduct = async (productData) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await productService.createProduct(productData)
      // 将新商品添加到列表中
      products.value.unshift(response)
      return response
    } catch (err) {
      console.error('Create product error:', err)
      error.value = err.response?.data?.message || '创建商品失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 更新商品
  const updateProduct = async (productId, productData) => {
    isLoading.value = true
    error.value = ''

    try {
      const response = await productService.updateProduct(productId, productData)
      // 更新商品列表中的对应项
      const index = products.value.findIndex(p => p.id === productId)
      if (index !== -1) {
        products.value[index] = { ...products.value[index], ...response }
      }
      return response
    } catch (err) {
      console.error('Update product error:', err)
      error.value = err.response?.data?.message || '更新商品失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  // 删除商品
  const deleteProduct = async (productId) => {
    isLoading.value = true
    error.value = ''

    try {
      await productService.deleteProduct(productId)
      // 从商品列表中移除
      const index = products.value.findIndex(p => p.id === productId)
      if (index !== -1) {
        products.value.splice(index, 1)
      }
      return true
    } catch (err) {
      console.error('Delete product error:', err)
      error.value = err.response?.data?.message || '删除商品失败'
      throw err
    } finally {
      isLoading.value = false
    }
  }

  return {
    // 状态
    products,
    categories,
    currentProduct,
    searchResults,
    featuredProducts,
    newArrivals,
    hotProducts,
    isLoading,
    error,
    pagination,
    filters,

    // 计算属性
    hasProducts,
    hasSearchResults,
    filteredProducts,

    // 方法
    fetchProducts,
    fetchProductDetail,
    searchProducts,
    fetchCategories,
    fetchHotProducts,
    fetchFeaturedProducts,
    fetchNewArrivals,
    fetchProductsByCategory,
    fetchProductReviews,
    addProductReview,
    createProduct,
    updateProduct,
    deleteProduct,
    setFilters,
    clearFilters,
    resetState
  }
})