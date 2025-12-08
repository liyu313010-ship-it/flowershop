import { createRouter, createWebHistory } from 'vue-router'
import { defineAsyncComponent } from 'vue'
import { notifyError } from '@/utils/notify'
import { useUserStore } from '@/stores/user'

// 带重试的异步组件加载器，避免动态导入失败导致页面不可用
const withRetry = (loader, maxAttempts = 3, delay = 500) => defineAsyncComponent({
  loader,
  timeout: 15000,
  onError(error, retry, fail, attempts) {
    const isImportError = /Failed to fetch dynamically imported module|net::ERR_ABORTED/i.test(error?.message || '')
    if (isImportError && attempts < maxAttempts) {
      setTimeout(() => retry(), delay * attempts)
    } else {
      notifyError('页面加载失败，请重试')
      fail(error)
    }
  }
})

// 路由组件懒加载 - 提高首屏加载速度
const Home = () => import('@/views/Home.vue')
const Auth = () => import('@/views/Auth.vue') // 统一的认证页面
import AdminDashboard from '@/views/admin/Dashboard.vue'
import ProductManagement from '@/views/admin/ProductManagement.vue'
import OrderManagement from '@/views/admin/OrderManagement.vue'
import AdminOrderDetail from '@/views/admin/OrderDetail.vue'
const UserManagement = () => import('@/views/admin/UserManagement.vue')
const StatsReport = () => import('@/views/admin/StatsReport.vue')
const CouponManagement = () => import('@/views/admin/CouponManagement.vue')
const ReviewManagement = () => import('@/views/admin/ReviewManagement.vue')

const Cart = () => import('@/views/Cart.vue')
import Orders from '@/views/Orders.vue'
import OrderDetail from '@/views/OrderDetail.vue'

import Profile from '@/views/user/Profile.vue'
const Favorites = () => import('@/views/Favorites.vue')

// 全局错误处理将于路由实例创建后注册

const ProductDetail = () => import('@/views/ProductDetail.vue')
const Products = () => import('@/views/Products.vue')
// 直接导入 About 组件以避免懒加载问题
import About from '@/views/About.vue'
const Contact = () => import('@/views/Contact.vue')

import Checkout from '@/views/Checkout.vue'
const PaymentVerification = () => import('@/views/PaymentVerification.vue')

const Shipping = () => import('@/views/Shipping.vue') // 配送说明页面
const Returns = () => import('@/views/Returns.vue') // 退换货政策页面
const FAQ = () => import('@/views/FAQ.vue') // 常见问题页面
const Privacy = () => import('@/views/Privacy.vue') // 隐私政策页面

