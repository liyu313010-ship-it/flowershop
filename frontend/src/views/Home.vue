<template>
  <PageTransition>
    <div class="min-h-screen">
      <LoadingSpinner :visible="loading" text="正在加载首页..." />
      <!-- 英雄区域 - 主要横幅 -->
      <section class="relative h-96 md:h-[32rem] lg:h-[40rem] overflow-hidden m-4 md:m-8 hero-section">
        <!-- 背景容器 -->
        <div class="absolute inset-0 w-full h-full rounded-2xl">
          <!-- 背景图片 -->
          <transition name="fade" mode="out-in">
            <img 
              v-if="heroBackgroundType === 'image'"
              :key="heroImagePath"
              :src="heroImagePath" 
              alt="欢雨flower横幅" 
              class="absolute inset-0 w-full h-full object-cover object-center hero-image"
              style="object-position: center center; image-rendering: -webkit-optimize-contrast; image-rendering: auto;"
              loading="eager"
              @load="onHeroBackgroundLoad"
              @error="fallbackToDefaultImage"
            />
            
            <!-- 背景视频 -->
            <video 
              v-else
              :key="heroVideoPath"
              ref="heroVideoRef"
              :src="heroVideoPath"
              class="absolute inset-0 w-full h-full object-cover object-center hero-video"
              autoplay
              muted
              loop
              playsinline
              @loadeddata="onHeroBackgroundLoad"
              @error="onHeroBackgroundLoad"
              style="object-position: center center;"
            ></video>
          </transition>
        </div>
        
        
        
        <!-- 文字内容 - 直接显示在背景上，无遮罩 -->
        <div class="absolute inset-0 flex items-center justify-center text-center text-white px-4 rounded-[4rem]">
          <div class="max-w-3xl fade-in bg-black/5 backdrop-blur-sm p-8 rounded-3xl border-2 border-white/30 shadow-2xl hero-content">
            <h1 class="text-4xl md:text-6xl font-bold mb-4 text-shadow">
              欢雨flower
            </h1>
            <p class="text-xl md:text-2xl mb-8 text-shadow">
              用心传递每一份美好，让鲜花为您的生活增添色彩
            </p>
            <div class="flex flex-col sm:flex-row gap-4 justify-center">
              <router-link 
                  to="/products" 
                  class="btn-primary bg-white text-huanyu-pink-300 hover:bg-huanyu-pink-50 font-bold shadow-lg hover:shadow-xl transform hover:scale-105 transition-all duration-200"
                >
                浏览商品
              </router-link>
              <router-link 
                to="/auth" 
                class="btn-secondary border-white text-pink-700 hover:bg-white hover:text-huanyu-pink-500 font-bold shadow-lg hover:shadow-xl transform hover:scale-105 transition-all duration-200"
              >
                立即注册
              </router-link>
            </div>
          </div>
        </div>
      </section>
    
    <!-- 视频展示区域 -->
    <section class="py-16 bg-gradient-to-br from-huanyu-pink-50 to-white">
      <div class="container mx-auto px-4">
        <div class="text-center mb-12">
          <h2 class="text-3xl md:text-4xl font-bold text-huanyu-pink-700 mb-4">
            品牌故事视频
          </h2>
          <p class="text-gray-600 max-w-2xl mx-auto">
            了解欢雨flower的品牌理念和服务承诺
          </p>
        </div>
        
        <!-- 视频播放器 -->
        <div class="max-w-4xl mx-auto">
          <div class="relative rounded-2xl overflow-hidden shadow-2xl">
            <!-- 视频播放器 -->
              <div ref="videoSection" class="relative video-container bg-gray-900 rounded-2xl overflow-hidden w-full" :style="`padding-bottom: ${videoAspectRatio ? getPaddingBottom(videoAspectRatio) : '56.25%'}; min-height: 400px;`">
              <!-- 视频封面图片 -->
              <img 
                src="/images/视屏壁纸图片.png" 
                alt="视频封面" 
                class="absolute inset-0 w-full h-full object-cover hero-image"
                style="image-rendering: -webkit-optimize-contrast; image-rendering: auto; object-position: center; width: 100%; height: 100%;"
                v-if="!videoPlaying"
              />
              
              <!-- 播放按钮覆盖层 -->
              <div 
                v-if="!videoPlaying"
                class="absolute inset-0 bg-black/30 flex items-center justify-center cursor-pointer hover:bg-black/40 transition-colors w-full h-full"
                @click="playVideo"
              >
                <div class="play-button rounded-full p-6 bg-white/90 hover:bg-white transition-colors">
                  <svg class="w-12 h-12 text-huanyu-pink-500" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M8 5v14l11-7z"/>
                  </svg>
                </div>
              </div>
              
              <!-- 视频播放器 -->
              <video 
                v-if="videoPlaying"
                ref="videoPlayer"
                class="absolute inset-0 w-full h-full"
                style="object-fit: cover; background: black;"
                controls
                playsinline
                preload="metadata"
                @loadedmetadata="onVideoLoaded"
                @ended="videoEnded"
                @error="onVideoError"
                @canplay="onVideoCanPlay"
                @loadstart="onVideoLoadStart"
                @progress="onVideoProgress"
                @waiting="onVideoWaiting"
                @playing="onVideoPlaying"
                @touchstart="handleMobileVideoPlay"
                @touchend.prevent="preventDoubleTapZoom">
                <!-- 使用本地视频文件 -->
                <source :src="heroVideoPath" type="video/mp4">
                <p>您的浏览器不支持视频播放</p>
              </video>
              
              <!-- 视频加载失败时的占位符 -->
              <div v-if="!videoPlaying && hasPlayed" class="absolute inset-0 flex items-center justify-center bg-gray-900">
                <div class="text-center text-white">
                  <div class="mb-4">
                    <svg class="w-16 h-16 mx-auto text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z"></path>
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                    </svg>
                  </div>
                  <p class="text-lg font-medium mb-2">视频暂时无法播放</p>
                  <p class="text-sm text-gray-400 mb-4">请稍后重试或联系客服</p>
                  <button 
                    @click="retryVideoLoad"
                    class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg transition-colors"
                  >
                    重新加载
                  </button>
                </div>
              </div>
              
              <!-- 视频信息调试 -->
              <div v-if="videoPlaying && videoDimensions" class="absolute top-2 right-2 bg-black/70 text-white text-xs p-2 rounded z-20">
                <div>分辨率: {{ videoDimensions.width }}x{{ videoDimensions.height }}</div>
                <div>比例: {{ videoAspectRatio }}</div>
              </div>
            </div>
            
            <!-- 视频信息 -->
            <div class="bg-white p-6 border-t">
              <h3 class="text-xl font-semibold text-gray-800 mb-2">欢雨flower - 用心传递每一份美好</h3>
              <p class="text-gray-600 mb-4">
                从花田到花束，从花艺师到配送员，每一个环节我们都用心对待。让我们一起见证鲜花的美丽旅程。
              </p>
              <div class="flex items-center justify-between">
                <div class="flex items-center space-x-4 text-sm text-gray-500">
                  <span>时长: {{ videoDuration || '3:45' }}</span>
                  <span>发布时间: 2024年1月</span>
                  <span v-if="videoDimensions">{{ videoDimensions.width }}x{{ videoDimensions.height }}</span>
                </div>
                <div class="video-controls flex space-x-2">
                  <button 
                    v-if="videoPlaying"
                    @click="pauseVideo"
                    class="bg-huanyu-pink-100 hover:bg-huanyu-pink-200 text-huanyu-pink-600 px-4 py-2 rounded-lg transition-colors"
                  >
                    暂停
                  </button>
                  <button 
                    v-if="!videoPlaying && hasPlayed"
                    @click="replayVideo"
                    class="bg-huanyu-pink-100 hover:bg-huanyu-pink-200 text-huanyu-pink-600 px-4 py-2 rounded-lg transition-colors"
                  >
                    重播
                  </button>
                  <button 
                    @click="shareVideo"
                    class="bg-huanyu-pink-500 hover:bg-huanyu-pink-600 text-white px-4 py-2 rounded-lg transition-colors"
                  >
                    分享
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
    
    <!-- 特色服务区域 -->
    <section class="py-16 bg-white">
      <div class="container mx-auto px-4">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
          
          <!-- 特色卡片1 -->
          <div class="text-center group">
            <div class="w-20 h-20 mx-auto mb-4 bg-huanyu-pink-50 rounded-full flex items-center justify-center group-hover:bg-huanyu-pink-100 transition-colors">
              <svg class="w-10 h-10 text-huanyu-pink-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
            <h3 class="text-xl font-semibold mb-2">当日配送</h3>
            <p class="text-gray-600">下单后最快2小时送达，让爱意及时传递</p>
          </div>
          
          <!-- 特色卡片2 -->
          <div class="text-center group">
            <div class="w-20 h-20 mx-auto mb-4 bg-huanyu-pink-50 rounded-full flex items-center justify-center group-hover:bg-huanyu-pink-100 transition-colors">
              <svg class="w-10 h-10 text-huanyu-pink-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path>
              </svg>
            </div>
            <h3 class="text-xl font-semibold mb-2">品质保证</h3>
            <p class="text-gray-600">精选优质花材，确保每一朵花都新鲜美丽</p>
          </div>
          
          <!-- 特色卡片3 -->
          <div class="text-center group">
            <div class="w-20 h-20 mx-auto mb-4 bg-huanyu-pink-50 rounded-full flex items-center justify-center group-hover:bg-huanyu-pink-100 transition-colors">
              <svg class="w-10 h-10 text-huanyu-pink-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"></path>
              </svg>
            </div>
            <h3 class="text-xl font-semibold mb-2">用心服务</h3>
            <p class="text-gray-600">专业花艺师精心搭配，传递最真挚的情感</p>
          </div>
        </div>
      </div>
    </section>

    <!-- 热门商品区域 -->
    <section class="py-16 bg-gradient-to-br from-huanyu-pink-50 to-white">
      <div class="container mx-auto px-4">
        <div class="text-center mb-12">
          <h2 class="text-3xl md:text-4xl font-bold text-huanyu-pink-700 mb-4">
            flower商品
          </h2>
          <p class="text-gray-600 max-w-2xl mx-auto">
            精选最受欢迎的鲜花产品，每一款都承载着美好的祝福
          </p>
        </div>
        
        <!-- 商品网格 -->
        <div 
          class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6"
          @touchstart="handleTouchStart"
          @touchmove="handleTouchMove"
          @touchend="handleTouchEnd"
        >
          
          <!-- 商品卡片 -->
          <template v-if="featuredProducts.length > 0">
            <div 
              v-for="product in featuredProducts" 
              :key="product.id" 
              class="card group cursor-pointer relative"
              @touchstart="handleCardTouch(product, $event)"
              @touchend.prevent="preventDoubleTapZoom"
            >
            <div class="relative overflow-hidden rounded-xl">
              <!-- 商品图片 -->
              <div class="w-full h-48 bg-gray-50 overflow-hidden relative">
                <img 
                  :src="product.image" 
                  :alt="product.name"
                  class="w-full h-full object-contain transition-transform duration-300 group-hover:scale-110"
                  @error="handleProductImageError($event, product)"
                />
              </div>
              
              <!-- 商品标签 -->
              <span v-if="product.isHot" class="absolute top-2 left-2 bg-red-500 text-white px-2 py-1 text-xs rounded-full z-10">
                热卖
              </span>
              <span v-if="product.isNew" class="absolute top-2 right-2 bg-huanyu-pink-400 text-white px-2 py-1 text-xs rounded-full z-10">
                新品
              </span>
              
              <!-- 悬停操作按钮 -->
              <div class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center space-x-2 z-30 pointer-events-auto">
                <button 
                  @click.stop="quickView(product)"
                  class="w-10 h-10 bg-white rounded-full flex items-center justify-center hover:bg-huanyu-pink-50 transition-colors shadow-lg"
                  title="快速查看"
                >
                  <svg class="w-5 h-5 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
                  </svg>
                </button>
                <button 
                  v-if="!userStore.isAdmin"
                  @click.stop="handleAddToCart(product)"
                  class="w-10 h-10 bg-huanyu-pink-500 text-white rounded-full flex items-center justify-center hover:bg-huanyu-pink-600 transition-colors shadow-lg"
                  title="加入购物车"
                >
                  <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
                  </svg>
                </button>
                
                <button 
                  v-if="!userStore.isAdmin"
                  @click.stop="toggleFavorite(product)"
                  class="w-10 h-10 bg-white rounded-full flex items-center justify-center hover:bg-huanyu-pink-50 transition-colors shadow-lg"
                  :class="isFavorite(product) ? 'text-red-500' : 'text-gray-700'"
                  title="收藏"
                >
                  <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                    <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
                  </svg>
                </button>
              </div>
            </div>
            
            <div class="p-4">
              <h3 class="font-semibold text-lg mb-2 text-gray-800 group-hover:text-huanyu-pink-600 transition-colors">{{ product.name }}</h3>
              <AutoLinkText class="text-gray-600 text-sm mb-3 line-clamp-2" :text="product.description" />
              
              <!-- 评分和销量 -->
              <div class="flex items-center justify-between mb-3">
                <div class="flex items-center space-x-1">
                  <div class="flex text-yellow-400">
                    <svg v-for="i in 5" :key="i" class="w-4 h-4" :class="(product.averageRating || 0) >= i ? 'opacity-100' : 'opacity-30'" fill="currentColor" viewBox="0 0 24 24">
                      <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                    </svg>
                  </div>
                  <span class="text-sm text-gray-500">({{ (product.averageRating || 0).toFixed(1) }})</span>
                </div>
                <span class="text-sm text-gray-500">已售 {{ product.salesCount || 0 }}</span>
              </div>
              
              <!-- 价格和操作 -->
              <div class="flex items-center justify-between">
                <div>
                  <span class="text-xl font-bold text-huanyu-pink-500">¥{{ product.price }}</span>
                  <span v-if="product.originalPrice" class="text-sm text-gray-400 line-through ml-2">¥{{ product.originalPrice }}</span>
                </div>
                <div class="flex items-center space-x-1">
                  <!-- 数量选择器 -->
                  <div v-if="showQuantitySelector[product.id]" class="flex items-center space-x-1 mr-2">
                    <button 
                      @click.stop="decreaseQuantity(product)"
                      class="w-6 h-6 bg-gray-200 hover:bg-gray-300 rounded-full flex items-center justify-center transition-colors"
                    >
                      <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 12H4"></path>
                      </svg>
                    </button>
                    <span class="w-8 text-center text-sm font-medium">{{ getProductQuantity(product) }}</span>
                    <button 
                      @click.stop="increaseQuantity(product)"
                      class="w-6 h-6 bg-gray-200 hover:bg-gray-300 rounded-full flex items-center justify-center transition-colors"
                    >
                      <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path>
                      </svg>
                    </button>
                  </div>
                  
                  <!-- 加入购物车按钮 - 仅对非管理员显示 -->
                  <button 
                    v-if="!userStore.isAdmin"
                    @click.stop="handleAddToCart(product)"
                    class="bg-huanyu-pink-400 hover:bg-huanyu-pink-500 text-white p-2 rounded-full transition-all transform hover:scale-110"
                    :disabled="cartStore.loading"
                    :title="showQuantitySelector[product.id] ? '确认添加' : '加入购物车'"
                  >
                    <svg v-if="!showQuantitySelector[product.id]" class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
                    </svg>
                    <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </div>
          </template>
          
          <!-- 空状态显示 -->
          <template v-else>
            <div class="col-span-full text-center py-12">
              <div class="bg-gray-50 p-8 rounded-xl inline-block">
                <svg class="w-16 h-16 text-gray-300 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" />
                </svg>
                <h3 class="text-xl font-medium text-gray-800 mb-2">暂无商品</h3>
                <p class="text-gray-600">商品数据正在加载中，或暂无可用商品</p>
              </div>
            </div>
          </template>
        </div>
        
        <!-- 查看更多按钮 -->
        <div class="text-center mt-8">
          <router-link 
            to="/products" 
            class="btn-primary font-bold shadow-lg hover:shadow-xl transform hover:scale-105 transition-all duration-200"
          >
            查看更多商品
          </router-link>
        </div>
      </div>
    </section>

    <!-- 推荐商品区域 -->
    <section class="py-16 bg-white">
      <div class="container mx-auto px-4">
        <div class="text-center mb-12">
          <h2 class="text-3xl md:text-4xl font-bold text-huanyu-pink-700 mb-4">
            为你推荐
          </h2>
          <p class="text-gray-600 max-w-2xl mx-auto">
            基于你的偏好与全站热度的推荐花款
          </p>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          <template v-if="recommendedProducts.length > 0">
            <div 
              v-for="product in visibleRecommended" 
              :key="product.id" 
              class="card group cursor-pointer relative"
              @click="goToProduct(product.id)"
              @touchstart="handleCardTouch(product, $event)"
              @touchend.prevent="preventDoubleTapZoom"
            >
              <div class="relative overflow-hidden rounded-xl">
                <div class="w-full h-48 bg-gray-50 overflow-hidden relative">
                  <img 
                    :src="product.image" 
                    :alt="product.name"
                    class="w-full h-full object-contain transition-transform duration-300 group-hover:scale-110"
                    @error="handleProductImageError($event, product)"
                  />
                </div>
                <div class="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center space-x-2 z-30 pointer-events-auto">
                  <button 
                    @click.stop="quickView(product)"
                    class="w-10 h-10 bg-white rounded-full flex items-center justify-center hover:bg-huanyu-pink-50 transition-colors shadow-lg"
                    title="快速查看"
                  >
                    <svg class="w-5 h-5 text-gray-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"></path>
                    </svg>
                  </button>
                  <button 
                    v-if="!userStore.isAdmin"
                    @click.stop="handleAddToCart(product)"
                    class="w-10 h-10 bg-huanyu-pink-500 text-white rounded-full flex items-center justify-center hover:bg-huanyu-pink-600 transition-colors shadow-lg"
                    title="加入购物车"
                  >
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
                    </svg>
                  </button>
                  <button 
                    v-if="!userStore.isAdmin"
                    @click.stop="toggleFavorite(product)"
                    class="w-10 h-10 bg-white rounded-full flex items-center justify-center hover:bg-huanyu-pink-50 transition-colors shadow-lg"
                    :class="isFavorite(product) ? 'text-red-500' : 'text-gray-700'"
                    title="收藏"
                  >
                    <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                      <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
                    </svg>
                  </button>
                </div>
              </div>
              <div class="p-4">
                <h3 class="font-semibold text-lg mb-2 text-gray-800 group-hover:text-huanyu-pink-600 transition-colors">{{ product.name }}</h3>
                <p class="text-gray-600 text-sm mb-3 line-clamp-2">{{ product.description }}</p>
                <div class="flex items-center justify-between mb-3">
                  <span class="text-sm text-gray-500">已售 {{ product.salesCount || 0 }}</span>
                </div>
                <div class="flex items-center justify-between">
                  <span class="text-xl font-bold text-huanyu-pink-500">¥{{ product.price }}</span>
                  <button 
                    v-if="!userStore.isAdmin"
                    @click.stop="handleAddToCart(product)"
                    class="bg-huanyu-pink-400 hover:bg-huanyu-pink-500 text-white p-2 rounded-full transition-all transform hover:scale-110"
                    :disabled="cartStore.loading"
                  >
                    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path>
                    </svg>
                  </button>
                </div>
              </div>
            </div>
          </template>
          <template v-else>
            <div class="col-span-full text-center text-gray-500">暂无推荐</div>
          </template>
        </div>
        <div class="text-center mt-6" v-if="recommendedProducts.length > 4">
          <button v-if="recommendedLimit <= 4" @click="recommendedLimit = recommendedProducts.length" class="btn-primary font-bold shadow-lg hover:shadow-xl transform hover:scale-105 transition-all duration-200">
            查看更多推荐产品
          </button>
          <button v-else @click="recommendedLimit = 4" class="btn-secondary font-bold shadow-lg hover:shadow-xl transform hover:scale-105 transition-all duration-200">
            收起推荐
          </button>
        </div>
      </div>
    </section>
    
    <!-- 品牌故事区域 -->
    <section class="py-16 bg-white">
      <div class="container mx-auto px-4">
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
          
          <!-- 左侧文字内容 -->
          <div>
            <h2 class="text-3xl md:text-4xl font-bold text-huanyu-pink-700 mb-6">
              关于欢雨flower
            </h2>
            <div class="space-y-4 text-gray-600">
              <p>
                欢雨flower成立于2025年，我们致力于为每一位客户提供最优质的鲜花产品和服务。从一朵玫瑰到一束精心搭配的花束，我们都用心对待。
              </p>
              <p>
                我们相信，鲜花不仅仅是装饰品，更是情感的载体。无论是生日、纪念日、表白还是道歉，一束恰到好处的鲜花都能传递您内心最真挚的情感。
              </p>
              <p>
                我们的花艺师都经过专业培训，拥有丰富的插花经验。每一束花都经过精心设计和搭配，确保为您呈现出最美的效果。
              </p>
            </div>
            
            <!-- 品牌数据 -->
            <div class="grid grid-cols-3 gap-4 mt-8">
              <div class="text-center">
                <div class="text-2xl font-bold text-huanyu-pink-500">10000+</div>
                <div class="text-sm text-gray-600">满意客户</div>
              </div>
              <div class="text-center">
                <div class="text-2xl font-bold text-huanyu-pink-500">50+</div>
                <div class="text-sm text-gray-600">花品种类</div>
              </div>
              <div class="text-center">
                <div class="text-2xl font-bold text-huanyu-pink-500">4.9</div>
                <div class="text-sm text-gray-600">用户评分</div>
              </div>
            </div>
          </div>
          
          <!-- 右侧图片 -->
          <div class="relative">
            <img 
              src="/images/封面3.png" 
              alt="欢雨花店" 
              class="rounded-2xl shadow-2xl w-full h-auto object-contain bg-gray-50"
              style="image-rendering: -webkit-optimize-contrast; image-rendering: crisp-edges; image-rendering: pixelated;"
              loading="lazy"
            >
            <!-- 装饰元素 -->
            <div class="absolute -bottom-6 -left-6 w-32 h-32 bg-huanyu-pink-100 rounded-full opacity-50 -z-10"></div>
            <div class="absolute -top-6 -right-6 w-24 h-24 bg-huanyu-pink-200 rounded-full opacity-50 -z-10"></div>
          </div>
        </div>
      </div>
    </section>
    
    <!-- 客户评价区域 -->
    <section class="py-16 bg-gradient-to-br from-huanyu-pink-50 to-white">
      <div class="container mx-auto px-4">
        <div class="text-center mb-12">
          <h2 class="text-3xl md:text-4xl font-bold text-huanyu-pink-800 mb-4">
            客户评价
          </h2>
          <p class="text-gray-600">听听客户对我们的评价</p>
        </div>
        
        <!-- 评价卡片 -->
        <div v-if="!reviews.length" class="text-center py-12 text-gray-500">
          <div class="text-4xl mb-2">💬</div>
          <p>暂无评价</p>
        </div>
        <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
          <div v-for="review in visibleReviews" :key="review.id" class="bg-white p-6 rounded-xl shadow-lg border border-gray-100 hover:shadow-xl transition">
            <div class="flex items-center mb-4">
              <!-- 用户头像 -->
              <img 
                :src="review.avatar"
                :alt="review.userName"
                class="w-12 h-12 rounded-full mr-3 ring-2 ring-huanyu-pink-200 object-cover"
                @error="handleAvatarError"
              />
              <div>
                <h4 class="font-semibold">{{ review.userName }}</h4>
                <div class="flex text-yellow-400">
                  <!-- 星级评分 -->
                  <span v-for="star in 5" :key="star" class="text-sm">
                    {{ star <= review.rating ? '★' : '☆' }}
                  </span>
                </div>
              </div>
            </div>
            <AutoLinkText class="text-gray-700 leading-relaxed line-clamp-3 mt-1" :text="review.comment" />
            <div class="mt-2 text-xs text-gray-500 flex flex-wrap gap-2">
              <span class="px-2 py-0.5 bg-gray-100 rounded-full">评价ID: {{ review.id || review.Id }}</span>
              <span class="px-2 py-0.5 bg-gray-100 rounded-full">产品ID: {{ review.productId || review.ProductId }}</span>
              <span class="px-2 py-0.5 bg-gray-100 rounded-full">时间: {{ formatReviewDate(review.createdAt) }}</span>
            </div>
            <a href="#" @click.prevent="toggleReviewDetail(review)" class="mt-4 inline-block text-huanyu-pink-600 hover:text-huanyu-pink-700 hover:underline">查看评价详情</a>
            <div v-if="activeReviewId === (review.id || review.Id)" class="mt-3 border-t pt-3">
              <div class="text-sm text-gray-500 mb-2">{{ formatReviewDate(review.createdAt) }}</div>
              <div class="text-gray-700">{{ activeReviewDetail?.Comment || activeReviewDetail?.comment || review.comment }}</div>
            </div>
          </div>
        </div>
        <div v-if="reviews.length > 3" class="text-center mt-6">
          <button @click="toggleShowAllReviews" class="px-4 py-2 border rounded-lg hover:bg-gray-50">
            {{ showAllReviews ? '收起评价' : `查看更多评价（剩余 ${reviews.length - 3} 条）` }}
          </button>
        </div>
      </div>
    </section>
    
  <!-- 快速查看模态框 -->
  <div 
    v-if="showQuickViewModal && quickViewProduct" 
    class="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4"
    @click="closeQuickView"
  >
    <div 
      class="bg-white rounded-2xl max-w-4xl w-full max-h-[90vh] overflow-y-auto"
      @click.stop
    >
      <!-- 模态框头部 -->
      <div class="sticky top-0 bg-white border-b px-6 py-4 flex items-center justify-between">
        <h3 class="text-xl font-semibold text-gray-800">商品详情</h3>
        <button 
          @click="closeQuickView"
          class="text-gray-400 hover:text-gray-600 transition-colors"
        >
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
          </svg>
        </button>
      </div>
      
      <!-- 模态框内容 -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6 p-6">
        <!-- 左侧图片 -->
        <div class="space-y-4">
          <div class="aspect-square bg-gray-100 rounded-xl overflow-hidden">
            <img 
                :src="quickViewProduct.image" 
                :alt="quickViewProduct.name"
                class="w-full h-full object-cover"
                @error="handleQuickViewImageError($event)"
              >
          </div>
          
          <!-- 缩略图 -->
          <div class="grid grid-cols-4 gap-2">
            <div v-for="i in 4" :key="i" class="aspect-square bg-gray-100 rounded-lg overflow-hidden cursor-pointer hover:ring-2 hover:ring-huanyu-pink-400 transition-all">
              <div 
                :style="{ backgroundImage: `url(${quickViewProduct.image})` }"
                :alt="`${quickViewProduct.name} ${i}`"
                class="w-full h-full bg-center bg-cover"
                @error="handleQuickViewImageError($event)"
              ></div>
            </div>
          </div>
        </div>
        
        <!-- 右侧信息 -->
        <div class="space-y-6">
          <!-- 商品标题 -->
          <div>
            <h2 class="text-2xl font-bold text-gray-800 mb-2">{{ quickViewProduct.name }}</h2>
            <AutoLinkText class="text-gray-600" :text="quickViewProduct.description" />
          </div>
          
          <!-- 评分 -->
          <div class="flex items-center space-x-4">
            <div class="flex text-yellow-400">
              <svg v-for="i in 5" :key="i" class="w-5 h-5" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
              </svg>
            </div>
            <span class="text-gray-600">{{ (quickViewProduct.averageRating || 0).toFixed(1) }}分</span>
            <span class="text-gray-400">|</span>
            <span class="text-gray-600">已售 {{ quickViewProduct.salesCount || 0 }} 件</span>
          </div>
          
          <!-- 价格 -->
          <div class="flex items-baseline space-x-3">
            <span class="text-3xl font-bold text-huanyu-pink-500">¥{{ quickViewProduct.price }}</span>
            <span v-if="quickViewProduct.originalPrice" class="text-lg text-gray-400 line-through">¥{{ quickViewProduct.originalPrice }}</span>
          </div>
          
          <!-- 商品特点 -->
          <div class="space-y-2">
            <div class="flex items-center space-x-2">
              <svg class="w-5 h-5 text-green-500" fill="currentColor" viewBox="0 0 24 24">
                <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
              <span class="text-gray-700">优质花材，新鲜保证</span>
            </div>
            <div class="flex items-center space-x-2">
              <svg class="w-5 h-5 text-green-500" fill="currentColor" viewBox="0 0 24 24">
                <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
              <span class="text-gray-700">专业花艺师精心搭配</span>
            </div>
            <div class="flex items-center space-x-2">
              <svg class="w-5 h-5 text-green-500" fill="currentColor" viewBox="0 0 24 24">
                <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
              <span class="text-gray-700">同城配送，快速送达</span>
            </div>
          </div>
          
          <!-- 数量选择 -->
          <div class="flex items-center space-x-4">
            <span class="text-gray-700">数量：</span>
            <div class="flex items-center space-x-2">
              <button 
                @click="decreaseQuantity(quickViewProduct)"
                class="w-8 h-8 bg-gray-200 hover:bg-gray-300 rounded-full flex items-center justify-center transition-colors"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 12H4"></path>
                </svg>
              </button>
              <span class="w-12 text-center font-medium">{{ getProductQuantity(quickViewProduct) }}</span>
              <button 
                @click="increaseQuantity(quickViewProduct)"
                class="w-8 h-8 bg-gray-200 hover:bg-gray-300 rounded-full flex items-center justify-center transition-colors"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path>
                </svg>
              </button>
            </div>
          </div>
          
          <!-- 操作按钮 -->
          <div class="flex space-x-4">
            <!-- 仅对非管理员显示加入购物车按钮 -->
            <button 
              v-if="!userStore.isAdmin"
              @click="handleAddToCart(quickViewProduct); closeQuickView()"
              class="flex-1 bg-huanyu-pink-500 hover:bg-huanyu-pink-600 text-white py-3 rounded-xl font-medium transition-colors"
            >
              加入购物车
            </button>
            <button 
              @click="goToProduct(quickViewProduct.id); closeQuickView()"
              class="px-6 py-3 border border-gray-300 rounded-xl hover:bg-gray-50 transition-colors text-gray-600"
            >
              查看详情
            </button>
            <button 
              @click="toggleFavorite(quickViewProduct)"
              class="px-6 py-3 border border-gray-300 rounded-xl hover:bg-gray-50 transition-colors"
              :class="isFavorite(quickViewProduct) ? 'text-red-500 border-red-300' : 'text-gray-600'"
            >
              <svg class="w-6 h-6" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z"/>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
  
  

  </div>
  </PageTransition>
