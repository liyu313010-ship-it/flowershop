<template>
  <PageTransition>
    <div class="min-h-screen products-page">
      <!-- 页面加载动画 -->
      <LoadingSpinner v-if="isLoading" />
      
      <!-- 页面标题 -->
      <div class="page-heading">
        <div class="container mx-auto px-4 py-6">
          <div class="flex justify-between items-center">
            <div>
              <h1 class="text-2xl font-bold text-gray-800">全部商品</h1>
              <p class="text-gray-600 mt-2">精选优质鲜花，传递美好情感</p>
            </div>
            <button v-if="userStore.isAdmin" @click="handleAddProductClick" class="btn-primary">
              <i class="fas fa-plus mr-2"></i>添加商品
            </button>
          </div>
        </div>
      </div>

      <div class="container mx-auto px-4 py-8">
        <div class="grid grid-cols-1 lg:grid-cols-4 gap-8">
          <!-- 筛选侧边栏 -->
          <div class="lg:col-span-1">
            <button type="button" class="mobile-filter-trigger btn-secondary mb-3" @click="isFilterOpen = !isFilterOpen">
              {{ isFilterOpen ? '收起筛选' : '筛选花礼' }}
            </button>
            <div class="filter-panel p-6" :class="{ 'is-open': isFilterOpen }">
              <div class="flex items-center justify-between mb-4">
                <h2 class="filter-heading text-lg font-semibold">选一束心意</h2>
                <button type="button" class="text-sm text-[color:var(--brand-coral)]" @click="clearFilters">清除</button>
              </div>
              <div class="space-y-2">
                <label 
                  v-for="category in categories" 
                  :key="category.id"
                  class="flex items-center space-x-2 cursor-pointer"
                >
                  <input 
                    type="checkbox" 
                    class="rounded text-huanyu-pink-600"
                    v-model="selectedCategories"
                    :value="category.id"
                  />
                  <span>{{ category.name }}</span>
                </label>
              </div>

              <div class="mt-6">
                <h3 class="font-medium mb-3">价格区间</h3>
                <div class="space-y-2">
                  <label class="flex items-center space-x-2 cursor-pointer">
                    <input type="radio" name="price" class="text-huanyu-pink-600" value="0-100" v-model="selectedPriceRange" />
                    <span>0 - 100元</span>
                  </label>
                  <label class="flex items-center space-x-2 cursor-pointer">
                    <input type="radio" name="price" class="text-huanyu-pink-600" value="100-200" v-model="selectedPriceRange" />
                    <span>100 - 200元</span>
                  </label>
                  <label class="flex items-center space-x-2 cursor-pointer">
                    <input type="radio" name="price" class="text-huanyu-pink-600" value="200-500" v-model="selectedPriceRange" />
                    <span>200 - 500元</span>
                  </label>
                  <label class="flex items-center space-x-2 cursor-pointer">
                    <input type="radio" name="price" class="text-huanyu-pink-600" value="500+" v-model="selectedPriceRange" />
                    <span>500元以上</span>
                  </label>
                </div>
              </div>
            </div>
          </div>

          <!-- 商品列表 -->
          <div class="lg:col-span-3">
            <div class="products-panel p-6 mb-6">
              <div class="flex flex-wrap gap-3 justify-between items-center mb-6">
                <div class="text-gray-600">找到 {{ filteredProducts.length }} 个商品</div>
                <div class="flex items-center space-x-2">
                  <span class="text-gray-600 text-sm">排序方式：</span>
                  <select v-model="sortOrder" class="input-field !w-auto !py-2 !px-3 text-sm">
                    <option value="default">默认排序</option>
                    <option value="price-asc">价格从低到高</option>
                    <option value="price-desc">价格从高到低</option>
                    <option value="sales-desc">销量从高到低</option>
                  </select>
                </div>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                <div v-for="product in filteredProducts" :key="product.id" class="card group cursor-pointer relative">
                  <div class="relative overflow-hidden rounded-xl">
                    <div class="w-full h-64 bg-gray-50 overflow-hidden relative">
                      <img 
                        :src="product.image" 
                        :alt="product.name"
                        class="w-full h-full object-contain transition-transform duration-300 group-hover:scale-110"
                        @error="onProductImageError"
                      />
                    </div>
                    <div class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center space-x-2 z-30 pointer-events-auto">
                      <button 
                        @click.stop="quickView(product)"
                        class="w-10 h-10 bg-white rounded-full flex items-center justify-center hover:bg-huanyu-pink-50 transition-colors shadow-lg"
                        title="快速查看"
                      >
                        <svg class="w-5 h-5 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
                        </svg>
                      </button>
                      <button 
                        v-if="!userStore.isAdmin"
                        @click.stop="handleAddToCart(product)"
                        class="w-10 h-10 bg-huanyu-pink-500 text-white rounded-full flex items-center justify-center hover:bg-huanyu-pink-600 transition-colors shadow-lg"
                        title="加入购物车"
                      >
                        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
                        </svg>
                      </button>
                      <button 
                        @click.stop="toggleFavorite(product)"
                        class="w-10 h-10 bg-white rounded-full flex items-center justify-center hover:bg-huanyu-pink-50 transition-colors shadow-lg"
                        :class="isFavorite(product) ? 'text-red-500' : 'text-gray-700'"
                        title="收藏"
                      >
                        <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                          <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
                        </svg>
                      </button>
                    </div>
                  </div>
                  <div class="p-4">
                    <h3 class="font-semibold text-lg mb-2 text-gray-800 group-hover:text-huanyu-pink-600 transition-colors">{{ product.name }}</h3>
                    <p class="text-gray-600 text-sm mb-3 line-clamp-2">{{ product.description }}</p>
                    <div class="flex items-center justify-between mb-3">
                      <div class="flex items-center space-x-1">
                        <div class="flex text-yellow-400">
                          <svg v-for="i in 5" :key="i" class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
                            <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                          </svg>
                        </div>
                        <span class="text-sm text-gray-500">({{ (product.reviewCount || 0).toFixed ? (product.reviewCount || 0).toFixed(1) : (product.reviewCount || 0) }})</span>
                      </div>
                      <span class="text-sm text-gray-500">已售 {{ product.salesCount || 0 }}</span>
                    </div>
                    <div class="flex items-center justify-between">
                      <div>
                        <span class="text-xl font-bold text-huanyu-pink-500">¥{{ Math.round(product.price || 0) }}</span>
                        <span v-if="product.originalPrice" class="text-sm text-gray-400 line-through ml-2">¥{{ product.originalPrice }}</span>
                      </div>
                      <div class="flex items-center space-x-1">
                        <div v-if="showQuantitySelector[product.id]" class="flex items-center space-x-1 mr-2">
                          <button @click.stop="decreaseQuantity(product)" class="w-6 h-6 bg-gray-200 hover:bg-gray-300 rounded-full flex items-center justify-center transition-colors">
                            <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 12H4"></path>
                            </svg>
                          </button>
                          <span class="w-8 text-center text-sm font-medium">{{ getProductQuantity(product) }}</span>
                          <button @click.stop="increaseQuantity(product)" class="w-6 h-6 bg-gray-200 hover:bg-gray-300 rounded-full flex items-center justify-center transition-colors">
                            <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path>
                            </svg>
                          </button>
                        </div>
                        <button 
                          v-if="!userStore.isAdmin"
                          @click.stop="handleAddToCart(product)"
                          class="bg-huanyu-pink-400 hover:bg-huanyu-pink-500 text-white p-2 rounded-full transition-all transform hover:scale-110"
                          title="加入购物车"
                        >
                          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
                          </svg>
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <div v-if="filteredProducts.length === 0" class="brand-empty-state">
                <img src="/images/flower-empty.svg" alt="暂无花礼" class="brand-empty-state-icon" />
                <h3 class="text-lg font-medium text-gray-700 mb-2">还没有找到合适的花礼</h3>
                <p class="text-gray-500">{{ loadError ? errorMessage : '试试清除筛选，或者看看全部花礼。' }}</p>
                <div class="mt-5 flex flex-wrap justify-center gap-3">
                  <button @click="clearFilters" class="btn-secondary">清除筛选</button>
                  <button @click="loadAllData" class="btn-primary">重新加载</button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

  </PageTransition>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import PageTransition from '@/components/PageTransition.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import { productService } from '@/services/product'
