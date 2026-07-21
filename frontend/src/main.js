import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from './router'
import App from './App.vue'
import { installAssetCacheRecovery } from './utils/assetRecovery'

// 引入全局样式
import './style.css'
import './styles/theme.css'

// 引入Element Plus
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'

// 浏览器本地缓存损坏时，图片和视频会显示 ERR_CACHE_READ_FAILURE。
// 在组件占位图逻辑执行前先绕过缓存自动重试一次。
installAssetCacheRecovery()

// 创建Vue应用实例
const app = createApp(App)

// 使用Element Plus
app.use(ElementPlus)

// 使用Pinia状态管理
app.use(createPinia())

// 使用Vue Router路由
app.use(router)

// 挂载应用到DOM
app.mount('#app')
