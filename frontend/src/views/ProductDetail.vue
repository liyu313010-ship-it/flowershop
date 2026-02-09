<template>
  <div class="min-h-screen bg-gradient-to-br from-huanyu-pink-50 to-white relative overflow-hidden">
    <!-- 装饰背景球 -->
    <div class="absolute top-[-10%] left-[-10%] w-[500px] h-[500px] bg-huanyu-pink-200 rounded-full blur-[100px] opacity-30 animate-pulse-slow pointer-events-none"></div>
    <div class="absolute bottom-[-10%] right-[-10%] w-[600px] h-[600px] bg-purple-200 rounded-full blur-[120px] opacity-30 animate-pulse-slow pointer-events-none" style="animation-delay: 2s"></div>

    <div class="container mx-auto px-4 py-12 relative z-10">
      <!-- 面包屑导航 -->
      <nav class="flex mb-8 text-sm text-gray-500 animate-fade-in-down">
        <router-link to="/" class="hover:text-huanyu-pink-600 transition-colors">首页</router-link>
        <span class="mx-2">/</span>
        <router-link to="/products" class="hover:text-huanyu-pink-600 transition-colors">全部商品</router-link>
        <span class="mx-2">/</span>
        <span class="text-gray-900 font-medium">{{ product.name }}</span>
      </nav>

      <div class="grid grid-cols-1 lg:grid-cols-2 gap-12 lg:gap-20">
        
        <!-- 商品图片区域 -->
        <div class="space-y-6 animate-fade-in-left">
          <div 
            class="aspect-square bg-white/60 backdrop-blur-sm rounded-3xl overflow-hidden shadow-2xl relative group cursor-zoom-in border border-white/50"
            @click="openImageModal"
          >
            <img 
              :src="product.image" 
              :alt="product.name"
              class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110"
            >
            <div class="absolute inset-0 bg-black/0 group-hover:bg-black/5 transition-colors duration-300"></div>
            <div class="absolute bottom-4 right-4 bg-white/90 backdrop-blur text-gray-700 px-3 py-1.5 rounded-full text-sm font-medium shadow-sm opacity-0 group-hover:opacity-100 transition-opacity transform translate-y-2 group-hover:translate-y-0">
              <i class="fas fa-search-plus mr-1"></i> 查看大图
            </div>
          </div>
          
          <!-- 缩略图 -->
          <div class="grid grid-cols-4 gap-4" v-if="product.thumbnails && product.thumbnails.length > 0">
            <div 
              v-for="(thumb, index) in product.thumbnails" 
              :key="index"
              class="aspect-square bg-white/60 backdrop-blur-sm rounded-xl overflow-hidden cursor-pointer border-2 transition-all duration-300 transform hover:-translate-y-1 hover:shadow-lg"
              :class="activeImageIndex === index ? 'border-huanyu-pink-500 ring-2 ring-huanyu-pink-200' : 'border-transparent hover:border-huanyu-pink-300'"
              @click="activeImageIndex = index"
            >
              <img :src="thumb" :alt="`${product.name} ${index + 1}`" class="w-full h-full object-cover">
            </div>
          </div>
        </div>

        <!-- 商品信息区域 -->
        <div class="space-y-8 animate-fade-in-right">
          <div class="bg-white/60 backdrop-blur-md rounded-3xl p-8 shadow-xl border border-white/50">
            <div class="space-y-4">
              <h1 class="text-4xl font-bold text-gray-900 tracking-tight leading-tight">{{ product.name }}</h1>
              <p class="text-gray-600 text-lg leading-relaxed">{{ product.description }}</p>
            </div>

            <div class="flex items-end space-x-3 mt-6 border-b border-gray-100 pb-6">
              <span class="text-5xl font-bold text-transparent bg-clip-text bg-gradient-to-r from-huanyu-pink-600 to-huanyu-red-500">¥{{ product.price }}</span>
              <span v-if="product.originalPrice" class="text-xl text-gray-400 line-through mb-2">
                ¥{{ product.originalPrice }}
              </span>
            </div>

            <!-- 标签与库存 -->
            <div class="flex flex-wrap items-center gap-3 mt-6">
              <span v-if="product.isHot" class="px-3 py-1 bg-gradient-to-r from-red-500 to-pink-500 text-white rounded-full text-xs font-bold shadow-sm uppercase tracking-wider">
                HOT 热卖
              </span>
              <span v-if="product.isNew" class="px-3 py-1 bg-gradient-to-r from-blue-400 to-cyan-400 text-white rounded-full text-xs font-bold shadow-sm uppercase tracking-wider">
                NEW 新品
              </span>
              <span class="px-3 py-1 bg-green-100 text-green-700 rounded-full text-xs font-bold flex items-center">
                <span class="w-2 h-2 bg-green-500 rounded-full mr-1.5"></span>
                {{ product.stock > 0 ? `库存: ${product.stock}` : '暂时缺货' }}
              </span>
            </div>

            <!-- 商品规格选择 -->
            <div class="mt-8 space-y-6">
              <!-- 数量选择 -->
              <div id="qty-section">
                <label class="block text-sm font-bold text-gray-700 mb-3">购买数量</label>
                <div class="flex items-center space-x-4">
                  <div class="flex items-center bg-gray-50 rounded-xl border border-gray-200 p-1 shadow-inner">
                    <button 
                      @click="decreaseQuantity"
                      class="w-10 h-10 rounded-lg bg-white hover:bg-gray-100 text-gray-600 flex items-center justify-center transition-colors shadow-sm disabled:opacity-50 disabled:cursor-not-allowed"
                      :disabled="quantity <= 1"
                    >
                      <i class="fas fa-minus text-xs"></i>
                    </button>
                    <input 
                      v-model.number="quantity"
                      type="number"
                      min="1"
                      :max="product.stock || 999"
                      class="w-16 text-center bg-transparent border-none focus:ring-0 text-gray-800 font-bold text-lg p-0"
                      @input="validateQuantity"
                    >
                    <button 
                      @click="increaseQuantity"
                      class="w-10 h-10 rounded-lg bg-white hover:bg-gray-100 text-gray-600 flex items-center justify-center transition-colors shadow-sm disabled:opacity-50 disabled:cursor-not-allowed"
                      :disabled="quantity >= (product.stock || 999)"
                    >
                      <i class="fas fa-plus text-xs"></i>
                    </button>
                  </div>
                  <span class="text-sm text-gray-500" v-if="Number.isFinite(product.stock) && quantity > product.stock">
                    库存不足
                  </span>
                </div>
              </div>
            </div>

            <!-- 操作按钮 -->
            <div class="mt-10 space-y-4">
              <template v-if="!userStore.isAdmin">
                <div class="grid grid-cols-2 gap-4">
                  <button 
                    @click="addToCart"
                    class="btn-primary-lg relative overflow-hidden group"
                    :disabled="addingToCart || (Number.isFinite(product.stock) && (product.stock <= 0 || quantity > product.stock))"
                  >
                    <span class="relative z-10 flex items-center justify-center">
                      <i class="fas fa-shopping-cart mr-2 transition-transform group-hover:-translate-x-1"></i>
                      {{ addingToCart ? '添加中...' : (product.stock <= 0 ? '缺货' : '加入购物车') }}
                    </span>
                    <div class="absolute inset-0 bg-white/20 translate-y-full group-hover:translate-y-0 transition-transform duration-300"></div>
                  </button>
                  
                  <button 
                    @click="buyNow"
                    class="btn-secondary-lg group"
                    :disabled="Number.isFinite(product.stock) && (product.stock <= 0 || quantity > product.stock)"
                  >
                    <span class="flex items-center justify-center">
                      <i class="fas fa-bolt mr-2 transition-transform group-hover:scale-110"></i>
                      {{ product.stock <= 0 ? '缺货' : '立即购买' }}
                    </span>
                  </button>
                </div>

                <button 
                  @click="toggleFavorite"
                  class="w-full py-3 rounded-xl border border-gray-200 text-gray-500 hover:bg-gray-50 hover:text-huanyu-pink-500 transition-all flex items-center justify-center space-x-2"
                  :class="{ 'text-huanyu-pink-500 border-huanyu-pink-200 bg-huanyu-pink-50': isFavorite }"
                >
                  <i :class="isFavorite ? 'fas fa-heart' : 'far fa-heart'" class="text-lg transition-transform active:scale-125"></i>
                  <span>{{ isFavorite ? '已收藏' : '添加到收藏夹' }}</span>
                </button>
              </template>
              
              <template v-else>
                <button 
                  @click="editProduct"
                  class="w-full btn-primary-lg"
                >
                  <i class="fas fa-edit mr-2"></i> 编辑商品
                </button>
              </template>
            </div>
          </div>

          <!-- 商品参数卡片 -->
          <div class="bg-white/60 backdrop-blur-md rounded-3xl p-8 shadow-xl border border-white/50">
            <h3 class="text-xl font-bold text-gray-900 mb-6 flex items-center">
              <i class="fas fa-info-circle text-huanyu-pink-500 mr-2"></i> 商品详情
            </h3>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-y-4 gap-x-8 text-sm">
              <div class="flex border-b border-gray-100 pb-2">
                <span class="text-gray-500 w-24">商品规格</span>
                <span class="text-gray-800 font-medium">{{ product.size }}</span>
              </div>
              <div class="flex border-b border-gray-100 pb-2">
                <span class="text-gray-500 w-24">花材/包装</span>
                <span class="text-gray-800 font-medium">{{ product.material }}</span>
              </div>
              <div class="flex border-b border-gray-100 pb-2">
                <span class="text-gray-500 w-24">适合场合</span>
                <span class="text-gray-800 font-medium">{{ product.occasion }}</span>
              </div>
              <div class="flex border-b border-gray-100 pb-2">
                <span class="text-gray-500 w-24">配送说明</span>
                <span class="text-gray-800 font-medium">支持全国配送，市区内免费</span>
              </div>
            </div>
          </div>

          <!-- 购买须知 -->
          <div class="bg-gradient-to-br from-yellow-50 to-orange-50 border border-yellow-100 rounded-3xl p-6 shadow-sm">
            <h4 class="font-bold text-yellow-800 mb-3 flex items-center">
              <i class="fas fa-exclamation-triangle mr-2"></i> 购买须知
            </h4>
            <ul class="text-sm text-yellow-800/80 space-y-2 list-disc list-inside">
              <li>鲜花为天然产品，实际收到的花材颜色和形态可能略有差异</li>
              <li>建议提前1-2天预订，确保准时送达，特别是节假日</li>
              <li>如遇季节性花材短缺，我们将使用同等价值的替代花材，敬请谅解</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- 图片放大模态框 -->
  <div v-if="showImageModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 backdrop-blur-lg bg-black/70 transition-all duration-300" @click="closeImageModal">
    <div class="relative max-w-5xl w-full animate-zoom-in" @click.stop>
      <button @click="closeImageModal" class="absolute -top-12 right-0 text-white hover:text-huanyu-pink-400 transition-colors">
        <i class="fas fa-times text-3xl"></i>
      </button>
      <img :src="product.image" :alt="product.name" class="w-full h-auto max-h-[85vh] object-contain rounded-lg shadow-2xl" />
    </div>
  </div>
