<template>
  <div class="min-h-screen bg-gray-50 flex items-center justify-center">
    <div class="text-center">
      <div class="mb-8">
        <img 
          src="/images/flower-empty.svg" 
          alt="暂无花礼" 
          class="w-32 h-32 rounded-full mx-auto mb-4 object-cover"
        >
        <h1 class="text-6xl font-bold text-gray-800 mb-4">404</h1>
        <h2 class="text-2xl font-semibold text-gray-600 mb-4">页面未找到</h2>
        <p class="text-gray-500 mb-8 max-w-md mx-auto">
          抱歉，您访问的页面不存在或已被移除。请检查URL是否正确，或返回首页继续浏览。
        </p>
      </div>
      
      <div class="space-x-4">
        <router-link to="/" class="btn-primary">
          返回首页
        </router-link>
        <button @click="goBack" class="btn-secondary">
          返回上页
        </button>
      </div>
      
      <!-- 推荐链接 -->
      <div class="mt-12">
        <h3 class="text-lg font-medium text-gray-700 mb-4">您可能在寻找：</h3>
        <div class="flex flex-wrap justify-center gap-4">
          <router-link to="/products" class="text-huanyu-pink-600 hover:text-huanyu-pink-800">
            全部商品
          </router-link>
          <router-link to="/about" class="text-huanyu-pink-600 hover:text-huanyu-pink-800">
            关于我们
          </router-link>
          <router-link to="/contact" class="text-huanyu-pink-600 hover:text-huanyu-pink-800">
            联系我们
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'

const router = useRouter()

const goBack = () => {
  router.go(-1)
}
</script>
