<template>
  <PageTransition>
    <div class="min-h-screen bg-gray-50">
      <LoadingSpinner :visible="isLoading" text="正在加载收藏..." />
      <div class="bg-white shadow-sm border-b">
        <div class="container mx-auto px-4 py-6">
          <div class="flex justify-between items-center">
            <div>
              <h1 class="text-2xl font-bold text-gray-800">我的收藏</h1>
              <p class="text-gray-600 mt-2">已收藏的鲜花商品，可加入购物车或查看详情</p>
            </div>
          </div>
        </div>
      </div>

      <div class="container mx-auto px-4 py-8">
        <div v-if="favorites.length === 0 && !isLoading" class="text-center py-16">
          <div class="text-gray-400 mb-4">💖</div>
          <h3 class="text-lg font-medium text-gray-700 mb-2">暂无收藏</h3>
          <p class="text-gray-500">去商品列表挑选心仪的鲜花并点击收藏</p>
          <router-link to="/products" class="mt-4 btn-primary">去逛逛</router-link>
        </div>

        <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          <div v-for="product in favorites" :key="product.id" class="card group cursor-pointer relative">
            <div class="relative overflow-hidden rounded-xl">
              <div class="w-full h-64 bg-gray-50 overflow-hidden relative">
                <img :src="product.image" :alt="product.name" class="w-full h-full object-contain transition-transform duration-300 group-hover:scale-110" @error="onImageError" />
              </div>
              <div class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center space-x-2 z-30 pointer-events-auto">
                <button @click.stop="goToProduct(product.id)" class="w-10 h-10 bg-white rounded-full flex items-center justify-center hover:bg-huanyu-pink-50 transition-colors shadow-lg" title="快速查看">
                  <svg class="w-5 h-5 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/></svg>
                </button>
                <button v-if="!userStore.isAdmin" @click.stop="handleAddToCart(product)" class="w-10 h-10 bg-huanyu-pink-500 text-white rounded-full flex items-center justify-center hover:bg-huanyu-pink-600 transition-colors shadow-lg" title="加入购物车">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"/></svg>
                </button>
                <button @click.stop="removeFavorite(product)" class="w-10 h-10 bg-white rounded-full flex items-center justify-center hover:bg-red-50 transition-colors shadow-lg text-red-500" title="取消收藏">
                  <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24"><path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/></svg>
                </button>
              </div>
            </div>
            <div class="p-4">
              <h3 class="font-semibold text-lg mb-2 text-gray-800 group-hover:text-huanyu-pink-600 transition-colors">{{ product.name }}</h3>
              <AutoLinkText class="text-gray-600 text-sm mb-3 line-clamp-2" :text="product.description" />
              <div class="flex items-center justify-between">
                <span class="text-xl font-bold text-huanyu-pink-500">¥{{ Math.round(product.price || 0) }}</span>
                <button v-if="!userStore.isAdmin" @click.stop="handleAddToCart(product)" class="bg-huanyu-pink-400 hover:bg-huanyu-pink-500 text-white p-2 rounded-full transition-all transform hover:scale-110" title="加入购物车">
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"/></svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </PageTransition>
 </template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import PageTransition from '@/components/PageTransition.vue'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import { favoriteService } from '@/services/favorite'
import { productService } from '@/services/product'
import { useCartStore } from '@/stores/cart'
import { useUserStore } from '@/stores/user'
import { notifySuccess, notifyError, notifyInfo } from '@/utils/notify'
import AutoLinkText from '@/components/AutoLinkText.vue'

const router = useRouter()
const cartStore = useCartStore()
const userStore = useUserStore()

const isLoading = ref(false)
const favorites = ref([])

const loadFavorites = async () => {
  isLoading.value = true
  try {
    const res = await favoriteService.list(1, 100)
    const list = res?.Favorites || res?.favorites || res?.data?.Favorites || res?.data?.favorites || []
    favorites.value = list.map(f => ({
      id: f.ProductId || f.productId || f.Id || f.id,
      name: f.ProductName || f.productName || '未命名商品',
      description: '',
      price: f.ProductPrice || f.productPrice || 0,
      image: f.ProductImage || f.productImage || '/images/default-product.svg'
    }))
  } catch {
    favorites.value = []
  } finally {
    isLoading.value = false
  }
}

const goToProduct = async (id) => {
  try { await router.push(`/product/${id}`) } catch { window.location.href = `/product/${id}` }
}

const handleAddToCart = async (product) => {
  const token = localStorage.getItem('token')
  if (!token) { notifyInfo('请先登录再加入购物车'); router.push('/auth'); return }
  try {
    const res = await cartStore.addToCart(product)
    if (res.success) { notifySuccess('已加入购物车') } else { notifyError(res.message || '加入购物车失败') }
  } catch { notifyError('加入购物车失败') }
}

const removeFavorite = async (product) => {
  try {
    await favoriteService.remove(product.id)
    favorites.value = favorites.value.filter(p => p.id !== product.id)
    notifySuccess('已取消收藏')
  } catch { notifyError('取消收藏失败') }
}

const onImageError = (e) => { e.target.src = '/images/default-product.svg' }

onMounted(loadFavorites)
</script>

<style scoped>
.card { transition: all 0.3s ease }
.card:hover { transform: translateY(-4px); box-shadow: 0 20px 25px -5px rgba(0,0,0,0.1), 0 10px 10px -5px rgba(0,0,0,0.04) }
.btn-primary { background-color: #ec4899; color: #fff; padding: 0.5rem 1rem; border-radius: 0.5rem }
.line-clamp-2 { display: -webkit-box; -webkit-line-clamp: 2; line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden }
</style>
