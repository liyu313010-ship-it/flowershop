<template>
  <div class="min-h-screen bg-gray-50 review-management-page">
    <div class="page-header">
      <h1>评价管理</h1>
      <p>查看和管理用户评价</p>
    </div>

    <div class="container mx-auto px-4 py-8">
      <!-- 筛选和搜索 -->
      <div class="filter-section mb-6">
        <div class="flex flex-wrap items-center justify-between gap-4">
          <div class="flex items-center gap-4">
            <div class="filter-item">
              <label for="rating-filter" class="mr-2">评分:</label>
              <select id="rating-filter" v-model="filter.rating" class="form-control">
                <option value="">全部</option>
                <option value="5">5星</option>
                <option value="4">4星</option>
                <option value="3">3星</option>
                <option value="2">2星</option>
                <option value="1">1星</option>
              </select>
            </div>
            <div class="filter-item">
              <label for="product-filter" class="mr-2">商品:</label>
              <select id="product-filter" v-model="filter.productId" class="form-control">
                <option value="">全部</option>
                <option v-for="product in products" :key="product.id" :value="product.id">
                  {{ product.name }}
                </option>
              </select>
            </div>
          </div>
          <div class="search-item">
            <input 
              type="text" 
              v-model="filter.search" 
              placeholder="搜索评价内容" 
              class="form-control"
              @input="handleSearch"
            >
          </div>
        </div>
      </div>

      <!-- 评价列表 -->
      <div class="review-list">
        <div v-if="loading" class="loading-state">
          <div class="loading-spinner"></div>
          <p>加载评价中...</p>
        </div>

        <div v-else-if="reviews.length === 0" class="empty-state">
          <div class="empty-icon">📝</div>
          <h3>暂无评价</h3>
          <p>还没有用户评价</p>
        </div>

        <div v-else class="reviews-grid">
          <div v-for="review in reviews" :key="review.id" class="review-card">
            <div class="review-header">
              <div class="user-info">
                <div class="user-avatar" :style="{ backgroundImage: `url(${review.avatar})` }"></div>
                <div class="user-details">
                  <h4 class="user-name">{{ review.userName }}</h4>
                  <div class="rating-stars">
                    <span v-for="star in 5" :key="star" class="star" :class="{ filled: star <= review.rating }">
                      ★
                    </span>
                  </div>
                </div>
              </div>
              <div class="review-actions">
                <button 
                  @click.stop="deleteReview(review.id)" 
                  class="delete-btn"
                  :disabled="deletingReviews.includes(review.id)"
                >
                  <span v-if="deletingReviews.includes(review.id)" class="spinner"></span>
                  <span v-else>删除</span>
                </button>
              </div>
            </div>
            <div class="review-content">
              <p class="comment">{{ review.comment }}</p>
              <div class="review-meta">
                <span class="product-name">{{ getProductName(review.productId) }}</span>
                <span class="review-date">{{ formatDate(review.createdAt) }}</span>
              </div>
              <a href="#" class="text-blue-600 hover:underline" @click.prevent="toggleReviewDetail(review)">查看评价详情</a>
              <div v-if="activeReviewId === review.id" class="mt-2 border-t pt-2">
                <div class="detail-content">
                  <div class="user-avatar-large" :style="{ backgroundImage: `url(${review.avatar})` }"></div>
                  <div class="user-info-detail">
                    <p class="user-name-large">{{ review.userName }}</p>
                    <div class="rating-stars-large">
                      <span v-for="star in 5" :key="star" class="star" :class="{ filled: star <= review.rating }">★</span>
                    </div>
                    <p class="product-name-detail">{{ getProductName(review.productId) }}</p>
                    <p class="review-date-detail">评价日期：{{ formatDate(review.createdAt) }}</p>
                    <p class="review-id-detail">评价ID：{{ review.id }}</p>
                  </div>
                </div>
                <p class="comment-detail">{{ review.comment || '暂无评价内容' }}</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 分页 -->
      <div v-if="totalPages > 1" class="pagination">
        <button 
          @click="changePage(1)" 
          :disabled="currentPage === 1"
          class="pagination-btn"
        >
          首页
        </button>
        <button 
          @click="changePage(currentPage - 1)" 
          :disabled="currentPage === 1"
          class="pagination-btn"
        >
          上一页
        </button>
        <span class="page-info">
          第 {{ currentPage }} / {{ totalPages }} 页
        </span>
        <button 
          @click="changePage(currentPage + 1)" 
          :disabled="currentPage === totalPages"
          class="pagination-btn"
        >
          下一页
        </button>
        <button 
          @click="changePage(totalPages)" 
          :disabled="currentPage === totalPages"
          class="pagination-btn"
        >
          末页
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { productService } from '@/services/product'
import { notifySuccess, notifyError } from '@/utils/notify'