</template>

<script setup>
import { ref, onMounted, nextTick, watch, onUnmounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { notifySuccess, notifyError, notifyInfo } from '@/utils/notify'
import { useCartStore } from '@/stores/cart'
import { useProductStore } from '@/stores/product'
import { useUserStore } from '@/stores/user'
import { productService } from '@/services/product'
 
import { recommendationService } from '@/services/recommendation'
import { favoriteService } from '@/services/favorite'
import { getAvatarUrl, handleAvatarError, getProductImageUrl } from '@/utils/avatar.js'
import LoadingSpinner from '@/components/LoadingSpinner.vue'
import PageTransition from '@/components/PageTransition.vue'
import AutoLinkText from '@/components/AutoLinkText.vue'
import api from '@/services/api'
import adminService from '@/services/adminService'

// 路由和状态管理
const router = useRouter()
const cartStore = useCartStore()

// 页面加载状态
const isLoading = ref(true)
const heroImageLoaded = ref(false)
const productStore = useProductStore()
const userStore = useUserStore()

// 添加图片加载失败的回退处理
const fallbackToDefaultImage = () => {
  heroImagePath.value = '/images/主图.jpg'
}

// 英雄区域背景设置 - 这里可以修改默认背景路径
const heroBackgroundType = ref('image')
const heroImagePath = ref('/images/主页背景.png') // 修改这里的路径来更改默认图片背景
const heroVideoPath = ref('/videos/2.mp4') // 设置视频背景路径
const heroVideoRef = ref(null)
const heroBackgroundLoaded = ref(false)

// 响应式数据
const featuredProducts = ref([])
const recommendedProducts = ref([])
const recommendedLimit = ref(4)
const visibleRecommended = computed(() => (recommendedProducts.value || []).slice(0, recommendedLimit.value))
const reviews = ref([])
const REVIEW_POLL_MS = 600000
let reviewsPoll = null
const lastReviewsFetchAt = ref(0)
let reviewsFetching = false
let reviewsController = null
 
// 直接使用store中的categories，不创建本地副本

// 获取所有评价（去重与防抖）
const fetchReviews = async () => {
  if (reviewsFetching) return
  const now = Date.now()
  if (now - lastReviewsFetchAt.value < 1200) return
  reviewsFetching = true
  try {
    if (reviewsController) { try { reviewsController.abort() } catch {} }
    reviewsController = new AbortController()
    const response = await productService.getAllReviews({ pageSize: 10 }, { signal: reviewsController.signal })
    const data = response?.data || response || []
    const reviewList = Array.isArray(data) ? data : (data.Items || [])
    reviews.value = (reviewList || []).map(review => ({
      ...review,
      userName: review.UserName || review.userName,
      comment: review.Comment || review.comment || '',
      rating: review.Rating || review.rating || 0,
      avatar: getAvatarUrl(review.Avatar || review.avatar || review.UserName || review.userName || ''),
      id: review.Id || review.id,
      userId: review.UserId || review.userId,
      productId: review.ProductId || review.productId,
      createdAt: review.CreatedAt || review.createdAt,
      updatedAt: review.UpdatedAt || review.updatedAt
    }))
  } catch {
    reviews.value = []
  } finally {
    lastReviewsFetchAt.value = Date.now()
    reviewsFetching = false
  }
}

const startReviewsPolling = () => {
  stopReviewsPolling()
  reviewsPoll = setInterval(() => { fetchReviews() }, REVIEW_POLL_MS)
}

const stopReviewsPolling = () => {
  if (reviewsPoll) { clearInterval(reviewsPoll); reviewsPoll = null }
  if (reviewsController) { try { reviewsController.abort() } catch {} }
}

const handleVisibilityReview = () => {
  if (document.hidden) {
    stopReviewsPolling()
  } else {
    // 防抖：避免在快速切换焦点时触发多次
    const now = Date.now()
    if (now - lastReviewsFetchAt.value > 1500) {
      fetchReviews()
    }
    startReviewsPolling()
  }
}

const goToReviewDetail = async (review) => {
  const rid = review.id || review.Id
  const pid = review.productId || review.ProductId
  if (!rid || !pid) { notifyError('评价信息缺失'); return }
  try {
    await router.push(`/reviews/${rid}?productId=${pid}`)
  } catch (e) {
    window.location.href = `/reviews/${rid}?productId=${pid}`
  }
}

const activeReviewId = ref(null)
const activeReviewDetail = ref(null)

const showAllReviews = ref(false)
const visibleReviews = computed(() => {
  const list = reviews.value || []
  return showAllReviews.value ? list : list.slice(0, 3)
})
const toggleShowAllReviews = () => { showAllReviews.value = !showAllReviews.value }

const toggleReviewDetail = async (review) => {
  const rid = review.id || review.Id
  if (!rid) return
  if (activeReviewId.value === rid) {
    activeReviewId.value = null
    activeReviewDetail.value = null
    return
  }
  activeReviewId.value = rid
  try {
    const r = await productService.getReviewById(rid)
    const d = r?.data || r
    activeReviewDetail.value = d
  } catch {
    activeReviewDetail.value = null
  }
}

const canDeleteReview = (review) => {
  const uid = userStore.user?.id
  const reviewUserId = review.userId || review.UserId
  return userStore.isAdmin || (!!uid && !!reviewUserId && uid === reviewUserId)
}

const deleteReview = async (review) => {
  const rid = review.id || review.Id
  if (!rid) { notifyError('评价ID缺失'); return }
  const ok = window.confirm('确认删除该评价？')
  if (!ok) return
  try {
    await productService.deleteProductReview(rid)
    reviews.value = reviews.value.filter(r => (r.id || r.Id) !== rid)
    notifySuccess('评价已删除')
  } catch (e) {
    notifyError(e?.message || '删除失败')
  }
}





const formatReviewDate = (val) => {
  if (!val) return ''
  try {
    return new Date(val).toLocaleString('zh-CN', {
      year: 'numeric', month: '2-digit', day: '2-digit',
      hour: '2-digit', minute: '2-digit', second: '2-digit',
      timeZone: 'Asia/Shanghai'
    })
  } catch { return '' }
}

// 加载分类数据
const loadCategories = async () => {
  try {
    await productStore.fetchCategories()
  } catch (error) {
    // 如果加载失败，使用默认数据
    productStore.categories = [
      {
        id: 1,
        name: '玫瑰',
        description: '浪漫爱情的象征'
      },
      {
        id: 2,
        name: '康乃馨',
        description: '温馨感恩的选择'
      },
      {
        id: 3,
        name: '百合',
        description: '纯洁优雅的代表'
      },
      {
        id: 4,
        name: '郁金香',
        description: '高贵优雅的花中皇后'
      }
    ]
  }
}

// 加载特色商品
const loadFeaturedProducts = async () => {
  try {
    // 使用新的首页商品API，只获取前4个商品
    const response = await productService.getHomeProducts()
    
    // 处理API响应数据，兼容不同的响应格式
    let products = []
    if (Array.isArray(response)) {
      products = response
    } else if (response.data && Array.isArray(response.data)) {
      products = response.data
    } else {
      // 如果响应格式不符合预期，尝试直接使用response
      products = response || []
    }
    
    // 转换数据格式，确保正确映射数据库字段
    const apiProducts = products.map(product => {
      // 处理图片URL，确保是完整有效的URL
      let imageUrl = product.ImageUrl || product.imageUrl || product.image || ''
      
      // 修复不完整的URL，如"http://localhost:5"
      if (imageUrl && imageUrl.startsWith('http')) {
        // 检查是否是有效的HTTP URL
        try {
          new URL(imageUrl);
          // 有效URL，保留原样
        } catch {
          // 无效URL，使用默认图片
          imageUrl = '/images/default-product.svg';
        }
      } else if (!imageUrl || imageUrl.trim() === '') {
        // 空URL，使用默认图片
        imageUrl = '/images/default-product.svg';
      }
      // 相对路径保持原样
      
      return {
        id: product.Id || product.id,
        name: product.Name || product.name || '未命名商品',
        description: product.Description || product.description || '暂无描述',
        price: product.Price || product.price || 0,
        image: getProductImageUrl(imageUrl),
        isHot: (product.SalesCount || product.salesCount || 0) > 10,
        isNew: product.CreatedAt ? new Date(product.CreatedAt) > new Date(Date.now() - 7 * 24 * 60 * 60 * 1000) : false,
        salesCount: product.SalesCount || product.salesCount || 0,
        averageRating: product.AverageRating || product.averageRating || 0,
        reviewCount: product.ReviewCount || product.reviewCount || 0,
        // 添加额外字段用于完整展示
        originalPrice: null, // 可以根据需要从数据库映射
        isActive: product.IsActive || product.isActive || true,
        stock: product.Stock || product.stock || 0
      }
    })
    
    // 使用API返回的商品数据，确保只取前4个
    featuredProducts.value = apiProducts.slice(0, 4)
    
  } catch (error) {
    // 只在API完全不可用时才显示空状态，不再使用默认模拟数据
    featuredProducts.value = []
  }
}

// 加载推荐商品（登录用户用个性化，未登录用全站推荐）
const loadRecommendedProducts = async () => {
  try {
    let res
    const token = localStorage.getItem('token')
    if (token) {
      res = await recommendationService.getUser(8)
    } else {
      res = await recommendationService.getGlobal(8)
    }
    const items = res?.data || res || []
    const ids = items.map(i => i.productId || i.ProductId || i.id || i.Id).filter(Boolean)
        const details = await Promise.all(ids.map(async (id) => {
      try {
        const r = await productService.getProductById(id)
        const p = r?.data || r
        return {
          id: p.Id || p.id,
          name: p.Name || p.name || '未命名商品',
          description: p.Description || p.description || '暂无描述',
          price: p.Price || p.price || 0,
          image: getProductImageUrl(p.ImageUrl || p.imageUrl || p.image || ''),
          salesCount: p.SalesCount || p.salesCount || 0,
          popularity: p.Popularity || p.popularity || 0,
          averageRating: p.AverageRating || p.averageRating || 0,
          reviewCount: p.ReviewCount || p.reviewCount || 0
        }
      } catch { return null }
    }))
    const list = details.filter(Boolean)
  if (list.length > 0) {
      recommendedProducts.value = list
  } else {
    // 回退到管理员设置的首页推荐/特色商品
    const f = await productService.getFeaturedProducts()
    const fp = Array.isArray(f?.data) ? f.data : (Array.isArray(f) ? f : [])
    if (fp.length > 0) {
      recommendedProducts.value = fp.slice(0, 8).map(p => ({
        id: p.Id || p.id,
        name: p.Name || p.name || '未命名商品',
        description: p.Description || p.description || '暂无描述',
        price: p.Price || p.price || 0,
        image: getProductImageUrl(p.ImageUrl || p.imageUrl || p.image || ''),
        salesCount: p.SalesCount || p.salesCount || 0,
        popularity: p.Popularity || p.popularity || 0,
        averageRating: p.AverageRating || p.averageRating || 0,
        reviewCount: p.ReviewCount || p.reviewCount || 0
      }))
    } else {
      const h = await productService.getHomeProducts()
      const hp = Array.isArray(h?.data) ? h.data : (Array.isArray(h) ? h : [])
      if (hp.length > 0) {
        recommendedProducts.value = hp.slice(0, 8).map(p => ({
          id: p.Id || p.id,
          name: p.Name || p.name || '未命名商品',
          description: p.Description || p.description || '暂无描述',
          price: p.Price || p.price || 0,
          image: (p.ImageUrl || p.imageUrl || p.image || '/images/default-product.svg'),
          salesCount: p.SalesCount || p.salesCount || 0,
          popularity: p.Popularity || p.popularity || 0,
          averageRating: p.AverageRating || p.averageRating || 0,
          reviewCount: p.ReviewCount || p.reviewCount || 0
        }))
      } else {
        const all = await productService.getProducts({ Page: 1, PageSize: 12 })
        const ap = all?.data?.items || all?.data || (Array.isArray(all) ? all : [])
        recommendedProducts.value = (ap || []).slice(0, 8).map(p => ({
          id: p.Id || p.id,
          name: p.Name || p.name || '未命名商品',
          description: p.Description || p.description || '暂无描述',
          price: p.Price || p.price || 0,
          image: getProductImageUrl(p.ImageUrl || p.imageUrl || p.image || ''),
          salesCount: p.SalesCount || p.salesCount || 0,
          popularity: p.Popularity || p.popularity || 0,
          averageRating: p.AverageRating || p.averageRating || 0,
          reviewCount: p.ReviewCount || p.reviewCount || 0
        }))
      }
    }
  }
  } catch (e) {
    try {
      const f = await productService.getFeaturedProducts()
      const fp = Array.isArray(f?.data) ? f.data : (Array.isArray(f) ? f : [])
      if (fp.length > 0) {
        recommendedProducts.value = fp.slice(0, 8).map(p => ({
          id: p.Id || p.id,
          name: p.Name || p.name || '未命名商品',
          description: p.Description || p.description || '暂无描述',
          price: p.Price || p.price || 0,
          image: (p.ImageUrl || p.imageUrl || p.image || '/images/default-product.svg'),
          salesCount: p.SalesCount || p.salesCount || 0,
          popularity: p.Popularity || p.popularity || 0,
          averageRating: p.AverageRating || p.averageRating || 0,
          reviewCount: p.ReviewCount || p.reviewCount || 0
        }))
        return
      }
      const h = await productService.getHomeProducts()
      const hp = Array.isArray(h?.data) ? h.data : (Array.isArray(h) ? h : [])
      if (hp.length > 0) {
        recommendedProducts.value = hp.slice(0, 8).map(p => ({
          id: p.Id || p.id,
          name: p.Name || p.name || '未命名商品',
          description: p.Description || p.description || '暂无描述',
          price: p.Price || p.price || 0,
          image: (p.ImageUrl || p.imageUrl || p.image || '/images/default-product.svg'),
          salesCount: p.SalesCount || p.salesCount || 0,
          popularity: p.Popularity || p.popularity || 0,
          averageRating: p.AverageRating || p.averageRating || 0,
          reviewCount: p.ReviewCount || p.reviewCount || 0
        }))
        return
      }
      const all = await productService.getProducts({ Page: 1, PageSize: 12 })
      const ap = all?.data?.items || all?.data || (Array.isArray(all) ? all : [])
      recommendedProducts.value = (ap || []).slice(0, 8).map(p => ({
        id: p.Id || p.id,
        name: p.Name || p.name || '未命名商品',
        description: p.Description || p.description || '暂无描述',
        price: p.Price || p.price || 0,
        image: getProductImageUrl(p.ImageUrl || p.imageUrl || p.image || ''),
        salesCount: p.SalesCount || p.salesCount || 0,
        popularity: p.Popularity || p.popularity || 0,
        averageRating: p.AverageRating || p.averageRating || 0,
        reviewCount: p.ReviewCount || p.reviewCount || 0
      }))
    } catch {
      recommendedProducts.value = []
    }
  }

  try {
    const rankingRes = await adminService.getProductSalesRanking(100)
    const ranking = rankingRes?.data || rankingRes || []
    const salesMap = new Map((Array.isArray(ranking) ? ranking : []).map(r => [
      r.productId || r.ProductId || r.id || r.Id,
      r.salesCount || r.SalesCount || 0
    ]))
    if (salesMap.size > 0) {
      recommendedProducts.value = (recommendedProducts.value || []).map(p => ({
        ...p,
        salesCount: salesMap.has(p.id) ? salesMap.get(p.id) : (p.salesCount || 0)
      }))
    }
  } catch {}
}

// 英雄区域背景加载处理
const onHeroBackgroundLoad = () => {
  heroBackgroundLoaded.value = true
  // 延迟隐藏加载动画，确保所有内容都已加载
  setTimeout(() => {
    isLoading.value = false
  }, 800)
}

// 监听背景类型变化
watch(heroBackgroundType, () => {
  heroBackgroundLoaded.value = false
})

// 自动播放视频
const playHeroVideo = async () => {
  if (heroVideoRef.value && heroBackgroundType.value === 'video') {
    try {
      await heroVideoRef.value.play()
    } catch (error) {
      // 视频自动播放失败，需要用户交互
    }
  }
}

// 根据文件路径自动判断背景类型
const determineBackgroundType = (path) => {
  const videoExtensions = ['.mp4', '.webm', '.ogg']
  const isVideo = videoExtensions.some(ext => path.toLowerCase().endsWith(ext))
  return isVideo ? 'video' : 'image'
}

// 设置英雄区域背景
const setHeroBackground = (path) => {
  const type = determineBackgroundType(path)
  heroBackgroundType.value = type
  if (type === 'video') {
    heroVideoPath.value = path
  } else {
    heroImagePath.value = path
  }
  heroBackgroundLoaded.value = false
}

// 切换背景类型
const toggleBackgroundType = () => {
  heroBackgroundType.value = heroBackgroundType.value === 'image' ? 'video' : 'image'
}

// 背景切换函数 - 点击控制面板按钮时调用
const useDefaultImageBackground = () => {
  heroBackgroundType.value = 'image'
  heroImagePath.value = '/images/主图.jpg' // 与上面的默认路径保持一致
}

const useDefaultVideoBackground = () => {
  heroBackgroundType.value = 'video'
  heroVideoPath.value = '/videos/2.mp4'
}

// 组件挂载时加载数据
onMounted(async () => {
  // 设置超时以确保即使图片加载失败也能显示内容
  setTimeout(() => { if (isLoading.value) { isLoading.value = false } }, 3000)

  try {
    await loadCategories()
    await loadFeaturedProducts()
    // 延迟加载推荐与评价，降低并发峰值
    const schedule = (fn, ms) => {
      if ('requestIdleCallback' in window) {
        // @ts-ignore
        requestIdleCallback(() => setTimeout(fn, 0), { timeout: ms })
      } else {
        setTimeout(fn, ms)
      }
    }
    schedule(() => { loadRecommendedProducts() }, 400)
    schedule(() => { fetchReviews(); startReviewsPolling() }, 800)

    document.addEventListener('visibilitychange', handleVisibilityReview)

    try {
      const token = localStorage.getItem('token')
      if (token) {
        const favRes = await favoriteService.list(1, 100)
        const items = favRes?.data || favRes || []
        favoriteProducts.value = items.map(i => i.productId || i.ProductId || i.id || i.Id).filter(Boolean)
      } else {
        favoriteProducts.value = []
      }
    } catch {}
  } catch (error) {
    console.error('Failed to load initial data:', error)
  }

  // 添加移动端滚动优化
  if (typeof window !== 'undefined') {
    window.addEventListener('scroll', handleScroll, { passive: true })
    window.addEventListener('touchend', preventDoubleTapZoom, { passive: false })
    nextTick(() => { playHeroVideo() })
  }
})

watch(() => userStore.user?.avatar, () => { fetchReviews() })

onUnmounted(() => {
  stopReviewsPolling()
  document.removeEventListener('visibilitychange', handleVisibilityReview)
})

if (import.meta && import.meta.hot) {
  import.meta.hot.dispose(() => {
    stopReviewsPolling()
  })
}

// 方法
const goToProduct = async (productId) => {
  try {
    await router.push(`/product/${productId}`)
  } catch (error) {
    console.error('跳转到商品详情失败:', error)
    window.location.href = `/product/${productId}`
  }
}

const goToCategory = async (categoryId) => {
  try {
    // 所有分类都跳转到商品列表页面
    await router.push(`/products?category=${categoryId}`)
  } catch (error) {
    console.error('跳转到分类页面失败:', error)
    window.location.href = `/products?category=${categoryId}`
  }
}

const addToCart = async (product) => {
  const token = localStorage.getItem('token')
  if (!token) {
    notifyInfo('请先登录再加入购物车')
    router.push('/auth')
    return
  }
  if (!cartStore.loading) {
    const result = await cartStore.addToCart(product)
    if (result.success) {
      notifySuccess(result.message || '已加入购物车')
    } else {
      notifyError(result.message || '加入购物车失败')
    }
  }
}

// 视频控制
const videoPlaying = ref(false)
const hasPlayed = ref(false)
const videoPlayer = ref(null)
const videoSection = ref(null)
const videoAspectRatio = ref('')
const videoDimensions = ref(null)
const videoDuration = ref('')

// 商品相关状态
const showQuantitySelector = ref({})
const productQuantities = ref({})
const favoriteProducts = ref([])
const quickViewProduct = ref(null)
const showQuickViewModal = ref(false)
 

const retryVideoLoad = () => {
  videoDimensions.value = null
  videoAspectRatio.value = ''
  videoDuration.value = ''
  hasPlayed.value = false
  
  if (videoPlayer.value) {
    videoPlayer.value.load()
    playVideo()
  }
}

// 视频控制方法
const playVideo = () => {
  if (videoPlaying.value) {
    pauseVideo()
    return
  }
  
  videoPlaying.value = true
  hasPlayed.value = true
  
  nextTick(() => {
    if (videoPlayer.value) {
      // 尝试自动播放视频
      const playPromise = videoPlayer.value.play()
      
      if (playPromise !== undefined) {
        playPromise.then(() => {
          showNotification('视频开始播放', 'success')
        }).catch((error) => {
          showNotification('请点击视频播放按钮开始播放', 'info')
        })
      }
    }
  })
}

const onVideoWaiting = () => {
  // 视频缓冲中...
}

const onVideoPlaying = () => {
  // 视频正在播放
}

const onVideoLoadStart = () => {
  // 视频开始加载
}

const onVideoProgress = (event) => {
  const video = event.target
  if (video.buffered.length > 0) {
    const bufferedEnd = video.buffered.end(video.buffered.length - 1)
    const duration = video.duration
    const bufferedPercentage = (bufferedEnd / duration) * 100
    // 视频缓冲进度: ${bufferedPercentage.toFixed(2)}%
  }
}

const onVideoCanPlay = () => {
  showNotification('视频加载完成', 'success')
}

const pauseVideo = () => {
  if (videoPlayer.value) {
    videoPlayer.value.pause()
  }
  videoPlaying.value = false
}

const replayVideo = () => {
  if (videoPlayer.value) {
    videoPlayer.value.currentTime = 0
    videoPlayer.value.play()
  }
  videoPlaying.value = true
}

const videoEnded = () => {
  // 完全重置所有视频状态和数据，确保完全恢复到初始状态
  videoPlaying.value = false
  hasPlayed.value = false
  videoAspectRatio.value = ''
  videoDimensions.value = null
  videoDuration.value = ''
  
  // 重置视频播放器
  if (videoPlayer.value) {
    videoPlayer.value.currentTime = 0
  }
  
  // 显示完成提示
  showNotification('视频播放完成，已返回页面', 'success')
  
  // 延迟滚动到视频section，让用户看到提示
  setTimeout(() => {
    // 优先使用ref定位到视频section
    if (videoSection.value) {
      videoSection.value.scrollIntoView({ behavior: 'smooth', block: 'center' })
    } else {
      // 备用方案：滚动到品牌故事视频section
      const brandStorySection = document.querySelector('[class*="品牌故事视频"]')?.closest('section')
      if (brandStorySection) {
        brandStorySection.scrollIntoView({ behavior: 'smooth', block: 'start' })
      } else {
        // 最后备用：滚动到视频容器
        const videoContainer = document.querySelector('.video-container')
        if (videoContainer) {
          videoContainer.scrollIntoView({ behavior: 'smooth', block: 'center' })
        }
      }
    }
  }, 500)
}

const onVideoError = (event) => {
  console.error('本地视频加载错误:', event)
  const video = event.target
  console.error('视频错误代码:', video.error ? video.error.code : '未知')
  console.error('视频错误消息:', video.error ? video.error.message : '未知')
  console.error('当前视频源:', video.src)
  
  // 不显示错误通知，避免干扰用户体验
  videoPlaying.value = false
  hasPlayed.value = true // 标记已尝试播放，显示占位符
}

const shareVideo = () => {
  if (navigator.share) {
    navigator.share({
      title: '欢雨鲜花 - 品牌故事',
      text: '了解欢雨鲜花的品牌理念和服务承诺',
      url: window.location.href
    }).catch(() => {
      // 用户取消分享
    })
  } else {
    // 复制链接到剪贴板
    navigator.clipboard.writeText(window.location.href)
    alert('链接已复制到剪贴板')
  }
}

// 视频加载完成后的处理
const onVideoLoaded = () => {
  if (videoPlayer.value) {
    const video = videoPlayer.value
    
    // 获取视频尺寸
    videoDimensions.value = {
      width: video.videoWidth,
      height: video.videoHeight
    }
    
    // 计算视频比例
    const ratio = video.videoWidth / video.videoHeight
    if (Math.abs(ratio - 16/9) < 0.1) {
      videoAspectRatio.value = '16:9'
    } else if (Math.abs(ratio - 4/3) < 0.1) {
      videoAspectRatio.value = '4:3'
    } else {
      videoAspectRatio.value = `${Math.round(ratio * 100) / 100}:1`
    }
    
    // 获取视频时长
    const duration = video.duration
    const minutes = Math.floor(duration / 60)
    const seconds = Math.floor(duration % 60)
    videoDuration.value = `${minutes}:${seconds.toString().padStart(2, '0')}`
    
    // 调整视频容器样式
    adjustVideoContainer(ratio)
  }
}

// 调整视频容器样式以适应不同比例
const adjustVideoContainer = (ratio) => {
  const container = document.querySelector('.video-container')
  const player = videoPlayer.value
  
  if (container && player) {
    // 根据比例调整容器样式
    if (Math.abs(ratio - 16/9) < 0.1) {
      // 16:9 比例，使用标准的宽屏布局
      videoAspectRatio.value = '16:9'
      container.style.paddingBottom = '56.25%' // 9/16 * 100%
      player.style.objectFit = 'cover'
    } else if (Math.abs(ratio - 4/3) < 0.1) {
      // 4:3 比例，调整容器高度
      videoAspectRatio.value = '4:3'
      container.style.paddingBottom = '75%' // 3/4 * 100%
      player.style.objectFit = 'cover'
    } else {
      // 其他比例，使用默认处理
      videoAspectRatio.value = `${Math.round(ratio * 100) / 100}:1`
      container.style.paddingBottom = `${(1/ratio) * 100}%`
      player.style.objectFit = 'cover'
    }
  }
}

// 根据视频比例计算padding-bottom
const getPaddingBottom = (aspectRatio) => {
  switch (aspectRatio) {
    case '16:9':
      return '56.25%' // 9/16 * 100%
    case '4:3':
      return '75%' // 3/4 * 100%
    default:
      // 解析自定义比例，如 "1.33:1"
      if (aspectRatio && aspectRatio.includes(':')) {
        const [width, height] = aspectRatio.split(':').map(Number)
        if (width && height) {
          return `${(height / width) * 100}%`
        }
      }
      return '56.25%' // 默认16:9
  }
}

// 商品相关方法
const getProductQuantity = (product) => {
  return productQuantities.value[product.id] || 1
}

const increaseQuantity = (product) => {
  const currentQuantity = getProductQuantity(product)
  if (currentQuantity < 99) {
    productQuantities.value[product.id] = currentQuantity + 1
  }
}

const decreaseQuantity = (product) => {
  const currentQuantity = getProductQuantity(product)
  if (currentQuantity > 1) {
    productQuantities.value[product.id] = currentQuantity - 1
  }
}

const handleAddToCart = async (product) => {
  const token = localStorage.getItem('token')
  if (!token) {
    notifyInfo('请先登录再加入购物车')
    router.push('/auth')
    return
  }
  if (!showQuantitySelector.value[product.id]) {
    // 首次点击，显示数量选择器
    showQuantitySelector.value[product.id] = true
  } else {
    // 确认添加到购物车
    const quantity = getProductQuantity(product)
    const result = await cartStore.addToCart({
      ...product,
      quantity
    })
    
    if (result.success) {
      // 重置状态
      showQuantitySelector.value[product.id] = false
      productQuantities.value[product.id] = 1
      
      // 显示成功提示
      showNotification('成功添加到购物车！', 'success')
    } else {
      showNotification(result.message, 'error')
    }
  }
}

const isFavorite = (product) => {
  return favoriteProducts.value.includes(product.id)
}

const toggleFavorite = async (product) => {
  const index = favoriteProducts.value.indexOf(product.id)
  try {
    if (index > -1) {
      await favoriteService.remove(product.id)
      favoriteProducts.value.splice(index, 1)
      const idx = recommendedProducts.value.findIndex(p => p.id === product.id)
      if (idx !== -1) { recommendedProducts.value[idx].popularity = Math.max(0, (recommendedProducts.value[idx].popularity || 0) - 1) }
      showNotification('已取消收藏', 'info')
    } else {
      await favoriteService.add(product.id)
      favoriteProducts.value.push(product.id)
      const idx = recommendedProducts.value.findIndex(p => p.id === product.id)
      if (idx !== -1) { recommendedProducts.value[idx].popularity = (recommendedProducts.value[idx].popularity || 0) + 1 }
      showNotification('已添加到收藏', 'success')
    }
  } catch (e) {
    showNotification('收藏操作失败', 'error')
  }
}

const quickView = (product) => {
  quickViewProduct.value = product
  showQuickViewModal.value = true
}

const closeQuickView = () => {
  showQuickViewModal.value = false
  quickViewProduct.value = null
}

 

const showNotification = (message, type = 'info') => {
  // 创建通知元素
  const notification = document.createElement('div')
  notification.className = `fixed top-4 right-4 z-50 px-6 py-3 rounded-lg shadow-lg transform transition-all duration-300 translate-x-full`
  
  // 根据类型设置样式
  switch (type) {
    case 'success':
      notification.className += ' bg-green-500 text-white'
      break
    case 'error':
      notification.className += ' bg-red-500 text-white'
      break
    case 'warning':
      notification.className += ' bg-yellow-500 text-white'
      break
    default:
      notification.className += ' bg-blue-500 text-white'
  }
  
  notification.textContent = message
  document.body.appendChild(notification)
  
  // 显示动画
  setTimeout(() => {
    notification.classList.remove('translate-x-full')
  }, 100)
  
  // 3秒后自动消失
  setTimeout(() => {
    notification.classList.add('translate-x-full')
    setTimeout(() => {
      document.body.removeChild(notification)
    }, 300)
  }, 3000)
}

// 触摸滑动支持
const touchStartX = ref(0)
const touchEndX = ref(0)
const isDragging = ref(false)

const handleTouchStart = (event) => {
  touchStartX.value = event.touches[0].clientX
  isDragging.value = true
}

const handleTouchMove = (event) => {
  if (!isDragging.value) return
  touchEndX.value = event.touches[0].clientX
}

const handleTouchEnd = () => {
  if (!isDragging.value) return
  isDragging.value = false
  
  const swipeDistance = touchEndX.value - touchStartX.value
  const minSwipeDistance = 50
  
  if (Math.abs(swipeDistance) > minSwipeDistance) {
    // 可以在这里添加滑动切换功能
    console.log('Swipe detected:', swipeDistance > 0 ? 'right' : 'left')
  }
}

// 移动端商品卡片触摸优化
const handleCardTouch = (product, event) => {
  // 延迟显示操作按钮，避免误触
  setTimeout(() => {
    if (!isDragging.value) {
      // 可以在这里添加触摸长按功能
    }
  }, 200)
}

// 防止iOS双击缩放
let lastTouchEnd = 0
const preventDoubleTapZoom = (event) => {
  const now = Date.now()
  if (now - lastTouchEnd <= 300) {
    event.preventDefault()
  }
  lastTouchEnd = now
}

// 移动端滚动优化
const handleScroll = () => {
  // 添加滚动时的性能优化
  requestAnimationFrame(() => {
    // 滚动相关逻辑
  })
}

// 移动端视频播放优化
const handleMobileVideoPlay = () => {
  const video = videoRef.value
  if (video && video.paused) {
    // 移动端视频播放需要用户交互
    video.play().catch(error => {
      console.error('Mobile video play failed:', error)
      // 可以显示播放按钮让用户手动点击
    })
  }
}

// 处理商品图片加载错误
const handleProductImageError = (event, product) => {
  console.warn(`商品${product.name}的图片加载失败，使用默认图片`)
  // 更新product对象中的image属性，使用默认图片
  if (featuredProducts.value.length > 0) {
    const index = featuredProducts.value.findIndex(p => p.id === product.id)
    if (index !== -1) {
      featuredProducts.value[index].image = '/images/product-placeholder.svg'
    }
  }
}

// 处理快速查看模态框中的图片加载错误
const handleQuickViewImageError = (event) => {
  console.warn('快速查看模态框中的图片加载失败，使用默认图片')
  // 更新quickViewProduct对象中的image属性，使用默认图片
  if (quickViewProduct.value) {
    quickViewProduct.value.image = '/images/product-placeholder.svg'
  }
}
</script>

<style scoped>
/* 首页样式增强 */
.fade-in {
  animation: fadeIn 1s ease-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* 文本截断样式 */
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.line-clamp-3 {
  display: -webkit-box;
  -webkit-line-clamp: 3;
  line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

/* 模态框动画 */
.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

/* 商品卡片悬停效果增强 */
.card {
  transition: all 0.3s ease;
}

.card:hover {
  transform: translateY(-4px);
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

/* 按钮动画效果 */
.btn-animated {
  position: relative;
  overflow: hidden;
}

.btn-animated::before {
  content: '';
  position: absolute;
  top: 50%;
  left: 50%;
  width: 0;
  height: 0;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.3);
  transform: translate(-50%, -50%);
  transition: width 0.6s, height 0.6s;
}

.btn-animated:active::before {
  width: 300px;
  height: 300px;
}

/* 图片加载动画 */
.image-loading {
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
}

@keyframes loading {
  0% {
    background-position: 200% 0;
  }
  100% {
    background-position: -200% 0;
  }
}

/* 收藏按钮动画 */
.favorite-animation {
  animation: favoriteHeart 0.6s ease-in-out;
}

@keyframes favoriteHeart {
  0% {
    transform: scale(1);
  }
  15% {
    transform: scale(1.3);
  }
  30% {
    transform: scale(1);
  }
  45% {
    transform: scale(1.2);
  }
  80% {
    transform: scale(1);
  }
}

/* 视频播放器样式优化 */
.video-container {
  background: #000;
  border-radius: 12px;
  overflow: hidden;
  position: relative;
  min-height: 300px;
  width: 100%;
}

.video-container .hero-image {
  position: absolute !important;
  top: 0 !important;
  left: 0 !important;
  width: 100% !important;
  height: 100% !important;
  object-fit: cover !important;
  object-position: center !important;
  border-radius: inherit !important;
}

.video-player {
  display: block !important;
  width: 100% !important;
  height: 100% !important;
  border-radius: 12px;
  object-fit: cover;
  position: absolute;
  top: 0;
  left: 0;
  z-index: 1;
  background: #000;
}

/* 确保视频容器正确定位 */
.video-container .relative {
  position: relative;
  overflow: hidden;
}

/* 视频比例指示器样式 */
.video-ratio-indicator {
  backdrop-filter: blur(10px);
  background: rgba(0, 0, 0, 0.7);
  border: 1px solid rgba(255, 255, 255, 0.1);
  z-index: 10;
  position: relative;
}

/* 图片清晰度优化 */
.hero-image {
  image-rendering: -webkit-optimize-contrast;
  image-rendering: auto;
  transform: translateZ(0);
  backface-visibility: hidden;
  perspective: 1000px;
}

/* 确保图片在高DPI屏幕上清晰显示 */
@media (-webkit-min-device-pixel-ratio: 2), (min-resolution: 192dpi) {
  .hero-image {
    image-rendering: -webkit-optimize-contrast;
    image-rendering: auto;
  }
}

/* 播放按钮优化 */
.play-button {
  backdrop-filter: blur(10px);
  background: rgba(255, 255, 255, 0.95);
  border: 1px solid rgba(255, 255, 255, 0.2);
  transition: all 0.3s ease;
}
.play-button:hover {
  background: rgba(255, 255, 255, 1);
  transform: scale(1.1);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
}

/* 视频控制按钮组优化 */
.video-controls {
  display: flex;
  gap: 8px;
  align-items: center;
}
.video-controls button {
  transition: all 0.2s ease;
}
.video-controls button:hover {
  transform: translateY(-1px);
}

/* 响应式视频容器 */
@media (max-width: 768px) {
  .video-container {
    min-height: 200px;
  }
  
  .video-controls {
    flex-wrap: wrap;
    justify-content: center;
  }
  
  .video-controls button {
    font-size: 14px;
    padding: 8px 12px;
  }
}

/* 移动端优化 */
@media (max-width: 640px) {
  /* 英雄区域移动端优化 */
  .hero-section {
    min-height: 70vh;
    padding: 2rem 0;
  }
  
  .hero-content h1 {
    font-size: 2rem;
    line-height: 1.2;
  }
  
  .hero-content p {
    font-size: 1rem;
    margin-bottom: 1.5rem;
  }
  
  .hero-buttons {
    flex-direction: column;
    gap: 1rem;
    width: 100%;
  }
  
  .hero-buttons .btn {
    width: 100%;
    padding: 1rem;
    font-size: 1rem;
  }
  
  /* 商品卡片移动端优化 */
  .card {
    margin-bottom: 1rem;
  }
  
  .card .relative {
    border-radius: 0.75rem;
  }
  
  .card img {
    height: 200px;
  }
  
  /* 悬停操作按钮移动端优化 */
  .card .absolute.inset-0 {
    opacity: 1;
    background: linear-gradient(to bottom, transparent 0%, rgba(0,0,0,0.3) 100%);
  }
  
  .card .absolute.inset-0 .flex {
    bottom: 1rem;
    top: auto;
    justify-content: center;
  }
  
  .card .absolute.inset-0 button {
    width: 3rem;
    height: 3rem;
    margin: 0 0.25rem;
  }
  
  /* 快速查看模态框移动端优化 */
  .fixed.inset-0 .bg-white {
    margin: 1rem;
    max-height: calc(100vh - 2rem);
    border-radius: 1rem;
  }
  
  .fixed.inset-0 .grid {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  
  /* 特色服务区域移动端优化 */
  .grid.grid-cols-3 {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }
  
  .grid.grid-cols-3 .text-center .w-20 {
    width: 4rem;
    height: 4rem;
  }
  
  .grid.grid-cols-3 .text-center .w-20 svg {
    width: 2rem;
    height: 2rem;
  }
  
  /* 品牌故事区域移动端优化 */
  .grid.grid-cols-2 {
    grid-template-columns: 1fr;
    gap: 2rem;
  }
  
  .grid.grid-cols-3 {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  
  /* 客户评价区域移动端优化 */
  .grid.grid-cols-1.md\\:grid-cols-3 {
    grid-template-columns: 1fr;
    gap: 1rem;
  }
  
  /* 按钮触摸优化 */
  button, .btn {
    min-height: 44px;
    min-width: 44px;
    touch-action: manipulation;
  }
  
  /* 输入框触摸优化 */
  input, textarea, select {
    min-height: 44px;
    font-size: 16px; /* 防止iOS缩放 */
  }
}

/* 触摸设备优化 */
@media (hover: none) and (pointer: coarse) {
  /* 移除悬停效果，优化触摸体验 */
  .card:hover {
    transform: none;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
  }
  
  .group:hover .group-hover\\:bg-huanyu-pink-100 {
    background-color: rgb(254 242 242);
  }
  
  .group:hover .group-hover\\:text-huanyu-pink-600 {
    color: rgb(219 39 119);
  }
  
  .group:hover .group-hover\\:scale-110 {
    transform: none;
  }
  
  .group:hover .group-hover\\:opacity-100 {
    opacity: 1;
  }
  
  /* 背景切换过渡效果 */
  .fade-enter-active,
  .fade-leave-active {
    transition: opacity 0.5s ease;
  }
  
  .fade-enter-from,
  .fade-leave-to {
    opacity: 0;
  }
  
  /* 确保英雄区域内容始终可见 */
  .hero-content {
    z-index: 10;
    transition: opacity 0.5s ease;
  }
  
  /* 背景控制面板样式 */
  .background-control-panel {
    position: absolute;
    bottom: 20px;
    right: 20px;
    z-index: 20;
    background: rgba(0, 0, 0, 0.7);
    border-radius: 8px;
    padding: 12px;
    backdrop-filter: blur(10px);
    display: flex;
    flex-direction: column;
    gap: 8px;
  }
  
  .control-button {
    padding: 8px 12px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.2s ease;
    font-size: 14px;
  }
  
  .control-button.active {
    background: #ec4899;
    color: white;
  }
  
  .control-button:not(.active) {
    background: rgba(255, 255, 255, 0.8);
    color: #333;
  }
  
  .control-button:hover {
    transform: translateY(-1px);
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  }
  
  /* 触摸反馈 */
  button:active, .btn:active {
    transform: scale(0.95);
    transition: transform 0.1s ease;
  }
  
  /* 增大触摸目标 */
  .card .absolute button {
    width: 3.5rem;
    height: 3.5rem;
  }
  
  /* 移动端视频播放按钮优化 */
  .play-button {
    width: 4rem;
    height: 4rem;
  }
  
  .play-button svg {
    width: 1.5rem;
    height: 1.5rem;
  }
}

/* 小屏幕设备优化 */
@media (max-width: 480px) {
  .container {
    padding-left: 1rem;
    padding-right: 1rem;
  }
  
  .hero-content h1 {
    font-size: 1.75rem;
  }
  
  .hero-buttons .btn {
    padding: 0.875rem;
    font-size: 0.875rem;
  }
  
  .card img {
    height: 180px;
  }
  
  .card .p-4 {
    padding: 1rem;
  }
  
  .card h3 {
    font-size: 1rem;
  }
  
  .card .text-xl {
    font-size: 1.125rem;
  }
  
  .card .text-sm {
    font-size: 0.75rem;
  }
}

/* 横屏模式优化 */
@media (max-height: 500px) and (orientation: landscape) {
  .hero-section {
    min-height: 90vh;
  }
  
  .hero-content {
    padding-top: 1rem;
  }
  
  .hero-content h1 {
    font-size: 1.5rem;
    margin-bottom: 0.5rem;
  }
  
  .hero-content p {
    font-size: 0.875rem;
    margin-bottom: 1rem;
  }
  
  .hero-buttons {
    flex-direction: row;
    gap: 0.5rem;
  }
  
  .hero-buttons .btn {
    padding: 0.75rem 1.5rem;
    font-size: 0.875rem;
  }
}
</style>