import { getProductImageUrl } from '@/utils/avatar.js'
import { categoryService } from '@/services/category'
import { useCartStore } from '@/stores/cart'
import { useUserStore } from '@/stores/user'
import { notifySuccess, notifyError, notifyInfo } from '@/utils/notify'
import { favoriteService } from '@/services/favorite'
import { watchEffect } from 'vue'

const router = useRouter()
const route = useRoute()
const cartStore = useCartStore()
const userStore = useUserStore()
const favoriteProducts = ref([])

// 响应式数据
const isLoading = ref(false)
const loadError = ref(false)
const errorMessage = ref('')
const products = ref([])
const categories = ref([])
const selectedCategory = ref(null) // 保留旧逻辑
const selectedCategories = ref([]) // 新增多选分类
const selectedPriceRange = ref(null)
const sortOrder = ref('default')
const isFilterOpen = ref(false)

// 数量选择与快速查看
const showQuantitySelector = ref({})
const productQuantities = ref({})

// 初始空数组，将通过API填充
categories.value = []
products.value = []

// 处理分类筛选
const selectCategory = async (categoryId) => {
  selectedCategory.value = categoryId
  isLoading.value = true
  try {
    console.log(`开始筛选分类 ${categoryId} 的商品...`)
    
    let response;
    // 服务端价格过滤参数
    const priceParams = (() => {
      switch (selectedPriceRange.value) {
        case '0-100': return { MinPrice: 0, MaxPrice: 100 }
        case '100-200': return { MinPrice: 100, MaxPrice: 200 }
        case '200-500': return { MinPrice: 200, MaxPrice: 500 }
        case '500+': return { MinPrice: 500, MaxPrice: null }
        default: return {}
      }
    })()

    if (!categoryId) {
      // 优先使用服务端搜索（仅价格过滤）
      const params = { ...priceParams, Page: 1, PageSize: 36 }
      response = await productService.searchProducts(params)
    } else {
      // 分类 + 价格过滤（若有价格范围）
      const params = { ...priceParams, CategoryId: categoryId, Page: 1, PageSize: 36 }
      response = await productService.searchProducts(params)
    }
    
    // 详细记录API响应
    console.log(`分类 ${categoryId} 商品API响应:`, JSON.stringify(response))
    
    // 处理API响应，兼容不同的数据格式并进行数据映射
    if (response) {
      const productsData = response.data?.items || response.data || response || []
      if (Array.isArray(productsData)) {
        // 映射数据格式，确保图片路径正确
        products.value = productsData.map(product => ({
          id: product.id || product.Id,
          name: product.name || product.Name,
          description: product.description || product.Description,
          price: product.price || product.Price || 0,
          image: getProductImageUrl(product.image || product.imageUrl || product.ImageUrl || ''),
          categoryId: (() => {
            const val = product.categoryId ?? product.CategoryId
            const num = Number(val)
            return Number.isFinite(num) ? num : null
          })(),
          categoryName: product.categoryName || product.CategoryName || '',
          originalPrice: product.originalPrice || null,
          discount: product.discount || null,
          reviewCount: product.reviewCount || 0,
          salesCount: product.salesCount || product.SalesCount || 0
        }))
        console.log(`筛选分类 ${categoryId} 的商品，数量:`, products.value.length)
      } else {
        console.warn(`分类 ${categoryId} 商品数据不是数组格式:`, productsData)
        products.value = []
      }
    } else {
      console.warn(`分类 ${categoryId} 商品响应为空`)
      products.value = []
    }
  } catch (error) {
    console.error(`筛选分类 ${categoryId} 失败:`, error)
    products.value = []
  } finally {
    isLoading.value = false
  }
}

