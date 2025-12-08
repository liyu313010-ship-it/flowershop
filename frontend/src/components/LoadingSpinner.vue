<template>
  <div v-if="visible" class="loading-overlay" :class="{ 'full-screen': fullScreen }">
    <div class="loading-content">
      <div class="loading-spinner">
        <div class="spinner-ring"></div>
        <div class="spinner-ring"></div>
        <div class="spinner-ring"></div>
        <div class="spinner-ring"></div>
      </div>
      <p v-if="text" class="loading-text">{{ text }}</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  visible: {
    type: Boolean,
    default: false
  },
  text: {
    type: String,
    default: '加载中...'
  },
  fullScreen: {
    type: Boolean,
    default: true
  },
  delay: {
    type: Number,
    default: 0
  }
})

const showLoading = ref(false)
let timer = null

onMounted(() => {
  if (props.delay > 0) {
    timer = setTimeout(() => {
      showLoading.value = props.visible
    }, props.delay)
  } else {
    showLoading.value = props.visible
  }
})

onUnmounted(() => {
  if (timer) {
    clearTimeout(timer)
  }
})
</script>

<style scoped>
.loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(255, 255, 255, 0.95);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 9999;
  backdrop-filter: blur(5px);
  transition: all 0.3s ease;
}

.loading-overlay.full-screen {
  position: fixed;
}

.loading-overlay:not(.full-screen) {
  position: absolute;
  background: rgba(255, 255, 255, 0.8);
}

.loading-content {
  text-align: center;
  animation: fadeInUp 0.5s ease-out;
}

.loading-spinner {
  position: relative;
  width: 60px;
  height: 60px;
  margin: 0 auto 20px;
}

.spinner-ring {
  position: absolute;
  width: 100%;
  height: 100%;
  border: 3px solid transparent;
  border-radius: 50%;
  animation: spin 1.2s cubic-bezier(0.5, 0, 0.5, 1) infinite;
}

.spinner-ring:nth-child(1) {
  border-top-color: #667eea;
  animation-delay: -0.45s;
}

.spinner-ring:nth-child(2) {
  border-right-color: #764ba2;
  animation-delay: -0.3s;
}

.spinner-ring:nth-child(3) {
  border-bottom-color: #f093fb;
  animation-delay: -0.15s;
}

.spinner-ring:nth-child(4) {
  border-left-color: #f5576c;
}

.loading-text {
  color: #2c3e50;
  font-size: 1rem;
  font-weight: 500;
  margin: 0;
  animation: pulse 1.5s ease-in-out infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.6;
  }
}

/* 响应式设计 */
@media (max-width: 768px) {
  .loading-spinner {
    width: 50px;
    height: 50px;
    margin-bottom: 15px;
  }
  
  .loading-text {
    font-size: 0.9rem;
  }
}

@media (max-width: 480px) {
  .loading-spinner {
    width: 40px;
    height: 40px;
    margin-bottom: 12px;
  }
  
  .loading-text {
    font-size: 0.85rem;
  }
}
</style>