</template>

<style scoped>
.btn-primary-lg {
  @apply w-full py-4 bg-gradient-to-r from-huanyu-pink-500 to-huanyu-red-500 text-white rounded-xl font-bold shadow-lg hover:shadow-huanyu-pink-500/30 transition-all transform hover:-translate-y-0.5 active:translate-y-0 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none;
}
.btn-secondary-lg {
  @apply w-full py-4 bg-white text-huanyu-pink-600 border-2 border-huanyu-pink-100 rounded-xl font-bold shadow-md hover:border-huanyu-pink-500 hover:text-huanyu-pink-700 transition-all transform hover:-translate-y-0.5 active:translate-y-0 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none;
}
</style>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useCartStore } from '@/stores/cart'
import { useUserStore } from '@/stores/user'
import { productService } from '@/services/product'
import { getProductImageUrl } from '@/utils/avatar.js'
import { notifySuccess, notifyError, notifyInfo } from '@/utils/notify'
import { favoriteService } from '@/services/favorite'

const route = useRoute()
const router = useRouter()
const cartStore = useCartStore()
const userStore = useUserStore()
const isFavorite = ref(false)

const quantity = ref(1)
const addingToCart = ref(false)
const loading = ref(false)

// 商品数据
const product = ref({
  id: 0,
  name: '',
  description: '',
  price: 0,
  originalPrice: 0,
  image: '',
  thumbnails: [],
  isHot: false,
  isNew: false,
  details: {
    material: '',
    packaging: '',
    shelfLife: '',
    occasions: ''
  },
  material: '',
  occasion: '',
  size: ''
})

