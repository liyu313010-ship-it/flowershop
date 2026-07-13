<template>
  <div class="min-h-screen bg-gray-50">
    

    <div class="container mx-auto px-4 py-8">
      <!-- 错误提示 -->
      <div v-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6">
        {{ error }}
        <button @click="error = ''" class="float-right text-red-500 hover:text-red-700">×</button>
      </div>

      <!-- 搜索和筛选 -->
      <div class="bg-white rounded-lg shadow p-6 mb-6">
        <div class="flex justify-between items-center mb-4">
          <h2 class="text-xl font-semibold">商品管理</h2>
          <button 
            @click="openAddModal"
            class="bg-huanyu-pink-600 hover:bg-huanyu-pink-700 text-white px-4 py-2 rounded-lg transition-colors"
          >
            添加商品
          </button>
        </div>
        <div class="grid grid-cols-1 md:grid-cols-5 gap-4">
          <input 
            v-model="searchQuery"
            @keyup.enter="searchProducts"
            type="text" 
            placeholder="搜索商品名称..."
            class="border rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-huanyu-pink-500"
          >
          <select v-model="selectedCategory" @focus="loadCategories" @change="searchProducts" class="border rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-huanyu-pink-500">
            <option value="">所有分类</option>
            <option v-for="cat in categoryOptions" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
          </select>
          <select v-model="selectedStatus" @change="searchProducts" class="border rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-huanyu-pink-500">
            <option value="">所有状态</option>
            <option value="active">上架</option>
            <option value="inactive">下架</option>
          </select>
          <button 
            @click="searchProducts"
            :disabled="isLoading"
            class="bg-gray-600 hover:bg-gray-700 text-white px-4 py-2 rounded-lg transition-colors disabled:opacity-50"
          >
            {{ isLoading ? '搜索中...' : '搜索' }}
          </button>
          <button 
            @click="resetFilters"
            class="bg-gray-300 hover:bg-gray-400 text-gray-700 px-4 py-2 rounded-lg transition-colors"
          >
            重置
          </button>
        </div>
      </div>

      <!-- 商品列表 -->
      <div class="bg-white rounded-lg shadow overflow-hidden relative">
        <div v-if="isLoading && products.length === 0" class="text-center py-12">
          <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-huanyu-pink-600"></div>
          <p class="mt-2 text-gray-600">加载商品中...</p>
        </div>
        
        <div v-else-if="products.length === 0" class="text-center py-12">
          <p class="text-gray-500">暂无商品数据</p>
          <button 
            @click="openAddModal"
            class="mt-4 bg-huanyu-pink-600 hover:bg-huanyu-pink-700 text-white px-4 py-2 rounded-lg transition-colors"
          >
            添加第一个商品
          </button>
        </div>
        
        <div v-else class="overflow-x-auto">
          <table class="w-full">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">商品</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">分类</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">价格</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">库存</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">状态</th>
                <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">操作</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="product in products" :key="product.id" class="hover:bg-gray-50">
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="flex items-center">
                    <img 
                      :src="getProductImage(product)" 
                      :alt="product.name"
                      class="w-12 h-12 object-cover rounded-lg"
                      @error="onImageError($event)"
                    >
                    <div class="ml-4">
                      <div class="text-sm font-medium text-gray-900">{{ product.name }}</div>
                      <div class="text-sm text-gray-500">{{ getProductDescription(product) }}</div>
                      <p class="text-sm text-gray-600">销量: {{ salesMap[product.id] || 0 }}</p>
                    </div>
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800">
                    {{ getCategoryName(product) }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  ¥{{ product.price }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                  {{ product.stock }}
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <span 
                    :class="(product.isActive || product.status === 'active') ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'"
                    class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full"
                  >
                    {{ (product.isActive || product.status === 'active') ? '上架' : '下架' }}
                  </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                  <button 
                    @click="editProduct(product)"
                    class="text-huanyu-pink-600 hover:text-huanyu-pink-900 mr-3"
                  >
                    编辑
                  </button>
                  <button 
                    @click="deleteProduct(product.id)"
                    class="text-red-600 hover:text-red-900"
                  >
                    删除
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        
        <!-- 表格加载状态遮罩 -->
        <div v-if="isLoading && products.length > 0" class="absolute inset-0 bg-white bg-opacity-50 flex items-center justify-center z-10">
          <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-huanyu-pink-600"></div>
        </div>
      </div>

      <!-- 分页 -->
      <div class="flex items-center justify-between mt-8">
        <div class="text-sm text-gray-700">
          显示第 {{ (currentPage - 1) * pageSize + 1 }} 到 {{ Math.min(currentPage * pageSize, total) }} 条，共 {{ total }} 条记录
        </div>
        <div class="flex items-center gap-4">
          <select 
            v-model="pageSize" 
            @change="handleSizeChange"
            class="border rounded px-3 py-1 text-sm"
          >
            <option :value="10">10条/页</option>
            <option :value="20">20条/页</option>
            <option :value="50">50条/页</option>
          </select>
          <div class="flex items-center space-x-2">
            <button 
              @click="handlePageChange(currentPage - 1)"
              :disabled="currentPage <= 1 || isLoading"
              class="px-3 py-2 border rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              上一页
            </button>
            
            <span class="px-4 py-2 text-sm text-gray-700">
              第 {{ currentPage }} 页，共 {{ Math.ceil(total / pageSize) }} 页
            </span>
            
            <button 
              @click="handlePageChange(currentPage + 1)"
              :disabled="currentPage >= Math.ceil(total / pageSize) || isLoading"
              class="px-3 py-2 border rounded-lg hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              下一页
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 添加/编辑商品模态框 -->
    <div v-if="showAddModal || showEditModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div class="bg-white rounded-lg p-8 max-w-2xl w-full mx-4">
        <h2 class="text-2xl font-bold mb-6">{{ showAddModal ? '添加商品' : '编辑商品' }}</h2>
        
        <!-- 错误提示 -->
        <div v-if="error" class="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-4">
          {{ error }}
        </div>
        
        <form @submit.prevent="saveProduct">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">商品名称</label>
              <input 
                v-model="productForm.name"
                type="text" 
                required
                class="w-full border rounded-lg px-3 py-2"
              >
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">分类</label>
              <select v-model="productForm.categoryId" @focus="loadCategories" required class="w-full border rounded-lg px-3 py-2">
                <option disabled value="">选择分类</option>
                <option v-for="cat in categoryOptions" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">价格</label>
              <input 
                v-model.number="productForm.price"
                type="number" 
                required
                min="0"
                step="0.01"
                class="w-full border rounded-lg px-3 py-2"
              >
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">库存</label>
              <input 
                v-model.number="productForm.stock"
                type="number" 
                required
                min="0"
                class="w-full border rounded-lg px-3 py-2"
              >
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">状态</label>
              <select v-model="productForm.status" required class="w-full border rounded-lg px-3 py-2">
                <option value="active">上架</option>
                <option value="inactive">下架</option>
              </select>
            </div>
            <div>
              <label class="flex items-center">
                <input 
                  v-model="productForm.isFeatured"
                  type="checkbox" 
                  class="mr-2"
                >
                <span class="text-sm font-medium text-gray-700">设为推荐产品</span>
              </label>
            </div>
          </div>
          
          <div class="mt-4">
            <label class="block text-sm font-medium text-gray-700 mb-2">商品图片</label>
            <div class="flex items-start space-x-4">
              <div class="flex-1">
                <input 
                  type="file" 
                  ref="imageFileInput"
                  @change="handleImageUpload"
                  accept="image/*"
                  class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-huanyu-pink-50 file:text-huanyu-pink-700 hover:file:bg-huanyu-pink-100"
                >
                <p class="mt-1 text-sm text-gray-500">点击选择图片文件，支持 JPG、PNG、GIF 格式</p>
              </div>
              <div class="flex-shrink-0">
                <div v-if="productForm.imagePreview" class="relative">
                  <img 
                    :src="productForm.imagePreview" 
                    alt="图片预览" 
                    class="h-24 w-24 object-cover rounded-lg border-2 border-gray-200"
                  >
                  <button 
                    type="button"
                    @click="removeImage"
                    class="absolute -top-2 -right-2 bg-red-500 text-white rounded-full p-1 hover:bg-red-600 transition-colors"
                    title="移除图片"
                  >
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                    </svg>
                  </button>
                </div>
                <div v-else class="h-24 w-24 border-2 border-dashed border-gray-300 rounded-lg flex items-center justify-center bg-gray-50">
                  <svg class="w-8 h-8 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
                  </svg>
                </div>
                <p class="mt-1 text-xs text-gray-500 text-center">图片预览</p>
              </div>
            </div>
          </div>

          <div class="mt-4">
            <label class="block text-sm font-medium text-gray-700 mb-2">商品描述</label>
            <textarea 
              v-model="productForm.description"
              rows="3"
              required
              class="w-full border rounded-lg px-3 py-2"
              placeholder="请输入商品的详细描述..."
            ></textarea>
          </div>

          <div class="flex justify-end space-x-4 mt-6">
            <button 
              type="button"
              @click="closeModal"
              class="px-6 py-2 border border-gray-300 rounded-lg hover:bg-gray-50"
            >
              取消
            </button>
            <button 
              type="submit"
              :disabled="isLoading || !productForm.categoryId"
              class="bg-huanyu-pink-600 hover:bg-huanyu-pink-700 text-white px-6 py-2 rounded-lg disabled:opacity-50"
            >
              {{ isLoading ? '保存中...' : '保存' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import AdminNav from '@/components/admin/AdminNav.vue'
import adminService from '@/services/adminService.js'
import { ElMessage } from 'element-plus'
import { getProductDescription } from '@/utils/productCopy'

// 响应式数据
const searchQuery = ref('')
const selectedCategory = ref('')
const selectedStatus = ref('')
const showAddModal = ref(false)
const showEditModal = ref(false)
const isLoading = ref(false)
const error = ref('')
const imageFileInput = ref(null)
const currentPage = ref(1)
const pageSize = ref(10)
const total = ref(0)

const productForm = reactive({
  id: null,
  name: '',
  description: '',
      categoryId: '',
  price: 0,
  stock: 0,
  image: '',
  imagePreview: '',
  status: 'active',
  isFeatured: false,
  isActive: true
})

const products = ref([])
const categoryOptions = ref([])
const salesMap = ref({})

// 方法
const getCategoryName = (product) => {
  const id = product.categoryId || product.CategoryId || product.category || product.Category
  if (!id && product.name) {
    const lower = String(product.name).toLowerCase()
    const mapByName = [
      { key: '玫瑰', name: '玫瑰' },
      { key: '康乃馨', name: '康乃馨' },
      { key: '百合', name: '百合' },
      { key: '郁金香', name: '郁金香' },
      { key: '向日葵', name: '向日葵' }
    ]
    const hit = mapByName.find(m => lower.includes(m.key.toLowerCase()))
    return hit ? hit.name : '未分类'
  }
  const idNum = typeof id === 'string' ? parseInt(id) : id
  const opt = categoryOptions.value.find(c => c.id === idNum)
  return opt ? opt.name : '未分类'
}

  const loadCategories = async () => {
    try {
      const res = await adminService.getCategories()
      const list = res?.data || res || []
      const opts = Array.isArray(list)
        ? list.map(c => ({ id: Number(c.id ?? c.Id), name: c.name ?? c.Name }))
        : []
      categoryOptions.value = opts.sort((a,b) => a.id - b.id)
      if (!opts.length) throw new Error('empty')
    } catch {
      categoryOptions.value = []
    }
  }

  // 已移除“新增分类”功能，分类通过后端加载并在下拉中选择

// 处理图片上传
const handleImageUpload = async (event) => {
  const file = event.target.files[0]
  if (file) {
    try {
      // 验证文件类型
      if (!file.type.startsWith('image/')) {
        error.value = '请选择图片文件'
        return
      }
      
      // 验证文件大小 (限制为5MB)
      if (file.size > 5 * 1024 * 1024) {
        error.value = '图片文件大小不能超过5MB'
        return
      }
      
      // 创建预览
      productForm.imagePreview = URL.createObjectURL(file)
      
      // 上传图片到服务器
      const response = await adminService.uploadProductImage(file)
      productForm.image = response.imageUrl
      
      // 清除错误信息
      error.value = ''
    } catch (err) {
      console.error('图片上传失败:', err)
      error.value = '图片上传失败，请重试'
      productForm.imagePreview = ''
    }
  }
}

// 移除图片
const removeImage = () => {
  productForm.imagePreview = ''
  productForm.image = ''
  if (imageFileInput.value) {
    imageFileInput.value.value = ''
  }
}

const loadProducts = async () => {
  try {
    isLoading.value = true
    const params = {
      page: currentPage.value,
      limit: pageSize.value // 后端使用limit而不是pageSize
    }
    const resp = await adminService.getAdminProducts(params)
    products.value = Array.isArray(resp?.data) ? resp.data : (Array.isArray(resp) ? resp : [])
    total.value = typeof resp?.total === 'number' ? resp.total : 0
    console.log('从数据库加载的商品数据:', products.value)
    await loadSalesCounts()
  } catch (err) {
    console.error('加载商品失败:', err)
    error.value = `加载商品失败: ${err.message || '未知错误'}`
    products.value = []
    total.value = 0
  } finally {
    isLoading.value = false
  }
}

const searchProducts = async () => {
  try {
    isLoading.value = true
    const params = {
      page: currentPage.value,
      limit: pageSize.value // 后端使用limit而不是pageSize
    }
    if (searchQuery.value) params.search = searchQuery.value
    if (selectedCategory.value) {
      params.categoryId = selectedCategory.value
      params.category = selectedCategory.value
    }
    params.status = selectedStatus.value || 'all'
    
    const resp = await adminService.getAdminProducts(params)
    products.value = Array.isArray(resp?.data) ? resp.data : (Array.isArray(resp) ? resp : [])
    total.value = typeof resp?.total === 'number' ? resp.total : 0
    console.log('搜索到的商品数据:', products.value)
  } catch (err) {
    console.error('搜索商品失败:', err)
    error.value = `搜索商品失败: ${err.message || '未知错误'}`
    products.value = []
    total.value = 0
  } finally {
    isLoading.value = false
  }
}

// 分页处理
const handlePageChange = (page) => {
  currentPage.value = page
  searchProducts()
}

const handleSizeChange = (size) => {
  pageSize.value = size
  currentPage.value = 1
  searchProducts()
}

// 重置筛选条件
const resetFilters = () => {
  searchQuery.value = ''
  selectedCategory.value = ''
  selectedStatus.value = ''
  currentPage.value = 1
  loadProducts()
}

const editProduct = async (product) => {
  try {
    const response = await adminService.getAdminProductById(product.id)
    const productData = response.data || response
    
    Object.assign(productForm, {
      id: productData.id,
      name: productData.name,
      description: getProductDescription(productData),
      categoryId: productData.categoryId || productData.CategoryId,
      price: productData.price,
      stock: productData.stock,
      image: productData.imageUrl,
      imagePreview: productData.imageUrl,
      status: productData.isActive ? 'active' : 'inactive',
      isFeatured: productData.isFeatured,
      isActive: productData.isActive
    })
    showEditModal.value = true
  } catch (err) {
    console.error('获取商品详情失败:', err)
    error.value = '获取商品详情失败'
  }
}

const deleteProduct = async (productId) => {
  if (confirm('确定要删除这个商品吗？删除后将无法恢复。')) {
    try {
      await adminService.deleteProduct(productId)
      console.log('商品删除成功，重新从数据库加载数据')
      // 删除成功后立即重新加载商品列表，确保与数据库实时同步
      await loadProducts()
    } catch (err) {
      console.error('删除商品失败:', err)
      error.value = `删除商品失败: ${err.message || '未知错误'}`
    }
  }
}

const saveProduct = async () => {
  try {
    isLoading.value = true
    
    const productData = {
      name: productForm.name,
      description: productForm.description,
      categoryId: productForm.categoryId ? Number(productForm.categoryId) : null,
      price: productForm.price,
      stock: productForm.stock,
      imageUrl: productForm.image,
      isFeatured: productForm.isFeatured,
      isActive: productForm.status === 'active'
    }
    if (!productData.categoryId) {
      error.value = '请选择分类'
      ElMessage.error(error.value)
      isLoading.value = false
      return
    }
    
    if (showAddModal.value) {
      await adminService.createProduct(productData)
      console.log('商品创建成功，重新从数据库加载数据')
    } else {
      await adminService.updateProduct(productForm.id, productData)
      console.log('商品更新成功，重新从数据库加载数据')
    }
    
    // 保存成功后立即重新加载商品列表，确保与数据库实时同步
    await loadProducts()
    closeModal()
  } catch (err) {
    console.error('保存商品失败:', err)
    error.value = `保存商品失败: ${err.message || '未知错误'}`
    // 显示错误提示
    ElMessage.error(error.value)
  } finally {
    isLoading.value = false
  }
}

const openAddModal = () => {
  if (!categoryOptions.value.length) {
    loadCategories()
  }
  showAddModal.value = true
}


const closeModal = () => {
  showAddModal.value = false
  showEditModal.value = false
  // 重置表单
  Object.assign(productForm, {
    id: null,
    name: '',
    description: '',
    category: '',
    price: 0,
    stock: 0,
    image: '',
    imagePreview: '',
    status: 'active',
    isFeatured: false,
    isActive: true
  })
  // 清除文件输入
  if (imageFileInput.value) {
    imageFileInput.value.value = ''
  }
  error.value = ''
}

// 页面加载时获取商品列表
onMounted(async () => {
  await loadCategories()
  await loadProducts()
})
// 图片地址与回退（移入脚本作用域）
const getProductImage = (product) => {
  const src = (product.imageUrl || product.image || '').trim()
  if (!src) return '/src/assets/default-product.png'
  if (src.startsWith('http')) return src
  return '/api' + src
}

const onImageError = (e) => {
  e.target.src = '/src/assets/default-product.png'
}

// 加载销量映射（移入脚本作用域）
const loadSalesCounts = async () => {
  try {
    const ranking = await adminService.getProductSalesRanking(100)
    const list = ranking?.data || ranking || []
    const map = {}
    list.forEach(item => {
      const id = item.id || item.productId || item.Id
      const count = item.salesCount || item.Quantity || 0
      if (id) map[id] = count
    })
    salesMap.value = map
  } catch (e) {
    salesMap.value = {}
  }
}

 
</script>
const hotCategoryNames = ['玫瑰','康乃馨','百合','郁金香','向日葵','满天星']
const hotCategories = computed(() => categoryOptions.value.filter(c => hotCategoryNames.includes(c.name)))
const otherCategories = computed(() => categoryOptions.value.filter(c => !hotCategoryNames.includes(c.name)))
