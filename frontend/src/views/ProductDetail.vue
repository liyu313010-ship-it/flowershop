<template>
  <main class="product-detail-page min-h-screen">
    <div class="container mx-auto px-4 py-8 md:py-12">
      <div class="product-shell grid grid-cols-1 gap-10 rounded-[2rem] border border-pink-100 bg-white p-5 shadow-sm md:p-8 lg:grid-cols-2 lg:gap-14">
        <section aria-label="商品图片">
          <button class="product-image-wrap block aspect-square w-full overflow-hidden rounded-[1.5rem] bg-pink-50" @click="showImageModal = true">
            <img :src="product.image" :alt="product.name" class="h-full w-full object-cover transition duration-500 hover:scale-[1.02]">
          </button>
        </section>

        <section class="space-y-6" aria-label="商品购买信息">
          <div>
            <p class="mb-2 text-sm font-semibold tracking-[0.2em] text-pink-500">FRESH FLOWERS · 当日鲜制</p>
            <h1 class="mb-3 text-3xl font-bold text-slate-900 md:text-4xl">{{ product.name }}</h1>
            <p class="text-base leading-8 text-slate-600">{{ getProductDescription(product) }}</p>
          </div>

          <div class="flex items-end gap-3">
            <span class="text-4xl font-bold text-pink-600"><small class="text-xl">¥</small>{{ product.price }}</span>
            <span v-if="product.originalPrice" class="pb-1 text-lg text-slate-400 line-through">¥{{ product.originalPrice }}</span>
          </div>

          <div class="flex flex-wrap gap-2">
            <span v-if="product.isHot" class="rounded-full bg-rose-100 px-3 py-1 text-sm text-rose-700">人气花礼</span>
            <span v-if="product.isNew" class="rounded-full bg-pink-100 px-3 py-1 text-sm text-pink-700">当季新品</span>
            <span :class="product.stock > 0 ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'" class="rounded-full px-3 py-1 text-sm">
              {{ product.stock > 0 ? `现货 · 库存 ${product.stock}` : '暂时售罄' }}
            </span>
          </div>

          <div id="qty-section" class="rounded-2xl bg-pink-50/70 p-4">
            <label class="mb-3 block text-sm font-semibold text-slate-700">选择数量</label>
            <div class="flex items-center gap-3">
              <button class="quantity-button" :disabled="quantity <= 1" @click="decreaseQuantity" aria-label="减少数量">−</button>
              <input v-model.number="quantity" type="number" min="1" :max="product.stock || 1" class="h-11 w-20 rounded-xl border border-pink-200 bg-white text-center" @input="validateQuantity">
              <button class="quantity-button" :disabled="quantity >= (product.stock || 1)" @click="increaseQuantity" aria-label="增加数量">＋</button>
            </div>
          </div>

          <div class="grid gap-3 sm:grid-cols-2">
            <button v-if="!userStore.isAdmin" class="btn-primary w-full" :disabled="addingToCart || product.stock <= 0" @click="addToCart">
              {{ addingToCart ? '添加中…' : (product.stock <= 0 ? '暂时售罄' : '加入购物车') }}
            </button>
            <button v-if="!userStore.isAdmin" class="btn-secondary w-full" :disabled="product.stock <= 0" @click="buyNow">立即购买</button>
            <button v-if="!userStore.isAdmin" class="btn-secondary w-full sm:col-span-2" @click="toggleFavorite">
              {{ isFavorite ? '已收藏 · 点击取消' : '♡ 收藏这束花' }}
            </button>
            <button v-else class="btn-primary w-full sm:col-span-2" @click="editProduct">编辑商品</button>
          </div>

          <div class="border-t border-pink-100 pt-6">
            <h2 class="mb-4 text-lg font-semibold text-slate-900">花礼详情</h2>
            <dl class="grid grid-cols-[5rem_1fr] gap-y-3 text-sm leading-6 text-slate-600">
              <dt class="font-medium text-slate-800">花材</dt><dd>{{ product.details.material || '以商品主花材与当季配叶搭配' }}</dd>
              <dt class="font-medium text-slate-800">规格</dt><dd>{{ product.details.size || '中型手作花束' }}</dd>
              <dt class="font-medium text-slate-800">包装</dt><dd>{{ product.details.packaging }}</dd>
              <dt class="font-medium text-slate-800">保鲜期</dt><dd>{{ product.details.shelfLife }}</dd>
              <dt class="font-medium text-slate-800">适合场合</dt><dd>{{ product.details.occasions || '生日、纪念日、探望与日常心意' }}</dd>
            </dl>
          </div>

          <div class="rounded-2xl border border-amber-100 bg-amber-50/70 p-4 text-sm leading-7 text-amber-800">
            鲜花为天然产品，花型与开放度会略有差异；如遇临时缺花，花艺师会在保持色系和价值的前提下进行同等替换。
          </div>
        </section>
      </div>

      <section v-if="reviews.length" class="reviews-panel mt-10 rounded-[2rem] border border-pink-100 bg-white p-6 shadow-sm md:p-9" aria-labelledby="product-reviews-title">
        <div class="mb-7 flex flex-wrap items-end justify-between gap-4">
          <div>
            <p class="mb-2 text-sm font-semibold tracking-[0.18em] text-pink-500">REAL MOMENTS</p>
            <h2 id="product-reviews-title" class="text-2xl font-bold text-slate-900">购买过这束花的人这样说</h2>
          </div>
          <div class="rounded-2xl bg-pink-50 px-5 py-3 text-right">
            <strong class="text-2xl text-pink-600">{{ reviewSummary.averageRating.toFixed(1) }}</strong>
            <span class="ml-1 text-amber-400">★★★★★</span>
            <p class="text-xs text-slate-500">来自 {{ reviewSummary.totalCount }} 条真实购买评价</p>
          </div>
        </div>

        <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          <article v-for="review in reviews" :key="review.id" class="rounded-2xl border border-pink-100 bg-pink-50/40 p-5">
            <header class="mb-4 flex items-center gap-3">
              <img :src="review.avatar" :alt="`${review.userName}头像`" class="h-11 w-11 rounded-full object-cover ring-2 ring-white" @error="handleAvatarError">
              <div>
                <h3 class="font-semibold text-slate-900">{{ review.userName }}</h3>
                <div class="text-sm text-amber-400" :aria-label="`${review.rating}星评价`">
                  <span v-for="star in 5" :key="star">{{ star <= review.rating ? '★' : '☆' }}</span>
                </div>
              </div>
            </header>
            <p class="min-h-16 leading-7 text-slate-700">{{ review.comment }}</p>
            <time class="mt-4 block text-xs text-slate-400" :datetime="review.createdAt">{{ formatReviewDate(review.createdAt) }}</time>
          </article>
        </div>
      </section>
    </div>

    <div v-if="showImageModal" class="fixed inset-0 z-50 flex items-center justify-center bg-slate-950/70 p-4" @click="showImageModal = false">
      <div class="relative max-h-[92vh] max-w-5xl overflow-hidden rounded-3xl bg-white p-3" @click.stop>
        <button class="absolute right-5 top-5 z-10 h-10 w-10 rounded-full bg-white/90 text-xl shadow" aria-label="关闭大图" @click="showImageModal = false">×</button>
        <img :src="product.image" :alt="product.name" class="max-h-[86vh] w-full rounded-2xl object-contain">
      </div>
    </div>
  </main>