// 响应式数据
const reviews = ref([])
const products = ref([])
const loading = ref(false)
const deletingReviews = ref([])
const currentPage = ref(1)
const pageSize = ref(10)
const totalPages = ref(1)
const activeReviewId = ref(null)

// 筛选条件
const filter = ref({
  rating: '',
  productId: '',
  search: ''
})

const toggleReviewDetail = (review) => {
  if (activeReviewId.value === review.id) {
    activeReviewId.value = null
  } else {
    activeReviewId.value = review.id
  }
}

// 加载商品列表
const loadProducts = async () => {
  try {
    const response = await productService.getProducts()
    const payload = response?.data ?? response
    products.value = Array.isArray(payload?.items)
      ? payload.items
      : (Array.isArray(payload) ? payload : [])
  } catch (error) {
    console.error('加载商品列表失败:', error)
  }
}

// 加载评价列表
const loadReviews = async () => {
  loading.value = true
  try {
    const params = {
      pageNumber: currentPage.value,
      pageSize: pageSize.value
    }
    
    // 获取所有评价（管理员功能）
    const response = await productService.getAllReviews(params)
    const list = Array.isArray(response)
      ? response
      : (Array.isArray(response?.data) ? response.data : (response?.Items || []))
    reviews.value = list || []
    totalPages.value = Math.max(1, Math.ceil((reviews.value.length || 0) / pageSize.value))
  } catch (error) {
    console.error('加载评价列表失败:', error)
    notifyError('加载评价列表失败')
  } finally {
    loading.value = false
  }
}

// 删除评价
const deleteReview = async (reviewId) => {
  if (confirm('确定要删除这个评价吗？')) {
    deletingReviews.value.push(reviewId)
    try {
      // 调用删除评价的API
      await productService.deleteProductReview(reviewId)
      notifySuccess('评价删除成功')
      // 重新加载评价列表
      await loadReviews()
    } catch (error) {
      console.error('删除评价失败:', error)
      notifyError('删除评价失败')
    } finally {
      deletingReviews.value = deletingReviews.value.filter(id => id !== reviewId)
    }
  }
}

// 处理搜索
const handleSearch = () => {
  currentPage.value = 1
  loadReviews()
}

// 分页处理
const changePage = (page) => {
  currentPage.value = page
  loadReviews()
}

// 获取商品名称
const getProductName = (productId) => {
  const product = products.value.find(p => p.id === productId)
  return product ? product.name : '未知商品'
}

// 格式化日期
const formatDate = (dateString) => {
  if (!dateString) return ''
  try {
    return new Date(dateString).toLocaleString('zh-CN', {
      year: 'numeric', month: '2-digit', day: '2-digit',
      hour: '2-digit', minute: '2-digit',
      timeZone: 'Asia/Shanghai'
    })
  } catch { return '' }
}

// 组件挂载时加载数据
onMounted(async () => {
  await loadProducts()
  await loadReviews()
})
</script>

<style scoped>
.review-management-page {
  min-height: 100vh;
  background-color: #f8f9fa;
}

