<template>
  <div class="contact-page min-h-screen">
    <!-- 页面标题 -->
    <div class="page-heading">
      <div class="container mx-auto px-4 py-6">
        <h1 class="text-2xl font-bold text-gray-800">联系我们</h1>
        <p class="text-gray-600 mt-2">我们期待您的消息</p>
      </div>
    </div>

    <div class="container mx-auto px-4 py-8">
      <div class="grid lg:grid-cols-2 gap-8">
        
        <!-- 联系表单 -->
        <div class="contact-card rounded-lg p-8">
          <h2 class="text-2xl font-bold text-huanyu-pink-600 mb-6">发送消息</h2>
          
          <form @submit.prevent="handleSubmit" class="space-y-6">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">姓名 *</label>
              <input 
                v-model="form.name"
                type="text" 
                required
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
                placeholder="请输入您的姓名"
              >
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">电话 *</label>
              <input 
                v-model="form.phone"
                type="tel" 
                required
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
                placeholder="请输入您的电话号码"
              >
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">邮箱</label>
              <input 
                v-model="form.email"
                type="email" 
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
                placeholder="请输入您的邮箱地址"
              >
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">咨询类型 *</label>
              <select 
                v-model="form.type"
                required
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
              >
                <option value="">请选择咨询类型</option>
                <option value="order">订单咨询</option>
                <option value="product">产品咨询</option>
                <option value="delivery">配送咨询</option>
                <option value="complaint">投诉建议</option>
                <option value="cooperation">商务合作</option>
                <option value="other">其他</option>
              </select>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-2">留言内容 *</label>
              <textarea 
                v-model="form.message"
                required
                rows="5"
                class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-huanyu-pink-500 focus:border-transparent"
                placeholder="请详细描述您的问题或需求..."
              ></textarea>
            </div>
            
            <div class="flex items-center">
              <input 
                v-model="form.agree"
                type="checkbox" 
                id="agree"
                class="rounded text-huanyu-pink-600"
              >
              <label for="agree" class="ml-2 text-sm text-gray-600">
                我同意欢雨flower处理我的个人信息
              </label>
            </div>
            
            <button 
              type="submit"
              :disabled="!form.agree || submitting"
              class="btn-primary w-full disabled:bg-gray-300 disabled:cursor-not-allowed"
            >
              {{ submitting ? '发送中...' : '发送消息' }}
            </button>
          </form>
        </div>
        
        <!-- 联系信息 -->
        <div class="space-y-6">
          
          <!-- 快速联系 -->
          <div class="contact-card rounded-lg p-8">
            <h2 class="text-2xl font-bold text-huanyu-pink-600 mb-6">快速联系</h2>
            <div class="space-y-4">
              <a 
                href="tel:15025033613"
                class="contact-card flex items-center p-4 rounded-lg hover:-translate-y-1 transition-transform"
              >
                <div class="w-12 h-12 bg-huanyu-pink-500 rounded-full flex items-center justify-center mr-4">
                  <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"></path>
                  </svg>
                </div>
                <div>
                  <p class="font-semibold">客服热线</p>
                  <p class="text-huanyu-pink-600 font-bold">15025033613</p>
                  <p class="text-sm text-gray-600">7×24小时服务</p>
                </div>
              </a>
              
              <a 
                href="mailto:service@huanyuflower.com"
                class="flex items-center p-4 bg-huanyu-pink-50 rounded-lg hover:bg-huanyu-pink-100 transition-colors"
              >
                <div class="w-12 h-12 bg-huanyu-pink-500 rounded-full flex items-center justify-center mr-4">
                  <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 4.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path>
                  </svg>
                </div>
                <div>
                  <p class="font-semibold">邮箱联系</p>
                  <p class="text-huanyu-pink-600 font-bold">2784107771@qq.com</p>
                  <p class="text-sm text-gray-600">24小时内回复</p>
                </div>
              </a>
            </div>
          </div>
          
          <!-- 店铺地址 -->
          <div class="contact-card rounded-lg p-8">
            <h2 class="text-2xl font-bold text-huanyu-pink-600 mb-6">店铺地址</h2>
            <div class="space-y-4">
              <div class="flex items-start">
                <svg class="w-6 h-6 text-huanyu-pink-500 mr-3 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"></path>
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"></path>
                </svg>
                <div>
                  <p class="font-semibold">总店地址</p>
                  <p class="text-gray-600">西南林业大学</p>
                  <p class="text-sm text-gray-500 mt-1">昆明五号线白龙寺D口出站，沿着南院路继续向前走500米</p>
                </div>
              </div>
              
              <div class="flex items-start">
                <svg class="w-6 h-6 text-huanyu-pink-500 mr-3 mt-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
                </svg>
                <div>
                  <p class="font-semibold">营业时间</p>
                  <p class="text-gray-600">周一至周五：8:00 - 20:00</p>
                  <p class="text-gray-600">周六至周日：9:00 - 21:00</p>
                  <p class="text-sm text-gray-500 mt-1">节假日正常营业</p>
                </div>
              </div>
            </div>
            

          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const submitting = ref(false)

const form = ref({
  name: '',
  phone: '',
  email: '',
  type: '',
  message: '',
  agree: false
})

const handleSubmit = async () => {
  if (!form.value.agree) {
    alert('请同意隐私政策')
    return
  }
  
  submitting.value = true
  
  try {
    const subject = `[欢雨鲜花] ${form.value.type || '客户咨询'} - ${form.value.name}`
    const body = [
      `姓名：${form.value.name}`,
      `电话：${form.value.phone}`,
      `邮箱：${form.value.email || '未填写'}`,
      `咨询类型：${form.value.type || '未选择'}`,
      '',
      form.value.message
    ].join('\n')
    window.location.href = `mailto:2784107771@qq.com?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(body)}`
    alert('已打开邮件客户端，请点击发送完成提交。')
    
    // 重置表单
    form.value = {
      name: '',
      phone: '',
      email: '',
      type: '',
      message: '',
      agree: false
    }
  } catch (error) {
    alert('发送失败，请稍后重试')
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  window.scrollTo(0, 0)
})
</script>
