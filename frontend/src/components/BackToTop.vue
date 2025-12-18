<template>
  <button 
    v-if="isVisible" 
    @click="scrollToTop" 
    class="back-to-top fixed bottom-8 right-8 px-2 py-2 bg-pink-400 text-white rounded-full shadow-lg hover:bg-pink-800 transition-all duration-300 transform hover:scale-110 z-50 flex items-center justify-center gap-2"
    aria-label="返回顶部"
  >
    <i class="fas fa-arrow-up"></i>
    <span class="font-medium">回到顶部</span>
  </button>
</template>

<script>
export default {
  name: 'BackToTop',
  data() {
    return {
      isVisible: false,
      scrollThreshold: 300 // 滚动超过300px时显示按钮
    }
  },
  mounted() {
    // 添加滚动事件监听
    window.addEventListener('scroll', this.handleScroll)
    // 初始检查滚动位置
    this.handleScroll()
  },
  beforeUnmount() {
    // 移除滚动事件监听，防止内存泄漏
    window.removeEventListener('scroll', this.handleScroll)
  },
  methods: {
    // 处理滚动事件
    handleScroll() {
      this.isVisible = window.pageYOffset > this.scrollThreshold
    },
    // 平滑滚动到顶部
    scrollToTop() {
      window.scrollTo({
        top: 0,
        behavior: 'smooth'
      })
    }
  }
}
</script>

<style scoped>
.back-to-top {
  cursor: pointer;
  opacity: 0.9;
  transition: opacity 0.3s ease, transform 0.3s ease;
}

.back-to-top:hover {
  opacity: 1;
}

.back-to-top i {
  font-size: 1.25rem;
}

/* 响应式调整 */
@media (max-width: 768px) {
  .back-to-top {
    bottom: 4rem;
    right: 1rem;
    padding: 0.5rem 1rem;
  }
  
  .back-to-top i {
    font-size: 1rem;
  }
}
</style>