const handleAddProductClick = () => {
  router.push('/admin/product/create')
}

const getProductQuantity = (product) => {
  return productQuantities.value[product.id] || 1
}

const increaseQuantity = (product) => {
  const current = getProductQuantity(product)
  if (current < 99) {
    productQuantities.value[product.id] = current + 1
  }
}

const decreaseQuantity = (product) => {
  const current = getProductQuantity(product)
  if (current > 1) {
    productQuantities.value[product.id] = current - 1
  }
}

const quickView = (product) => {
  router.push(`/product/${product.id}`)
}

//

const handleAddToCart = async (product) => {
  const token = localStorage.getItem('token')
  if (!token) {
    notifyInfo('请先登录再加入购物车')
    router.push('/auth')
    return
  }
  if (!showQuantitySelector.value[product.id]) {
    showQuantitySelector.value[product.id] = true
    return
  }
  const quantity = getProductQuantity(product)
  try {
    const res = await cartStore.addToCart(product, quantity)
    if (res.success) {
      showQuantitySelector.value[product.id] = false
      productQuantities.value[product.id] = 1
      notifySuccess('已加入购物车')
    } else {
      notifyError(res.message || '加入购物车失败')
    }
  } catch (e) {
    notifyError('加入购物车失败')
  }
}

// 组件挂载时加载数据
const loadAllData = async () => {
  isLoading.value = true
  loadError.value = false
  errorMessage.value = ''
  try {
    const [categoriesResponse, productsResponse] = await Promise.all([
      categoryService.getCategories(),
      productService.getProducts()
    ])
    const categoriesData = categoriesResponse?.data || categoriesResponse || []
    categories.value = Array.isArray(categoriesData)
      ? categoriesData.map(c => ({
          id: Number(c.id ?? c.Id),
          name: c.name ?? c.Name ?? '',
          description: c.description ?? c.Description ?? ''
        }))
      : []
    const productsData = productsResponse?.data || productsResponse || []
    products.value = Array.isArray(productsData) ? productsData.map(product => ({
        id: product.id || product.Id,
        name: product.name || product.Name,
        description: product.description || product.Description,
        price: product.price || product.Price || 0,
        image: getProductImageUrl(product.image || product.imageUrl || product.ImageUrl || ''),
        categoryId: (() => {
          const val = product.categoryId ?? product.CategoryId
          const num = Number(val)
          return Number.isFinite(num) ? num : null
        })(),
        categoryName: product.categoryName || product.CategoryName || '',
        originalPrice: product.originalPrice || null,
        discount: product.discount || null,
        reviewCount: product.reviewCount || 0,
        salesCount: product.salesCount || product.SalesCount || 0
      })) : []
  } catch (error) {
    products.value = []
    categories.value = []
    loadError.value = true
    errorMessage.value = '后端临时不可用，请稍后重试'
  } finally {
    isLoading.value = false
  }
  try {
    const token = localStorage.getItem('token')
    if (token) {
      const fav = await favoriteService.list(1, 50)
      const items = fav?.data || fav || []
      favoriteProducts.value = items.map(i => i.productId || i.ProductId || i.id || i.Id).filter(Boolean)
    } else {
      favoriteProducts.value = []
    }
  } catch {}
}

