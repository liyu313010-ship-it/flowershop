import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { resolve } from 'path'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src')
    }
  },
  server: {
    host: '0.0.0.0', // 允许外部访问
    port: 5173,
    open: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5002',
        changeOrigin: true,
        secure: false
      },
      '/uploads': {
        target: 'http://localhost:5002',
        changeOrigin: true,
        secure: false
      }
    },
    // 确保静态资源处理正确
    fs: {
      // 允许从public目录提供文件
      allow: ['..']
    }
  }
})
