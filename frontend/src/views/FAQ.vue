<template>
  <div class="min-h-screen bg-gray-50 py-8">
    <div class="container mx-auto px-4">
      <h1 class="text-3xl font-bold text-gray-800 mb-8 text-center">常见问题</h1>
      <p class="text-gray-600 text-center mb-10 max-w-3xl mx-auto">
        这里是我们收集的一些顾客常见问题，如果您有其他疑问，欢迎随时联系客服
      </p>
      
      <div class="mb-12">
        <h2 class="text-2xl font-bold text-pink-600 mb-6 flex items-center">
          <i class="fas fa-shopping-bag mr-3"></i>产品相关
        </h2>
        <div class="space-y-4">
          <div v-for="(item, index) in productFAQs" :key="index" class="bg-white rounded-lg shadow">
            <button 
              :class="['w-full text-left p-5 flex justify-between items-center', { 'border-b border-gray-200': index < productFAQs.length - 1 }]"
              @click="toggleAnswer('product', index)"
            >
              <span class="font-medium text-lg text-gray-800">{{ item.question }}</span>
              <i :class="['fas text-pink-500 transition-transform duration-300', { 'transform rotate-180': activeAnswers.product === index }]">
                <!-- 这里使用加号/减号图标 -->
                <span class="text-xl">下拉</span>
              </i>
            </button>
            <div 
              :class="['px-5 pb-5 overflow-hidden transition-all duration-300 ease-in-out', 
                      { 'max-h-0': activeAnswers.product !== index, 
                        'max-h-96': activeAnswers.product === index }]"
            >
              <div class="pt-2 text-gray-700">
                {{ item.answer }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="mb-12">
        <h2 class="text-2xl font-bold text-pink-600 mb-6 flex items-center">
          <i class="fas fa-truck mr-3"></i>配送相关
        </h2>
        <div class="space-y-4">
          <div v-for="(item, index) in deliveryFAQs" :key="index" class="bg-white rounded-lg shadow">
            <button 
              :class="['w-full text-left p-5 flex justify-between items-center', { 'border-b border-gray-200': index < deliveryFAQs.length - 1 }]"
              @click="toggleAnswer('delivery', index)"
            >
              <span class="font-medium text-lg text-gray-800">{{ item.question }}</span>
              <i :class="['fas text-pink-500 transition-transform duration-300', { 'transform rotate-180': activeAnswers.delivery === index }]">
                <span class="text-xl">下拉</span>
              </i>
            </button>
            <div 
              :class="['px-5 pb-5 overflow-hidden transition-all duration-300 ease-in-out', 
                      { 'max-h-0': activeAnswers.delivery !== index, 
                        'max-h-96': activeAnswers.delivery === index }]"
            >
              <div class="pt-2 text-gray-700">
                {{ item.answer }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="mb-12">
        <h2 class="text-2xl font-bold text-pink-600 mb-6 flex items-center">
          <i class="fas fa-credit-card mr-3"></i>支付相关
        </h2>
        <div class="space-y-4">
          <div v-for="(item, index) in paymentFAQs" :key="index" class="bg-white rounded-lg shadow">
            <button 
              :class="['w-full text-left p-5 flex justify-between items-center', { 'border-b border-gray-200': index < paymentFAQs.length - 1 }]"
              @click="toggleAnswer('payment', index)"
            >
              <span class="font-medium text-lg text-gray-800">{{ item.question }}</span>
              <i :class="['fas text-pink-500 transition-transform duration-300', { 'transform rotate-180': activeAnswers.payment === index }]">
                <span class="text-xl">下拉</span>
              </i>
            </button>
            <div 
              :class="['px-5 pb-5 overflow-hidden transition-all duration-300 ease-in-out', 
                      { 'max-h-0': activeAnswers.payment !== index, 
                        'max-h-96': activeAnswers.payment === index }]"
            >
              <div class="pt-2 text-gray-700">
                {{ item.answer }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="mb-12">
        <h2 class="text-2xl font-bold text-pink-600 mb-6 flex items-center">
          <i class="fas fa-undo mr-3"></i>退换货相关
        </h2>
        <div class="space-y-4">
          <div v-for="(item, index) in returnFAQs" :key="index" class="bg-white rounded-lg shadow">
            <button 
              :class="['w-full text-left p-5 flex justify-between items-center', { 'border-b border-gray-200': index < returnFAQs.length - 1 }]"
              @click="toggleAnswer('return', index)"
            >
              <span class="font-medium text-lg text-gray-800">{{ item.question }}</span>
              <i :class="['fas text-pink-500 transition-transform duration-300', { 'transform rotate-180': activeAnswers.return === index }]">
                <span class="text-xl">下拉</span>
              </i>
            </button>
            <div 
              :class="['px-5 pb-5 overflow-hidden transition-all duration-300 ease-in-out', 
                      { 'max-h-0': activeAnswers.return !== index, 
                        'max-h-96': activeAnswers.return === index }]"
            >
              <div class="pt-2 text-gray-700">
                {{ item.answer }}
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-lg shadow-lg p-8 text-center">
        <h2 class="text-2xl font-bold text-pink-600 mb-4">还有其他问题？</h2>
        <p class="text-gray-700 mb-6">
          如果您的问题没有在上面找到答案，请联系我们的客服团队获取帮助
        </p>
        <div class="flex flex-wrap justify-center gap-4">
          <a href="/contact" class="inline-flex items-center px-6 py-3 bg-pink-500 text-white font-medium rounded-lg hover:bg-pink-600 transition-colors duration-300">
            <i class="fas fa-envelope mr-2"></i>联系客服
          </a>
          <a href="tel:15025033613" class="inline-flex items-center px-6 py-3 bg-gray-100 text-gray-700 font-medium rounded-lg hover:bg-gray-200 transition-colors duration-300">
            <i class="fas fa-phone-alt mr-2"></i>电话咨询
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'FAQ',
  data() {
    return {
      activeAnswers: {
        product: null,
        delivery: null,
        payment: null,
        return: null
      },
      productFAQs: [
        {
          question: '如何确保收到的鲜花新鲜度？',
          answer: '我们承诺所有鲜花均为当日新鲜花材，采用专业的冷链配送方式，在配送过程中使用特制的保鲜材料和包装，确保鲜花在送达您手中时保持最佳状态。收到鲜花后，请按照随附的养护说明进行处理，可以延长花期。'
        },
        {
          question: '花束的实际效果与图片有差异怎么办？',
          answer: '由于鲜花的季节性和自然生长特性，实际花束可能会与网站展示图片略有差异。我们会尽量保证花材种类、颜色和整体风格的一致性，但在某些情况下可能会用同等价位的替代花材。如对花束效果有特殊要求，请在订单备注中说明。'
        },
        {
          question: '鲜花的保鲜期是多久？',
          answer: '不同种类的鲜花保鲜期不同，一般在5-15天不等。玫瑰、百合等常见花材通常可以保持7-10天的观赏期。我们会在配送时提供详细的鲜花养护指南，正确的养护方法可以显著延长花期。'
        },
        {
          question: '可以定制特殊花束吗？',
          answer: '当然可以！我们提供个性化定制服务，您可以联系客服说明您的需求，包括花材选择、颜色搭配、花束大小等，我们的花艺师将为您量身设计。定制花束需要提前24小时预约，请尽早联系我们。'
        }
      ],
      deliveryFAQs: [
        {
          question: '配送范围包括哪些地区？',
          answer: '我们提供全国范围内的鲜花配送服务，覆盖所有省市自治区的主要城市。对于偏远地区，配送时间可能会略有延长。您可以在下单时输入收货地址，系统会自动显示是否支持配送。'
        },
        {
          question: '下单后多久可以送达？',
          answer: '同城配送：当天12:00前下单，当天送达；12:00后下单，次日送达。异地配送：根据距离远近，一般需要1-3个工作日送达。您可以在下单时选择具体的配送日期和时间段。'
        },
        {
          question: '可以指定具体的送达时间吗？',
          answer: '可以。我们提供定时配送服务，您可以在下单时选择上午(10:00-12:00)、下午(14:00-17:00)或晚上(18:00-20:00)等时间段。如需更精确的时间，请在订单备注中说明，我们会尽量为您安排。定时配送需要收取15元的服务费。'
        },
        {
          question: '节假日配送有什么特殊安排？',
          answer: '节假日期间(如情人节、母亲节等)订单量较大，建议您提前3-5天下单，以确保能够按时送达。节假日期间可能会调整配送时间和费用，具体信息会在节日前在网站公告。'
        }
      ],
      paymentFAQs: [
        {
          question: '支持哪些支付方式？',
          answer: '我们支持多种支付方式，包括微信支付、支付宝、银联在线支付等。您可以在结算页面选择您偏好的支付方式。所有支付过程均经过加密处理，确保您的支付安全。'
        },
        {
          question: '可以货到付款吗？',
          answer: '目前我们仅支持在线支付方式，暂不提供货到付款服务。这是为了确保订单的准确性和及时性，同时也能更好地保障消费者权益。'
        },
        {
          question: '如何申请发票？',
          answer: '您可以在下单时选择需要发票，并填写发票抬头、税号等信息。我们会在订单完成后为您开具电子发票并发送至您的邮箱。如需纸质发票，请在下单时备注。'
        },
        {
          question: '优惠券如何使用？',
          answer: '在结算页面，您可以输入优惠券码并点击应用。优惠券会自动抵扣相应金额。请注意，每张优惠券都有使用期限和适用范围，请在使用前仔细阅读优惠券的使用说明。'
        }
      ],
      returnFAQs: [
        {
          question: '什么情况下可以申请退换货？',
          answer: '您可以在以下情况下申请退换货：收到的鲜花与订单描述严重不符；鲜花在配送过程中受损严重(超过30%的花朵枯萎)；由于我们的原因导致花材错误或数量短缺；订单配送错误。请在收到鲜花后2小时内联系客服并提供相关照片。'
        },
        {
          question: '如何申请退换货？',
          answer: '请在收到鲜花后2小时内，通过客服电话(15025033613)、在线客服或邮箱(2784107771@qq.com)联系我们，提供订单号、问题描述和照片凭证。我们的客服会在2小时内审核您的申请，并根据具体情况为您安排退货退款或重新配送。'
        },
        {
          question: '退款会退到哪里？多久可以到账？',
          answer: '确认符合退货条件后，我们将在24小时内处理退款申请。退款将原路返回您的支付账户，到账时间取决于您的支付方式，一般为1-7个工作日。如使用优惠券支付的订单，退还的金额将扣除优惠券部分。'
        },
        {
          question: '以下情况不支持退换货：',
          answer: '• 因正常养护不当导致的鲜花枯萎\n• 超过2小时未提出异议的情况\n• 因个人审美差异对花束造型不满意\n• 节假日、特殊活动期间不支持无理由退换\n• 季节性花材替换不属于质量问题'
        }
      ]
    }
  },
  methods: {
    toggleAnswer(category, index) {
      this.activeAnswers[category] = this.activeAnswers[category] === index ? null : index;
    }
  }
}
</script>

<style scoped>
.container {
  max-width: 1200px;
}

/* 添加响应式样式 */
@media (max-width: 768px) {
  h1 {
    font-size: 2xl;
  }
  h2 {
    font-size: 1.5rem;
  }
  .text-lg {
    font-size: 1rem;
  }
}

/* 手风琴动画效果 */
.max-h-96 {
  transition: max-height 0.3s ease-in-out;
}

.max-h-0 {
  transition: max-height 0.3s ease-out;
}

/* 按钮悬停效果 */
button:hover {
  background-color: #f9fafb;
  transition: background-color 0.3s ease;
}

/* 图标样式优化 */
.fas {
  font-size: 1.2rem;
}

/* 平滑滚动效果 */
html {
  scroll-behavior: smooth;
}

/* 卡片悬停效果 */
.bg-white.rounded-lg {
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.bg-white.rounded-lg:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

/* 按钮样式 */
a {
  transition: all 0.3s ease;
}

a:hover {
  transform: translateY(-1px);
}
</style>