onMounted(loadAllData)

// 根据路由查询参数预设分类
onMounted(() => {
  const q = route.query?.category
  const cid = q ? Number(q) : null
  if (cid && !Number.isNaN(cid)) {
    selectedCategories.value = [cid]
  }
})

// 监听路由变化，同步分类筛选
watchEffect(() => {
  const q = route.query?.category
  const cid = q ? Number(q) : null
  if (cid && !Number.isNaN(cid)) {
    selectedCategories.value = [cid]
  }
})

// 监听价格/分类选择变化：
// - 0 个分类：拉取所有（服务端价格过滤）
// - 1 个分类：服务端按分类+价格过滤
// - 2+ 个分类：服务端拉取所有（价格过滤），前端按多选分类交集过滤
watchEffect(async () => {
  const len = selectedCategories.value.length
  if (len === 0) {
    await selectCategory(null)
  } else if (len === 1) {
    await selectCategory(selectedCategories.value[0])
  } else {
    // 多选：拉取全量（受价格过滤），再用 filteredProducts 进行前端分类过滤
    await selectCategory(null)
  }
})

const isFavorite = (product) => favoriteProducts.value.includes(product.id)

const filteredProducts = computed(() => {
  let list = products.value || []
  // 分类多选过滤
  if (selectedCategories.value && selectedCategories.value.length > 0) {
    const set = new Set(selectedCategories.value)
    list = list.filter(p => set.has(p.categoryId))
  }
  switch (selectedPriceRange.value) {
    case '0-100':
      list = list.filter(p => (p.price || 0) <= 100)
      break
    case '100-200':
      list = list.filter(p => (p.price || 0) > 100 && (p.price || 0) <= 200)
      break
    case '200-500':
      list = list.filter(p => (p.price || 0) > 200 && (p.price || 0) <= 500)
      break
    case '500+':
      list = list.filter(p => (p.price || 0) > 500)
      break
  }
  return [...list].sort((a, b) => {
    if (sortOrder.value === 'price-asc') return (a.price || 0) - (b.price || 0)
    if (sortOrder.value === 'price-desc') return (b.price || 0) - (a.price || 0)
    if (sortOrder.value === 'sales-desc') return (b.salesCount || 0) - (a.salesCount || 0)
    return 0
  })
})

