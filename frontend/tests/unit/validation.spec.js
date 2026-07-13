import {
  validateEmail,
  validatePassword,
  validatePhone,
  validateRegisterForm
} from '@/utils/validation'

describe('validation rules', () => {
  it('accepts valid email and phone', () => {
    expect(validateEmail('customer@example.com').isValid).toBe(true)
    expect(validatePhone('13800138000').isValid).toBe(true)
  })

  it('requires production password length and complexity', () => {
    expect(validatePassword('abc123').isValid).toBe(false)
    expect(validatePassword('FlowerShop2026!').isValid).toBe(true)
  })

  it('validates the registration flow', () => {
    expect(validateRegisterForm({
      username: 'flower_user',
      email: 'customer@example.com',
      password: 'FlowerShop2026!',
      confirmPassword: 'FlowerShop2026!',
      phone: '13800138000'
    }).isValid).toBe(true)
  })
})
