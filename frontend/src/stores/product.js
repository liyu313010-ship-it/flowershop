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

  const normalizeCollection = (response) => {
    if (Array.isArray(response)) return response
    return response?.items || response?.Items || response?.products || response?.Products || response?.data?.items || response?.data?.Items || []
  }

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
      products.value = normalizeCollection(response)
      const page = response?.pagination || response?.Pagination || response?.data?.pagination
      if (page) pagination.value = page
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
      const searchParams = query && typeof query === 'object'
        ? query
        : { ...params, Search: query || undefined }
      const response = await productService.searchProducts(searchParams)
      searchResults.value = normalizeCollection(response)
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
      categories.value = []
      throw err
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
      hotProducts.value = normalizeCollection(response)
      return response
    } catch (err) {
      console.error('Fetch hot products error:', err)
      error.value = err.response?.data?.message || '获取热门商品失败'
      hotProducts.value = []
      throw err
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
      featuredProducts.value = normalizeCollection(response)
      return response
    } catch (err) {
      console.error('Fetch featured products error:', err)
      error.value = err.response?.data?.message || '获取推荐商品失败'
      featuredProducts.value = []
      throw err
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
      newArrivals.value = normalizeCollection(response)
      return response
    } catch (err) {
      console.error('Fetch new arrivals error:', err)
      error.value = err.response?.data?.message || '获取新品失败'
      newArrivals.value = []
      throw err
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
      products.value = normalizeCollection(response)
      const page = response?.pagination || response?.Pagination || response?.data?.pagination
      if (page) pagination.value = page
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