const clearFilters = () => {
  selectedCategories.value = []
  selectedCategory.value = null
  selectedPriceRange.value = null
  sortOrder.value = 'default'
  isFilterOpen.value = false
}

const toggleFavorite = async (product) => {
  const token = localStorage.getItem('token')
  if (!token) {
    notifyInfo('请先登录再收藏')
    router.push('/auth')
    return
  }
  try {
    if (isFavorite(product)) {
      await favoriteService.remove(product.id)
      favoriteProducts.value = favoriteProducts.value.filter(id => id !== product.id)
      notifySuccess('已取消收藏')
    } else {
      await favoriteService.add(product.id)
      favoriteProducts.value.push(product.id)
      notifySuccess('已添加收藏')
    }
  } catch (e) {
    notifyError('收藏操作失败')
  }
}

const onProductImageError = (e) => {
  e.target.src = '/images/default-product.svg'
}

//
</script>

<style scoped>
/* 自定义颜色 */
:root {
  --huanyu-pink-500: #ff6b6b;
  --huanyu-pink-600: #ee5a5a;
}

/* 商品卡片样式 */
.product-card {
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.product-card:hover {
  transform: translateY(-8px);
  box-shadow: 0 12px 24px rgba(0, 0, 0, 0.1);
}

/* 价格和评分样式 */
.text-yellow-400 {
  color: #fbbf24;
}

.text-huanyu-pink-500 {
  color: var(--huanyu-pink-500);
}

.text-huanyu-pink-600 {
  color: var(--huanyu-pink-600);
}

.bg-huanyu-pink-500 {
  background-color: var(--huanyu-pink-500);
}

.bg-huanyu-pink-600 {
  background-color: var(--huanyu-pink-600);
}

.hover\:bg-huanyu-pink-600:hover {
  background-color: var(--huanyu-pink-600);
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .grid-cols-1.lg\:grid-cols-4 {
    grid-template-columns: 1fr 3fr;
  }
}

@media (max-width: 768px) {
  .lg\:col-span-1 {
    margin-bottom: 2rem;
  }
  
  .grid-cols-1.lg\:grid-cols-4 {
    grid-template-columns: 1fr;
  }
  
  .product-card {
    margin-bottom: 1.5rem;
  }
  
  .container.mx-auto.px-4.py-6 {
    padding: 1rem;
  }
  
  .container.mx-auto.px-4.py-8 {
    padding: 1rem;
  }
  
  h1.text-2xl.font-bold.text-gray-800 {
    font-size: 1.5rem;
  }
}

@media (max-width: 480px) {
  .grid-cols-1.sm\:grid-cols-2.lg\:grid-cols-3 {
    grid-template-columns: 1fr;
  }
  
  .flex.justify-between.items-center {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }
  
  .bg-white.rounded-lg.shadow.p-6 {
    padding: 1rem;
  }
}

/* 动画效果 */
@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@keyframes slideUp {
  from {
    transform: translateY(20px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}
</style>