</template>

<script setup>
import { onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useCartStore } from '@/stores/cart'
import { useUserStore } from '@/stores/user'
import { productService } from '@/services/product'
import { favoriteService } from '@/services/favorite'
import { getAvatarUrl, getProductImageUrl, handleAvatarError } from '@/utils/avatar.js'
import { getProductDescription } from '@/utils/productCopy'
import { notifyError, notifyInfo, notifySuccess } from '@/utils/notify'

const route = useRoute()
const router = useRouter()
const cartStore = useCartStore()
const userStore = useUserStore()

const quantity = ref(1)
const addingToCart = ref(false)
const loading = ref(false)
const isFavorite = ref(false)
const showImageModal = ref(false)
const reviews = ref([])
const reviewSummary = ref({ totalCount: 0, averageRating: 0 })

const product = ref({
  id: 0,
  name: '',
  description: '',
  price: 0,
  originalPrice: 0,
  image: '/images/product-placeholder.svg',
  stock: 0,
  isHot: false,
  isNew: false,
  details: { material: '', size: '', packaging: '粉白定制花纸与缎带手工包装', shelfLife: '约 5–7 天', occasions: '' }
})

const valueOf = (object, pascal, camel) => object?.[pascal] ?? object?.[camel]

const loadReviews = async (productId) => {
  try {
    const response = await productService.getProductReviews(productId, { pageNumber: 1, pageSize: 12 })
    const payload = response?.data ?? response ?? {}
    const items = payload.Items ?? payload.items ?? (Array.isArray(payload) ? payload : [])
    reviews.value = items.map((review) => ({
      id: valueOf(review, 'Id', 'id'),
      userName: valueOf(review, 'UserName', 'userName') || '匿名花友',
      avatar: getAvatarUrl(valueOf(review, 'Avatar', 'avatar')),
      rating: Number(valueOf(review, 'Rating', 'rating') || 0),
      comment: valueOf(review, 'Comment', 'comment') || '',
      createdAt: valueOf(review, 'CreatedAt', 'createdAt')
    })).filter((review) => review.comment)
    reviewSummary.value = {
      totalCount: Number(payload.TotalCount ?? payload.totalCount ?? reviews.value.length),
      averageRating: Number(payload.AverageRating ?? payload.averageRating ?? 0)
    }
  } catch {
    reviews.value = []
    reviewSummary.value = { totalCount: 0, averageRating: 0 }
  }
}

