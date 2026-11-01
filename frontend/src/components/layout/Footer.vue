<template>
  <!-- 页脚组件 -->
  <footer class="bg-gradient-to-r from-pink-50 to-pink-100 text-pink-800 mt-16 border-t border-pink-200">
    <div class="container mx-auto px-4 py-12">
      
      <!-- 页脚主要内容 -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-8">
        
        <!-- 品牌介绍 -->
        <div class="col-span-1 md:col-span-2">
          <div class="flex items-center space-x-2 mb-4">
            <!-- 品牌Logo -->
            <img 
              src="/images/logo.png" 
              alt="欢雨鲜花Logo" 
              class="w-10 h-10 rounded-full object-cover"
            >
            <h3 class="text-xl font-bold">欢雨flower</h3>
          </div>
          <p class="text-pink-600 leading-relaxed mb-4">
            欢雨鲜花致力于为您提供最优质的花卉产品和服务。每一朵花都经过精心挑选，确保为您传递最真挚的情感和祝福。
          </p>
          
          <!-- 社交媒体链接 -->
          <div class="flex justify-start space-x-4">
            <div v-for="item in socialLinks" :key="item.name" class="relative group">
              <a 
                :href="item.link" 
                class="w-10 h-10 bg-white/80 backdrop-blur-sm rounded-full flex items-center justify-center hover:bg-pink-400 hover:text-white transition-all duration-300 shadow-sm hover:shadow-md hover:-translate-y-1" 
                :title="item.name"
              >
                <img :src="item.icon" :alt="item.name" class="w-5 h-5 transition-transform duration-300 group-hover:scale-110 group-hover:brightness-200" />
              </a>
              
              <!-- 悬浮预览 -->
              <div class="absolute bottom-full left-1/2 transform -translate-x-1/2 mb-3 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-300 z-10 translate-y-2 group-hover:translate-y-0">
                <div v-if="item.type === 'image'" class="w-40 h-40 bg-white rounded-lg shadow-xl border-2 border-pink-100 overflow-hidden p-2">
                  <img :src="item.qrCode" :alt="item.name + '二维码'" class="w-full h-full object-contain" />
                </div>
                <div v-else class="bg-white text-pink-800 px-4 py-2 rounded-lg shadow-xl border border-pink-100 whitespace-nowrap font-medium">
                  {{ item.content }}
                </div>
                <!-- 小箭头 -->
                <div class="w-3 h-3 bg-white border-b border-r border-pink-100 transform rotate-45 absolute left-1/2 -translate-x-1/2 -bottom-1.5 shadow-sm"></div>
              </div>
            </div>
          </div>
        </div>
        
        <!-- 快速链接 -->
        <div>
          <h4 class="text-lg font-semibold mb-4">快速链接</h4>
          <ul class="space-y-2">
            <li><a href="/" class="text-pink-600 hover:text-pink-800 transition-colors">首页</a></li>
            <li><a href="/products" class="text-pink-600 hover:text-pink-800 transition-colors">全部商品</a></li>
            <li><a href="/about" class="text-pink-600 hover:text-pink-800 transition-colors">关于我们</a></li>
            <li><a href="/contact" class="text-pink-600 hover:text-pink-800 transition-colors">联系我们</a></li>
          </ul>
        </div>
        
        <!-- 客户服务 -->
        <div>
          <h4 class="text-lg font-semibold mb-4">客户服务</h4>
          <ul class="space-y-2">
            <li><a href="/shipping" class="text-pink-600 hover:text-pink-800 transition-colors">配送说明</a></li>
            <li><a href="/returns" class="text-pink-600 hover:text-pink-800 transition-colors">退换货政策</a></li>
            <li><a href="/faq" class="text-pink-600 hover:text-pink-800 transition-colors">常见问题</a></li>
            <li><a href="/privacy" class="text-pink-600 hover:text-pink-800 transition-colors">隐私政策</a></li>
          </ul>
        </div>
        
        <!-- 联系信息 -->
        <div>
          <h4 class="text-lg font-semibold mb-4">联系我们</h4>
          <div class="space-y-3">
            <div class="flex items-center space-x-2">
              <svg class="w-5 h-5 text-pink-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"></path>
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"></path>
              </svg>
              <span class="text-pink-600">西南林业大学</span>
            </div>
            <div class="flex items-center space-x-2">
              <svg class="w-5 h-5 text-pink-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"></path>
              </svg>
              <span class="text-pink-600">15025033613</span>
            </div>
            <div class="flex items-center space-x-2">
              <svg class="w-5 h-5 text-pink-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path>
              </svg>
              <span class="text-pink-600">2784107771@qq.com</span>
            </div>
          </div>
          
          <!-- 营业时间 -->
          <div class="mt-4 p-3 bg-pink-200 rounded-lg">
            <p class="text-sm font-medium mb-1 text-pink-800">营业时间</p>
            <p class="text-xs text-pink-700">周一至周日: 8:00 - 20:00</p>
            <p class="text-xs text-pink-700">节假日正常营业</p>
          </div>
        </div>
      </div>
      
      <!-- 版权信息 -->
      <div class="border-t border-pink-300 mt-8 pt-8 text-center">
        <p class="text-pink-600">
          © 2025 欢雨flower. All rights reserved. | 
          <a href="/terms" class="hover:text-pink-800 transition-colors">服务条款</a> | 
          <a href="/privacy" class="hover:text-pink-800 transition-colors">隐私政策</a>
        </p>
        <p class="text-xs text-pink-500 mt-2">
          用心传递每一份美好 ❤️
        </p>
      </div>
    </div>
  </footer>
</template>

<script setup>
// 页脚组件 - 主要展示品牌信息和联系方式

const socialLinks = [
  {
    name: '微信',
    icon: '/images/微信.svg',
    qrCode: '/images/微信.png',
    link: '#',
    type: 'image'
  },
  {
    name: '抖音',
    icon: '/images/抖音.svg',
    qrCode: '/images/抖音.png',
    link: '#',
    type: 'image'
  },
  {
    name: '邮箱',
    icon: '/images/邮箱.svg',
    content: '2784107771@qq.com',
    link: 'mailto:service@huanyu.com',
    type: 'text'
  },
  {
    name: '小红书',
    icon: '/images/小红书.svg',
    qrCode: '/images/小红书.png',
    link: '#',
    type: 'image'
  }
]
</script>

<style scoped>
/* 页脚样式增强 */
ul a {
  display: inline-block;
  transition: all 0.3s ease;
}

ul a:hover {
  transform: translateX(2px);
}

/* 预览功能样式 */
.group .absolute {
  transition: opacity 0.3s ease, visibility 0.3s ease;
}

/* 预览图片样式优化 */
.group img {
  object-fit: contain;
  background-color: white;
}

/* 预览内容居中对齐 */
.group .transform {
  text-align: center;
}
</style>