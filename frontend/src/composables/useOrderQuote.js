import { ref, unref, watch, onBeforeUnmount } from 'vue'
import orderService from '@/services/orderService'

// Both cart and checkout display the same server-calculated quote.
export function useOrderQuote(cartItems, shippingMethod = ref('standard'), couponCode = ref('')) {
  const quote = ref(null)
  const quoteError = ref('')
  const isQuoting = ref(false)
  let sequence = 0

  const refreshQuote = async () => {
    const request = ++sequence
    quote.value = null
    quoteError.value = ''
    if (!unref(cartItems)?.length) { isQuoting.value = false; return null }
    isQuoting.value = true
    try {
      const result = await orderService.quoteOrder({
        shippingMethod: unref(shippingMethod),
        couponCode: unref(couponCode)?.trim() || null
      })
      if (request !== sequence) return null
      quote.value = result?.data || result
      return quote.value
    } catch (error) {
      if (request === sequence) quoteError.value = error.response?.data?.message || error.response?.data || error.message || '报价失败，请重试'
      return null
    } finally {
      if (request === sequence) isQuoting.value = false
    }
  }

  watch(() => JSON.stringify([
    (unref(cartItems) || []).map(item => [item.id, item.productId, item.price, item.quantity, item.stock, item.isActive]),
    unref(shippingMethod), unref(couponCode)
  ]), refreshQuote, { immediate: true, flush: 'sync' })
  onBeforeUnmount(() => { sequence += 1 })

  return { quote, quoteError, isQuoting, refreshQuote }
}