const increaseQuantity = () => {
  if (quantity.value < (product.value.stock || 999)) {
    quantity.value++
  }
}

const decreaseQuantity = () => {
  if (quantity.value > 1) {
    quantity.value--
  }
}

const validateQuantity = () => {
  // 确保数量不为负数
  if (quantity.value < 1) {
    quantity.value = 1
  }
  // 确保不超过库存
  if (Number.isFinite(product.value.stock) && product.value.stock > 0 && quantity.value > product.value.stock) {
    quantity.value = product.value.stock
  }
}

const addToCart = async () => {
  const token = localStorage.getItem('token')
  if (!token) {
    notifyInfo('请先登录再加入购物车')
    router.push('/auth')
    return
  }
  // 前端库存检查
  if (Number.isFinite(product.value.stock) && product.value.stock <= 0) {
    notifyError('抱歉，该商品暂时缺货')
    return
  }
  
  if (Number.isFinite(product.value.stock) && quantity.value > product.value.stock) {
    notifyError(`抱歉，库存不足。当前库存：${product.value.stock}`)
    quantity.value = product.value.stock
    return
  }
  
  addingToCart.value = true
  
  try {
    const result = await cartStore.addToCart(product.value, quantity.value)
    if (result.success) {
      notifySuccess(`已添加 ${quantity.value} 件 ${product.value.name} 到购物车`)
    } else {
      notifyError(result.message || '添加到购物车失败')
    }
  } catch (error) {
    console.error('添加到购物车失败:', error)
    notifyError('添加到购物车失败，请重试')
  } finally {
    addingToCart.value = false
  }
}

