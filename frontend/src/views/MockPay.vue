<template>
  <div class="mock-payment-page">
    <div class="payment-container">
      <div class="header">
        <h1>模拟支付网关</h1>
        <p class="subtitle">Mock Payment Gateway</p>
      </div>
      
      <div class="order-info">
        <div class="info-row">
          <span class="label">订单编号：</span>
          <span class="value">{{ orderId }}</span>
        </div>
        <div class="info-row">
          <span class="label">支付金额：</span>
          <span class="value price">¥{{ amount }}</span>
        </div>
        <div class="info-row">
          <span class="label">收款方：</span>
          <span class="value">欢雨鲜花 (Huanyu Flower Shop)</span>
        </div>
      </div>

      <div class="payment-methods">
        <h3>选择支付方式</h3>
        <div class="method-list">
          <div 
            class="method-item" 
            :class="{ active: selectedMethod === 'alipay' }"
            @click="selectedMethod = 'alipay'"
          >
            <div class="method-icon alipay">支</div>
            <span>支付宝 (模拟)</span>
          </div>
          <div 
            class="method-item" 
            :class="{ active: selectedMethod === 'wechat' }"
            @click="selectedMethod = 'wechat'"
          >
            <div class="method-icon wechat">微</div>
            <span>微信支付 (模拟)</span>
          </div>
        </div>
      </div>

      <div class="actions">
        <button class="btn btn-primary" @click="confirmPayment" :disabled="processing">
          {{ processing ? '支付处理中...' : '确认支付' }}
        </button>
        <button class="btn btn-secondary" @click="cancelPayment" :disabled="processing">
          取消支付
        </button>
      </div>
      
      <div class="debug-info">
        <p>这是开发环境下的模拟支付页面。</p>
        <p>点击"确认支付"将直接标记订单为已支付。</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import orderService from '@/services/orderService'
import { notifySuccess, notifyError } from '@/utils/notify'

const route = useRoute()
const useRouterInstance = useRouter()

const orderId = ref('')
const amount = ref('0.00')
const returnUrl = ref('')
const selectedMethod = ref('alipay')
const processing = ref(false)
const paymentReference = ref('')
const dbOrderId = ref(null)

onMounted(() => {
  // 从 URL 参数获取订单信息
  // 参数格式: order_id=ORDxxx&amount=99.00&reference=PAYxxx&return_url=...
  orderId.value = route.query.order_id || '未知订单'
  amount.value = route.query.amount || '0.00'
  paymentReference.value = route.query.reference || ''
  returnUrl.value = route.query.return_url || '/'
  
  if (!route.query.order_id) {
    notifyError('参数错误：缺少订单信息')
  }
  
  try {
    const u = new URL(returnUrl.value, window.location.origin)
    const oid = u.searchParams.get('orderId')
    if (oid) dbOrderId.value = Number(oid)
  } catch {}
})

const confirmPayment = async () => {
  processing.value = true
  
  // 模拟网络延迟
  await new Promise(resolve => setTimeout(resolve, 1500))
  
  try {
    // 若拿到了数据库订单ID，直接更新支付状态形成闭环
    if (dbOrderId.value) {
      await orderService.updatePaymentStatus(dbOrderId.value, {
        paymentStatus: 'paid',
        paymentMethod: selectedMethod.value,
        paymentReference: paymentReference.value || `PAY${Date.now()}`
      })
      notifySuccess('支付成功，订单状态已更新')
      useRouterInstance.push(`/orders/${dbOrderId.value}`)
      return
    }

    // 回退：无法解析订单ID时，跳转回回调页由其自动验证
    let targetUrl;
    try {
      targetUrl = new URL(returnUrl.value)
    } catch (e) {
      // 如果是相对路径，使用当前 origin 作为 base
      targetUrl = new URL(returnUrl.value, window.location.origin)
    }

    targetUrl.searchParams.set('out_trade_no', paymentReference.value)
    targetUrl.searchParams.set('trade_status', 'TRADE_SUCCESS')
    targetUrl.searchParams.set('verify', 'true')
    
    // 模拟成功，跳转回去
    window.location.href = targetUrl.toString()
    
  } catch (error) {
    console.error(error)
    notifyError('支付处理失败')
    processing.value = false
  }
}

const cancelPayment = () => {
  if (confirm('确定要放弃支付吗？')) {
    // 跳转回上一页或者首页
    window.history.back()
  }
}
</script>

<style scoped>
.mock-payment-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background-color: #f5f5f5;
  padding: 20px;
}

.payment-container {
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
  width: 100%;
  max-width: 480px;
  padding: 2rem;
}

.header {
  text-align: center;
  margin-bottom: 2rem;
  border-bottom: 1px solid #eee;
  padding-bottom: 1rem;
}

.header h1 {
  margin: 0;
  color: #333;
  font-size: 1.5rem;
}

.subtitle {
  color: #999;
  margin: 0.5rem 0 0;
  font-size: 0.9rem;
}

.order-info {
  background: #f9f9f9;
  border-radius: 8px;
  padding: 1.5rem;
  margin-bottom: 2rem;
}

.info-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 1rem;
  font-size: 1rem;
}

.info-row:last-child {
  margin-bottom: 0;
}

.label {
  color: #666;
}

.value {
  color: #333;
  font-weight: 500;
}

.value.price {
  color: #ff69b4;
  font-size: 1.2rem;
  font-weight: bold;
}

.payment-methods h3 {
  font-size: 1rem;
  margin-bottom: 1rem;
  color: #333;
}

.method-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-bottom: 2rem;
}

.method-item {
  display: flex;
  align-items: center;
  padding: 1rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s;
}

.method-item:hover {
  border-color: #ff69b4;
}

.method-item.active {
  border-color: #ff69b4;
  background-color: #fff0f6;
  box-shadow: 0 0 0 1px #ff69b4 inset;
}

.method-icon {
  width: 32px;
  height: 32px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: bold;
  margin-right: 12px;
}

.method-icon.alipay {
  background-color: #1677ff;
}

.method-icon.wechat {
  background-color: #07c160;
}

.actions {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.btn {
  width: 100%;
  padding: 12px;
  border: none;
  border-radius: 6px;
  font-size: 1rem;
  cursor: pointer;
  transition: background 0.3s;
}

.btn-primary {
  background-color: #ff69b4;
  color: white;
}

.btn-primary:hover {
  background-color: #ff1493;
}

.btn-primary:disabled {
  background-color: #ffb6c1;
  cursor: not-allowed;
}

.btn-secondary {
  background-color: #f5f5f5;
  color: #666;
}

.btn-secondary:hover {
  background-color: #e0e0e0;
}

.debug-info {
  margin-top: 2rem;
  padding-top: 1rem;
  border-top: 1px dashed #eee;
  font-size: 0.8rem;
  color: #999;
  text-align: center;
}

.debug-info p {
  margin: 0.25rem 0;
}
</style>