const loadProduct = async () => {
  loading.value = true
  const productId = Number(route.params.id)
  try {
    const response = await productService.getProductById(productId)
    const data = response?.data ?? response ?? {}
    product.value = {
      id: valueOf(data, 'Id', 'id') || 0,
      name: valueOf(data, 'Name', 'name') || '',
      description: valueOf(data, 'Description', 'description') || '',
      price: valueOf(data, 'Price', 'price') || 0,
      originalPrice: valueOf(data, 'OriginalPrice', 'originalPrice') || 0,
      image: getProductImageUrl(valueOf(data, 'ImageUrl', 'imageUrl') || ''),
      stock: valueOf(data, 'Stock', 'stock') || 0,
      isHot: Boolean(valueOf(data, 'IsFeatured', 'isFeatured')),
      isNew: Boolean(valueOf(data, 'IsNew', 'isNew')),
      details: {
        material: valueOf(data, 'Material', 'material') || '',
        size: valueOf(data, 'Size', 'size') || '',
        packaging: '粉白定制花纸与缎带手工包装',
        shelfLife: '约 5–7 天，随花附养护卡',
        occasions: valueOf(data, 'Occasion', 'occasion') || ''
      }
    }
    quantity.value = 1
    await loadReviews(product.value.id)
    try {
      const check = await favoriteService.check(product.value.id)
      isFavorite.value = Boolean(check?.data?.isFavorite || check?.isFavorite || check?.success)
    } catch {
      isFavorite.value = false
    }
  } catch (error) {
    console.error('加载商品失败:', error)
    notifyError('商品不存在或加载失败')
    router.push('/products')
  } finally {
    loading.value = false
  }
}

const increaseQuantity = () => { if (quantity.value < product.value.stock) quantity.value += 1 }
const decreaseQuantity = () => { if (quantity.value > 1) quantity.value -= 1 }
const validateQuantity = () => {
  quantity.value = Math.max(1, Math.min(Number(quantity.value) || 1, product.value.stock || 1))
}

const addToCart = async () => {
  if (!localStorage.getItem('token')) {
    notifyInfo('请先登录再加入购物车')
    router.push('/auth')
    return false
  }
  if (product.value.stock <= 0) return false
  addingToCart.value = true
  try {
    const result = await cartStore.addToCart(product.value, quantity.value)
    if (!result.success) throw new Error(result.message || '添加失败')
    notifySuccess(`已添加 ${quantity.value} 件 ${product.value.name} 到购物车`)
    return true
  } catch (error) {
    notifyError(error?.message || '添加到购物车失败，请重试')
    return false
  } finally {
    addingToCart.value = false
  }
}

const buyNow = async () => {
  if (await addToCart()) router.push('/checkout')
}

const toggleFavorite = async () => {
  if (!localStorage.getItem('token')) {
    notifyInfo('请先登录再收藏')
    router.push('/auth')
    return
  }
  try {
    if (isFavorite.value) await favoriteService.remove(product.value.id)
    else await favoriteService.add(product.value.id)
    isFavorite.value = !isFavorite.value
    notifySuccess(isFavorite.value ? '已添加收藏' : '已取消收藏')
  } catch {
    notifyError('收藏操作失败')
  }
}

const editProduct = () => router.push('/admin/products')
const formatReviewDate = (value) => value ? new Intl.DateTimeFormat('zh-CN', { year: 'numeric', month: 'long', day: 'numeric' }).format(new Date(value)) : ''

onMounted(loadProduct)
watch(() => route.params.id, (next, previous) => { if (next !== previous) loadProduct() })
</script>

<style scoped>
.product-detail-page { background: radial-gradient(circle at 85% 5%, #ffe9f2 0, transparent 30%), linear-gradient(180deg, #fffafb 0%, #fff 55%, #fff7fa 100%); }
.product-shell, .reviews-panel { box-shadow: 0 24px 70px rgba(181, 79, 122, .09); }
.product-image-wrap { box-shadow: inset 0 0 0 1px rgba(244, 182, 207, .28); }
.quantity-button { width: 2.75rem; height: 2.75rem; border: 1px solid #f3bfd2; border-radius: .75rem; background: white; color: #ad416f; font-size: 1.15rem; transition: .2s ease; }
.quantity-button:hover:not(:disabled) { background: #fff0f5; transform: translateY(-1px); }
.quantity-button:disabled { cursor: not-allowed; opacity: .4; }
</style>