const buyNow = () => {
  const token = localStorage.getItem('token')
  if (!token) {
    notifyInfo('请先登录再购买')
    router.push('/auth')
    return
  }
  addToCart().then(() => {
    router.push('/checkout')
  }).catch((e) => {
    console.error('立即购买失败:', e)
    notifyError('立即购买失败')
  })
}

// 管理员编辑商品
const editProduct = () => {
  // 跳转到商品管理页面或打开编辑模态框
  router.push('/admin/products')
}

// 加载商品数据
const loadProduct = async () => {
  loading.value = true
  try {
    const productId = route.params.id
    const response = await productService.getProductById(productId)
    const data = response?.data || response || {}
    const stockVal = Number((data.Stock ?? data.stock ?? 0))
    product.value = {
      id: data.Id || data.id || 0,
      name: data.Name || data.name || '',
      description: data.Description || data.description || '',
      price: data.Price || data.price || 0,
      originalPrice: data.OriginalPrice || data.originalPrice || 0,
      image: getProductImageUrl(data.ImageUrl || data.imageUrl || data.image || ''),
      thumbnails: Array.isArray(data.thumbnails) ? data.thumbnails : [],
      isHot: data.IsHot || data.isHot || false,
      isNew: data.IsNew || data.isNew || false,
      stock: Number.isFinite(stockVal) ? stockVal : 0,
      details: {
        material: (data.Details?.Material) || (data.details?.material) || '',
        packaging: (data.Details?.Packaging) || (data.details?.packaging) || '',
        shelfLife: (data.Details?.ShelfLife) || (data.details?.shelfLife) || ''
      },
      material: data.Material || data.material || '',
      occasion: data.Occasion || data.occasion || '',
      size: data.Size || data.size || ''
    }
    try {
      const token = localStorage.getItem('token')
      if (token) {
        const check = await favoriteService.check(product.value.id)
        isFavorite.value = !!(check?.data?.IsFavorited || check?.IsFavorited || check?.data?.isFavorited || check?.isFavorited)
      } else {
        isFavorite.value = false
      }
    } catch {}
  } catch (error) {
    console.error('加载商品失败:', error)
    notifyError('商品不存在或加载失败')
    router.push('/products')
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadProduct()
})

const toggleFavorite = async () => {
  const token = localStorage.getItem('token')
  if (!token) {
    notifyInfo('请先登录再收藏')
    router.push('/auth')
    return
  }
  try {
    if (isFavorite.value) {
      await favoriteService.remove(product.value.id)
      isFavorite.value = false
      notifySuccess('已取消收藏')
    } else {
      await favoriteService.add(product.value.id)
      isFavorite.value = true
      notifySuccess('已添加收藏')
    }
  } catch (e) {
    notifyError('收藏操作失败')
  }
}
</script>
const showImageModal = ref(false)
const overlayCartClickedOnce = ref(false)

const openImageModal = () => { showImageModal.value = true }
const closeImageModal = () => { showImageModal.value = false }

const handleOverlayAddToCart = () => {
  const token = localStorage.getItem('token')
  if (!token) {
    notifyInfo('请先登录再加入购物车')
    router.push('/auth')
    return
  }
  if (!overlayCartClickedOnce.value) {
    overlayCartClickedOnce.value = true
    const el = document.getElementById('qty-section')
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'center' })
    return
  }
  addToCart()
  overlayCartClickedOnce.value = false
}
