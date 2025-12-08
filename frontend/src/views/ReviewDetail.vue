<template>
  <div class="min-h-screen bg-gray-50">
    <div class="container mx-auto px-4 py-8">
      <div class="bg-white rounded-xl shadow p-6">
        <div class="flex items-center justify-between mb-4">
          <h1 class="text-xl font-bold">评价详情</h1>
          <button @click="goBack" class="px-3 py-2 border rounded-lg text-gray-700 hover:bg-gray-50">返回</button>
        </div>
        <div v-if="loading" class="py-10 text-center text-gray-500">加载中...</div>
        <div v-else-if="error" class="py-4 bg-red-50 border border-red-200 text-red-700 rounded">{{ error }}</div>
        <div v-else-if="review" class="space-y-4">
          <div class="flex items-center gap-3">
            <div :style="{ backgroundImage: `url(${avatar})` }" class="w-12 h-12 bg-center bg-cover rounded-full"></div>
            <div>
              <div class="font-semibold">{{ userName }}</div>
              <div class="flex text-yellow-400">
                <span v-for="star in 5" :key="star" class="text-sm">{{ star <= (review.rating || 0) ? '★' : '☆' }}</span>
              </div>
            </div>
          </div>
          <div class="text-gray-700">{{ review.comment }}</div>
          <div class="text-sm text-gray-500">{{ formatDate(review.createdAt) }}</div>
          <div class="pt-2 flex gap-2">
            <button v-if="productId" @click="goToProduct(productId)" class="px-3 py-2 bg-huanyu-pink-500 hover:bg-huanyu-pink-600 text-white rounded-lg">查看该商品</button>
            <button v-if="canDelete()" @click="deleteThisReview" class="px-3 py-2 border border-red-300 text-red-600 rounded-lg hover:bg-red-50">删除评价</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { productService } from '@/services/product'
import { getAvatarUrl } from '@/utils/avatar.js'
import { useUserStore } from '@/stores/user'
import { notifySuccess, notifyError, notifyInfo } from '@/utils/notify'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const loading = ref(true)
const error = ref('')
const review = ref(null)
const productId = ref(null)
const userName = ref('')
const avatar = ref('')

const goBack = () => { router.back() }
const formatDate = (val) => { try { return new Date(val).toLocaleString('zh-CN') } catch { return '' } }
const goToProduct = async (pid) => { try { await router.push(`/product/${pid}`) } catch { window.location.href = `/product/${pid}` } }

const canDelete = () => {
  const uid = userStore.user?.id || userStore.user?.Id
  const isOwner = !!uid && (uid === (foundUserId.value || review.value?.userId))
  return isOwner || userStore.isAdmin
}

const foundUserId = ref(null)

const loadReview = async () => {
  loading.value = true
  try {
    const rid = Number(route.params.id)
    const pid = Number(route.query.productId)
    if (!rid) { throw new Error('参数错误') }
    productId.value = pid || null
    const res = await productService.getReviewById(rid)
    const found = res?.data || res
    if (!found) { throw new Error('找不到该评价') }
    review.value = {
      id: found.Id || found.id,
      rating: found.Rating || found.rating || 0,
      comment: found.Comment || found.comment || '',
      createdAt: found.CreatedAt || found.createdAt
    }
    userName.value = found.UserName || found.userName || '匿名用户'
    avatar.value = getAvatarUrl(found.Avatar || found.avatar || userName.value)
    foundUserId.value = found.UserId || found.userId || null
    if (!productId.value) productId.value = found.ProductId || found.productId || null
  } catch (e) {
    error.value = e.message || '加载评价失败'
  } finally { loading.value = false }
}

onMounted(() => { loadReview() })

const deleteThisReview = async () => {
  const token = localStorage.getItem('token')
  if (!token) { notifyInfo('请先登录'); router.push('/auth'); return }
  const rid = review.value?.id
  if (!rid) { notifyError('评价信息缺失'); return }
  if (!canDelete()) { notifyError('无权限删除该评价'); return }
  try {
    await productService.deleteProductReview(rid)
    notifySuccess('评价已删除')
    router.back()
  } catch (e) {
    notifyError('删除评价失败')
  }
}
</script>

<style scoped>
</style>