// 路由配置
const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home,
    meta: { title: '欢雨鲜花 - 首页' }
  },
  {
    path: '/products',
    name: 'Products',
    component: Products,
    meta: { title: '商品列表 - 欢雨鲜花' }
  },
  {
    path: '/about',
    name: 'About',
    component: About,
    meta: { title: '关于我们 - 欢雨鲜花' }
  },
  {
    path: '/contact',
    name: 'Contact',
    component: Contact,
    meta: { title: '联系我们 - 欢雨鲜花' }
  },
  {
    path: '/shipping',
    name: 'Shipping',
    component: Shipping,
    meta: { title: '配送说明 - 欢雨鲜花' }
  },
  {
    path: '/returns',
    name: 'Returns',
    component: Returns,
    meta: { title: '退换货政策 - 欢雨鲜花' }
  },
  {
    path: '/faq',
    name: 'FAQ',
    component: FAQ,
    meta: { title: '常见问题 - 欢雨鲜花' }
  },
  {
    path: '/privacy',
    name: 'Privacy',
    component: Privacy,
    meta: { title: '隐私政策 - 欢雨鲜花' }
  },
  {
    path: '/auth',
    name: 'Auth',
    component: Auth,
    meta: { title: '登录注册 - 欢雨鲜花' }
  },
  {
    path: '/admin/login',
    name: 'AdminLogin',
    component: Auth,
    meta: { title: '管理员登录 - 欢雨鲜花' }
  },
  {
    path: '/product/:id',
    name: 'ProductDetail',
    component: ProductDetail,
    meta: { title: '商品详情 - 欢雨鲜花' }
  },
  {
    path: '/reviews/:id',
    name: 'ReviewDetail',
    component: () => import('@/views/ReviewDetail.vue'),
    meta: { title: '评价详情 - 欢雨鲜花' }
  },
  {
    path: '/admin',
    name: 'AdminDashboard',
    component: AdminDashboard,
    meta: { 
      title: '管理员后台 - 欢雨鲜花',
      requiresAuth: true,
      requiresAdmin: true
    }
  },
  {
    path: '/admin/products',
    name: 'ProductManagement',
    component: ProductManagement,
    meta: { 
      title: '商品管理 - 欢雨鲜花',
      requiresAuth: true,
      requiresAdmin: true
    }
  },
  {
    path: '/admin/orders',
    name: 'OrderManagement',
    component: OrderManagement,
    meta: { 
      title: '订单管理 - 欢雨鲜花',
      requiresAuth: true,
      requiresAdmin: true
    }
  },
  {
    path: '/admin/orders/:id',
    name: 'AdminOrderDetail',
    component: AdminOrderDetail,
    meta: { 
      title: '订单详情 - 欢雨鲜花',
      requiresAuth: true,
      requiresAdmin: true
    }
  },
  {
    path: '/admin/users',
    name: 'UserManagement',
    component: UserManagement,
    meta: { 
      title: '用户管理 - 欢雨鲜花',
      requiresAuth: true,
      requiresAdmin: true
    }
  },
  {
    path: '/admin/reviews',
    name: 'ReviewManagement',
    component: ReviewManagement,
    meta: { 
      title: '评价管理 - 欢雨鲜花',
      requiresAuth: true,
      requiresAdmin: true
    }
  },
  {
    path: '/admin/coupons',
    name: 'CouponManagement',
    component: CouponManagement,
    meta: { 
      title: '优惠券管理 - 欢雨鲜花',
      requiresAuth: true,
      requiresAdmin: true
    }
  },
  {
    path: '/cart',
    name: 'Cart',
    component: Cart,
    meta: { 
      title: '购物车 - 欢雨鲜花',
      requiresAuth: true
    }
  },
  {
    path: '/orders',
    name: 'Orders',
    component: Orders,
    meta: { 
      title: '我的订单 - 欢雨鲜花',
      requiresAuth: true
    }
  },
  {
    path: '/orders/:id',
    name: 'OrderDetail',
    component: OrderDetail,
    meta: { 
      title: '订单详情 - 欢雨鲜花',
      requiresAuth: true
    }
  },
  {
    path: '/payment/verify/:orderId',
    name: 'PaymentVerification',
    component: PaymentVerification,
    meta: { title: '支付验证 - 欢雨鲜花' }
  },
  {
    path: '/order-detail/:id',
    name: 'OrderDetailAlt',
    component: OrderDetail,
    meta: { 
      title: '订单详情 - 欢雨鲜花',
      requiresAuth: true
    }
  },
  {
    path: '/favorites',
    name: 'Favorites',
    component: Favorites,
    meta: { title: '我的收藏 - 欢雨鲜花', requiresAuth: true }
  },
  {
    path: '/profile',
    name: 'Profile',
    component: Profile,
    meta: { title: '个人资料 - 欢雨鲜花', requiresAuth: true }
  },
  {
    path: '/checkout',
    name: 'Checkout',
    component: Checkout,
    meta: { title: '结算 - 欢雨鲜花', requiresAuth: true }
  },

  // 管理员统计报表页面
  {
    path: '/admin/stats-report',
    name: 'StatsReport',
    component: StatsReport,
    meta: {
      title: '数据统计报表 - 欢雨鲜花',
      requiresAuth: true,
      requiresAdmin: true
    }
  },
  
  // 404页面
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('@/views/NotFound.vue'),
    meta: { title: '页面未找到 - 欢雨鲜花' }
  }
]

// 创建路由实例
const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(to, from, savedPosition) {
    // 页面切换时滚动到顶部
    return { top: 0 }
  }
})

// 动态导入失败的统一提示与重试提示
router.onError((error) => {
  const msg = error?.message || ''
  if (/Failed to fetch dynamically imported module|net::ERR_ABORTED/i.test(msg)) {
    // 导航切换导致的中断属预期，不提示错误

    return
  }

})

// 全局前置守卫 - 路由权限控制
router.beforeEach(async (to, from, next) => {
  const userStore = useUserStore()
  
  // 确保用户状态已初始化
  if (!userStore.user && userStore.token) {
    userStore.initializeUser()
  }
  
  // 设置页面标题
  document.title = to.meta.title || '欢雨鲜花'
  
  // 检查是否需要登录权限
  if (to.meta.requiresAuth && !userStore.isLoggedIn) {

    // 未登录跳转到认证页面
    next({
      path: '/auth',
      query: { redirect: to.fullPath }
    })
    return
  }
  
  // 检查是否需要管理员权限
  if (to.meta.requiresAdmin && !userStore.isAdmin) {

    // 非管理员跳转到管理员登录页面
    next({ path: '/admin/login', query: { redirect: to.fullPath } })
    return
  }
  

  next()
})

export default router
