<template>
  <!-- 页脚组件 -->
  <footer class="bg-gradient-to-r from-pink-50 to-pink-100 text-pink-800 mt-16 border-t border-pink-200 site-footer">
    <div class="container mx-auto px-4 py-12">
      
      <!-- 页脚主要内容 -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-8">
        
        <!-- 品牌介绍 -->
        <div class="col-span-1 md:col-span-2">
          <div class="flex items-center space-x-2 mb-4">
            <!-- 品牌Logo -->
            <img 
              src="/images/huanyu-flower-mark.svg"
              alt="欢雨鲜花Logo" 
              class="w-10 h-10 rounded-full object-cover"
            >
            <h3 class="text-xl font-bold">欢雨flower</h3>
          </div>
          <p class="text-pink-600 leading-relaxed mb-4">
            欢雨鲜花致力于为您提供最优质的花卉产品和服务。每一朵花都经过精心挑选，确保为您传递最真挚的情感和祝福。
          </p>
          
          <!-- 社交媒体链接 -->
          <div class="social-links" aria-label="关注欢雨flower">
            <!-- 微信 -->
            <div class="social-item group">
              <button type="button" class="social-link" aria-label="微信二维码，悬停或点击查看">
                <span class="social-icon social-icon-wechat"><img src="/images/微信.svg" alt="" /></span>
                <span class="social-label">微信</span>
                <span class="social-arrow" aria-hidden="true">↗</span>
              </button>
              <!-- 微信预览图片 -->
              <div class="social-popover" role="tooltip">
                <span class="popover-kicker">扫码添加微信</span>
                <div class="popover-image"><img src="/images/微信.png" alt="微信二维码" /></div>
                <span class="popover-tip">长按保存二维码</span>
              </div>
            </div>
            <!-- 抖音 -->
            <div class="social-item group">
              <button type="button" class="social-link" aria-label="抖音二维码，悬停或点击查看">
                <span class="social-icon social-icon-douyin"><img src="/images/抖音.svg" alt="" /></span>
                <span class="social-label">抖音</span>
                <span class="social-arrow" aria-hidden="true">↗</span>
              </button>
              <!-- 抖音预览图片 -->
              <div class="social-popover" role="tooltip">
                <span class="popover-kicker">关注抖音账号</span>
                <div class="popover-image"><img src="/images/抖音.png" alt="抖音二维码" /></div>
                <span class="popover-tip">长按保存二维码</span>
              </div>
            </div>
            <!-- 邮箱 -->
            <div class="social-item group">
              <a href="mailto:2784107771@qq.com" class="social-link" aria-label="发送邮件">
                <span class="social-icon social-icon-email"><img src="/images/邮箱.svg" alt="" /></span>
                <span class="social-label">邮件</span>
                <span class="social-arrow" aria-hidden="true">↗</span>
              </a>
            </div>
            <!-- 小红书 -->
            <div class="social-item group">
              <button type="button" class="social-link" aria-label="小红书二维码，悬停或点击查看">
                <span class="social-icon social-icon-redbook"><img src="/images/小红书.svg" alt="" /></span>
                <span class="social-label">小红书</span>
                <span class="social-arrow" aria-hidden="true">↗</span>
              </button>
              <!-- 小红书预览图片 -->
              <div class="social-popover" role="tooltip">
                <span class="popover-kicker">关注小红书账号</span>
                <div class="popover-image"><img src="/images/小红书.png" alt="小红书二维码" /></div>
                <span class="popover-tip">长按保存二维码</span>
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
            <li><a href="/terms" class="text-pink-600 hover:text-pink-800 transition-colors">服务条款</a></li>
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
          © {{ currentYear }} 欢雨flower. All rights reserved. |
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
const currentYear = new Date().getFullYear()
</script>

<style scoped>
/* 页脚样式增强 */
.social-links { display: flex; flex-wrap: wrap; gap: .65rem; margin-top: 1.25rem; }
.social-item { position: relative; }
.social-link {
  position: relative; display: inline-flex; align-items: center; gap: .55rem; min-width: 5.6rem;
  padding: .48rem .72rem .48rem .48rem; border: 1px solid rgba(255,255,255,.13); border-radius: 999px;
  background: rgba(255,255,255,.58); color: var(--ink); cursor: pointer; text-decoration: none;
  transition: transform .25s ease, background .25s ease, border-color .25s ease, box-shadow .25s ease;
}
.social-link:hover, .social-link:focus-visible { transform: translateY(-3px); background: #fff; border-color: rgba(211,112,151,.46); box-shadow: 0 10px 22px rgba(169,79,120,.16); outline: none; }
.social-icon { width: 1.9rem; height: 1.9rem; display: grid; place-items: center; border-radius: 50%; background: #fffaf5; box-shadow: 0 3px 8px rgba(0,0,0,.12); }
.social-icon img { width: 1.15rem; height: 1.15rem; object-fit: contain; background: transparent; }
.social-icon-douyin img { width: 1.25rem; height: 1.25rem; }
.social-icon-email img { width: 1.18rem; height: 1.18rem; }
.social-label { color: var(--ink); font-size: .8rem; font-weight: 600; letter-spacing: .03em; }
.social-arrow { color: var(--plum); font-size: .85rem; line-height: 1; opacity: .75; }
.social-popover { position: absolute; z-index: 20; left: 50%; bottom: calc(100% + .8rem); width: 10.5rem; padding: .65rem; border: 1px solid #f0c7b9; border-radius: 1rem; background: #fffaf5; box-shadow: 0 18px 42px rgba(24,10,18,.28); text-align: center; opacity: 0; visibility: hidden; transform: translate(-50%, .5rem); transition: opacity .22s ease, visibility .22s ease, transform .22s ease; pointer-events: none; }
.social-popover::after { content: ''; position: absolute; left: 50%; bottom: -.38rem; width: .75rem; height: .75rem; background: #fffaf5; border-right: 1px solid #f0c7b9; border-bottom: 1px solid #f0c7b9; transform: translateX(-50%) rotate(45deg); }
.social-item:hover .social-popover, .social-item:focus-within .social-popover { opacity: 1; visibility: visible; transform: translate(-50%, 0); pointer-events: auto; }
.popover-kicker { display: block; color: #4a2335; font-size: .72rem; font-weight: 700; margin-bottom: .4rem; }
.popover-image { width: 8.5rem; height: 8.5rem; margin: auto; padding: .35rem; border-radius: .65rem; background: #fff; }
.popover-image img { width: 100%; height: 100%; object-fit: contain; background: #fff; }
.popover-tip { display: block; color: #9a6c78; font-size: .65rem; margin-top: .35rem; }

@media (max-width: 640px) {
  .social-links { gap: .5rem; }
  .social-link { min-width: 5.1rem; padding-right: .6rem; }
  .social-popover { left: 0; transform: translate(0, .5rem); }
  .social-item:hover .social-popover, .social-item:focus-within .social-popover { transform: translate(0, 0); }
  .social-popover::after { left: 2rem; }
}
</style>
