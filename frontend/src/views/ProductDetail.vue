<template>
  <div class="min-h-screen bg-gray-50">
    <div class="container mx-auto px-4 py-8">
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-12">
        
        <!-- 商品图片 -->
        <div class="space-y-4">
          <div class="aspect-square bg-white rounded-lg overflow-hidden">
            <img 
              :src="product.image" 
              :alt="product.name"
              class="w-full h-full object-cover"
            >
          </div>
          
          <!-- 缩略图 -->
          <div class="grid grid-cols-4 gap-2">
            <div 
              v-for="(thumb, index) in product.thumbnails" 
              :key="index"
              class="aspect-square bg-white rounded-lg overflow-hidden cursor-pointer hover:ring-2 hover:ring-huanyu-pink-500"
            >
              <img :src="thumb" :alt="`${product.name} ${index + 1}`" class="w-full h-full object-cover">
            </div>
          </div>
        </div>

        <!-- 商品信息 -->
        <div class="space-y-6">
          <div>
            <h1 class="text-3xl font-bold text-gray-900 mb-2">{{ product.name }}</h1>
            <p class="text-gray-600">{{ product.description }}</p>
          </div>

          <div class="flex items-baseline space-x-2">
            <span class="text-3xl font-bold text-huanyu-pink-600">¥{{ product.price }}</span>
            <span v-if="product.originalPrice" class="text-lg text-gray-400 line-through">
              ¥{{ product.originalPrice }}
            </span>
          </div>

          <!-- 商品标签 -->
          <div class="flex flex-wrap gap-2">
            <span v-if="product.isHot" class="px-3 py-1 bg-red-100 text-red-800 rounded-full text-sm">
              热卖
            </span>
            <span v-if="product.isNew" class="px-3 py-1 bg-huanyu-pink-100 text-huanyu-pink-800 rounded-full text-sm">
              新品
            </span>
            <span v-if="product.stock > 0" class="px-3 py-1 bg-green-100 text-green-800 rounded-full text-sm">
              现货
            </span>
            <span v-else class="px-3 py-1 bg-red-100 text-red-800 rounded-full text-sm">
              缺货
            </span>
          </div>

          <!-- 库存信息 -->
          <div class="text-sm text-gray-600">
            库存: <span v-if="product.stock > 0" class="text-green-600 font-medium">{{ product.stock }}</span>
            <span v-else class="text-red-600 font-medium">0（缺货）</span>
          </div>

          <!-- 商品规格 -->
          <div class="space-y-4">
            <div id="qty-section">
              <label class="block text-sm font-medium text-gray-700 mb-2">数量</label>
              <div class="flex items-center space-x-3">
                <button 
                  @click="decreaseQuantity"
                  class="w-10 h-10 rounded-lg border hover:bg-gray-100 flex items-center justify-center"
                  :disabled="quantity <= 1"
                >
                  -
                </button>
                <input 
                  v-model.number="quantity"
                  type="number"
                  min="1"
                  :max="product.stock || 999"
                  class="w-20 text-center border rounded-lg px-3 py-2"
                  @input="validateQuantity"
                >
                <button 
                  @click="increaseQuantity"
                  class="w-10 h-10 rounded-lg border hover:bg-gray-100 flex items-center justify-center"
                  :disabled="quantity >= (product.stock || 999)"
                >
                  +
                </button>
              </div>
              <p v-if="Number.isFinite(product.stock) && quantity > product.stock" class="mt-1 text-sm text-red-600">
                库存不足，请选择更少的数量
              </p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">配送方式</label>
              <select class="w-full border rounded-lg px-3 py-2">
                <option>标准配送 (2-3天)</option>
                <option>快速配送 (1天)</option>
                <option>当日配送 (3小时)</option>
              </select>
            </div>
          </div>

          <!-- 操作按钮 -->
          <div class="space-y-3">
            <!-- 仅对非管理员显示购物车和购买按钮 -->
            <template v-if="!userStore.isAdmin">
              <button 
                @click="addToCart"
                class="w-full btn-primary"
                :disabled="addingToCart || (Number.isFinite(product.stock) && (product.stock <= 0 || quantity > product.stock))"
              >
                {{ addingToCart ? '添加中...' : (product.stock <= 0 ? '缺货' : '加入购物车') }}
              </button>
              
              <button 
                @click="buyNow"
                class="w-full btn-secondary"
                :disabled="Number.isFinite(product.stock) && (product.stock <= 0 || quantity > product.stock)"
              >
                {{ product.stock <= 0 ? '缺货' : '立即购买' }}
              </button>

              <button 
                @click="toggleFavorite"
                class="w-full btn-secondary"
              >
                {{ isFavorite ? '取消收藏' : '收藏' }}
              </button>
            </template>
            <!-- 管理员显示编辑按钮 -->
            <template v-if="userStore.isAdmin">
              <button 
                @click="editProduct"
                class="w-full btn-primary"
              >
                编辑商品
              </button>
            </template>
          </div>

          <!-- 商品详情 -->
          <div class="border-t pt-4">
            <h3 class="text-lg font-semibold mb-4">商品详情</h3>
            <div class="space-y-3 text-gray-600">
              <div class="flex">
                <span class="font-medium w-20">商品规格：</span>
                <span>{{ product.size }}</span>
              </div>
              <div class="flex">
                <span class="font-medium w-24">花材/包装：</span>
                <span>{{ product.material }}</span>
              </div>
              
              <div class="flex">
                <span class="font-medium w-26">适合场合：</span>
                <span>{{ product.occasion }}</span>
              </div>
            </div>
          </div>

          <!-- 购买须知 -->
          <div class="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
            <h4 class="font-medium text-yellow-800 mb-2">购买须知</h4>
            <ul class="text-sm text-yellow-700 space-y-1">
              <li>• 鲜花为天然产品，实际收到的花材可能略有差异</li>
              <li>• 建议提前1-2天预订，确保准时送达</li>
              <li>• 如遇花材短缺，我们将用同等价值的替代花材</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div v-if="showImageModal" class="fixed inset-0 bg-black/60 z-50 flex items-center justify-center p-4" @click="closeImageModal">
    <div class="bg-white rounded-2xl max-w-5xl w-full overflow-hidden" @click.stop>
      <div class="flex justify-between items-center px-4 py-3 border-b">
        <h3 class="text-lg font-semibold text-gray-800">查看大图</h3>
        <button @click="closeImageModal" class="text-gray-400 hover:text-gray-600">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
          </svg>
        </button>
      </div>
      <div class="p-4">
        <img :src="product.image" :alt="product.name" class="w-full h-auto object-contain" />
      </div>
    </div>
  </div>
</template>

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