.page-header {
  background: linear-gradient(135deg, #ff6b6b, #ff8e53);
  color: white;
  padding: 2rem 0;
  text-align: center;
}

.page-header h1 {
  font-size: 2.5rem;
  margin-bottom: 0.5rem;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
}

/* 筛选和搜索 */
.filter-section {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
}

.filter-item, .search-item {
  display: flex;
  align-items: center;
}

.form-control {
  padding: 0.5rem 1rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 1rem;
  width: 100%;
}

/* 评价列表 */
.loading-state, .empty-state {
  text-align: center;
  padding: 3rem;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #f3f3f3;
  border-top: 3px solid #ff6b6b;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.reviews-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.review-card {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.review-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background-size: cover;
  background-position: center;
  background-color: #f0f0f0;
}

.user-details {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.user-name {
  font-weight: 600;
  font-size: 1.1rem;
}

.rating-stars {
  display: flex;
  gap: 0.25rem;
}

.star {
  color: #ddd;
  font-size: 1rem;
  transition: color 0.2s ease;
}

.star.filled {
  color: #ffd700;
}

.review-actions {
  display: flex;
  gap: 0.5rem;
}

.delete-btn {
  background: #dc3545;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  transition: background 0.3s ease;
}

.delete-btn:hover {
  background: #c82333;
}

.delete-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.spinner {
  width: 16px;
  height: 16px;
  border: 2px solid #f3f3f3;
  border-top: 2px solid white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.review-content {
  margin-bottom: 1rem;
}

.comment {
  color: #666;
  line-height: 1.6;
  margin-bottom: 1rem;
}

.review-meta {
  display: flex;
  justify-content: space-between;
  align-items: center;
  color: #999;
  font-size: 0.9rem;
}

/* 分页 */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  margin-top: 2rem;
}

.pagination-btn {
  background: #ff6b6b;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  cursor: pointer;
  transition: background 0.3s ease;
}

.pagination-btn:hover:not(:disabled) {
  background: #ff8e53;
}

.pagination-btn:disabled {
  background: #6c757d;
  cursor: not-allowed;
}

.page-info {
  color: #666;
}

/* 详情模态框样式 */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
  padding: 1rem;
}

.modal-content {
  background: white;
  border-radius: 12px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
  max-width: 600px;
  width: 100%;
  max-height: 80vh;
  overflow-y: auto;
  animation: modalSlideIn 0.3s ease-out;
}

@keyframes modalSlideIn {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #eee;
}

.modal-header h3 {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 600;
  color: #333;
}

.close-btn {
  background: none;
  border: none;
  font-size: 2rem;
  cursor: pointer;
  color: #999;
  padding: 0;
  width: 30px;
  height: 30px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: all 0.2s ease;
}

.close-btn:hover {
  background-color: #f0f0f0;
  color: #333;
}

.modal-body {
  padding: 1.5rem;
}

.detail-section {
  margin-bottom: 1.5rem;
}

.detail-section h4 {
  margin: 0 0 1rem 0;
  font-size: 1.1rem;
  font-weight: 600;
  color: #555;
  border-bottom: 1px solid #eee;
  padding-bottom: 0.5rem;
}

.detail-content {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
}

.user-avatar-large {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background-size: cover;
  background-position: center;
  background-color: #f0f0f0;
}

.user-info-detail {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.user-name-large {
  font-size: 1.2rem;
  font-weight: 600;
  margin: 0;
}

.rating-stars-large {
  display: flex;
  gap: 0.25rem;
}

.rating-stars-large .star {
  font-size: 1.2rem;
  color: #ddd;
  transition: color 0.2s ease;
}

.rating-stars-large .star.filled {
  color: #ffd700;
}

.comment-detail {
  margin: 0;
  line-height: 1.6;
  color: #555;
  white-space: pre-wrap;
}

.product-name-detail, .review-date-detail, .review-id-detail {
  margin: 0.5rem 0;
  color: #666;
}

.modal-footer {
  padding: 1.5rem;
  border-top: 1px solid #eee;
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}

.cancel-btn {
  background: #6c757d;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 6px;
  cursor: pointer;
  font-size: 1rem;
  transition: background 0.3s ease;
}

.cancel-btn:hover {
  background: #5a6268;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .filter-section {
    flex-direction: column;
  }
  
  .filter-item, .search-item {
    width: 100%;
  }
  
  .reviews-grid {
    grid-template-columns: 1fr;
  }
  
  .review-header {
    flex-direction: column;
    gap: 1rem;
  }
  
  .review-meta {
    flex-direction: column;
    gap: 0.5rem;
    align-items: flex-start;
  }
  
  .modal-content {
    margin: 1rem;
    max-height: calc(100vh - 2rem);
  }
  
  .detail-content {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .user-avatar-large {
    align-self: center;
  }
  
  .user-info-detail {
    align-self: center;
    text-align: center;
  }
}
</style>
