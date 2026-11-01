import { ref, nextTick } from 'vue'
import orderService from '@/services/orderService'
import { useOrderQuote } from '@/composables/useOrderQuote'

jest.mock('@/services/orderService', () => ({ __esModule: true, default: { quoteOrder: jest.fn() } }))
jest.mock('vue', () => ({ ...jest.requireActual('vue'), onBeforeUnmount: jest.fn() }))

const settle = async () => { await Promise.resolve(); await Promise.resolve(); await nextTick() }

describe('server order quotes', () => {
  beforeEach(() => jest.clearAllMocks())

  it('uses the server total without the old cart-only free shipping or automatic discount', async () => {
    orderService.quoteOrder.mockResolvedValue({ success: true, data: { subtotal: 200, shippingFee: 15, discountAmount: 0, totalAmount: 215 } })
    const result = useOrderQuote(ref([{ id: 1, price: 200, quantity: 1 }]))
    expect(result.isQuoting.value).toBe(true)
    await settle()
    expect(result.quote.value.totalAmount).toBe(215)
    expect(result.isQuoting.value).toBe(false)
    expect(orderService.quoteOrder).toHaveBeenCalledWith({ shippingMethod: 'standard', couponCode: null })
  })

  it('does not let a slower old response replace the quote for the changed cart', async () => {
    let resolveOld, resolveNew
    orderService.quoteOrder.mockImplementationOnce(() => new Promise(resolve => { resolveOld = resolve }))
      .mockImplementationOnce(() => new Promise(resolve => { resolveNew = resolve }))
    const items = ref([{ id: 1, price: 100, quantity: 1 }])
    const result = useOrderQuote(items)
    items.value[0].quantity = 2
    resolveNew({ data: { totalAmount: 215 } })
    await settle()
    resolveOld({ data: { totalAmount: 115 } })
    await settle()
    expect(result.quote.value.totalAmount).toBe(215)
  })

  it('invalidates the previous quote when a selected coupon becomes invalid', async () => {
    orderService.quoteOrder.mockResolvedValueOnce({ data: { totalAmount: 115 } })
      .mockRejectedValueOnce({ response: { data: { message: '该优惠券已经使用' } } })
    const coupon = ref('')
    const result = useOrderQuote(ref([{ id: 1, price: 100, quantity: 1 }]), ref('standard'), coupon)
    await settle()
    coupon.value = 'USED'
    expect(result.quote.value).toBeNull()
    await settle()
    expect(result.quote.value).toBeNull()
    expect(result.quoteError.value).toBe('该优惠券已经使用')
    expect(result.isQuoting.value).toBe(false)
  })
})
