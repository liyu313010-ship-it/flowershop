/**
 * 表单验证工具函数
 */

/**
 * 验证用户名
 * @param {string} username - 用户名
 * @returns {Object} 验证结果 { isValid: boolean, message: string }
 */
export const validateUsername = (username) => {
  if (!username || username.trim() === '') {
    return { isValid: false, message: '用户名不能为空' }
  }
  
  if (username.length > 20) {
    return { isValid: false, message: '用户名长度不能超过20位' }
  }
  
  // 只允许字母、数字、下划线
  const usernameRegex = /^[a-zA-Z0-9_]+$/
  if (!usernameRegex.test(username)) {
    return { isValid: false, message: '用户名只能包含字母、数字和下划线' }
  }
  
  return { isValid: true, message: '' }
}

/**
 * 验证登录用户名（无限制，只要不为空即可）
 * @param {string} username - 用户名
 * @returns {Object} 验证结果 { isValid: boolean, message: string }
 */
export const validateLoginUsername = (username) => {
  if (!username || username.trim() === '') {
    return { isValid: false, message: '用户名不能为空' }
  }
  
  return { isValid: true, message: '' }
}

/**
 * 验证邮箱
 * @param {string} email - 邮箱地址
 * @returns {Object} 验证结果
 */
export const validateEmail = (email) => {
  if (!email || email.trim() === '') {
    return { isValid: false, message: '邮箱不能为空' }
  }
  
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  if (!emailRegex.test(email)) {
    return { isValid: false, message: '请输入有效的邮箱地址' }
  }
  
  return { isValid: true, message: '' }
}

/**
 * 验证登录密码（无限制，只要不为空即可）
 * @param {string} password - 密码
 * @returns {Object} 验证结果
 */
export const validateLoginPassword = (password) => {
  if (!password || password.trim() === '') {
    return { isValid: false, message: '密码不能为空' }
  }
  
  return { isValid: true, message: '' }
}

/**
 * 验证密码
 * @param {string} password - 密码
 * @returns {Object} 验证结果
 */
export const validatePassword = (password) => {
  if (!password || password.trim() === '') {
    return { isValid: false, message: '密码不能为空' }
  }
  
  if (password.length < 12) {
    return { isValid: false, message: '密码长度至少12位' }
  }
  
  if (password.length > 50) {
    return { isValid: false, message: '密码长度不能超过50位' }
  }
  
  // 检查密码强度（至少包含字母和数字）
  const hasLetter = /[a-zA-Z]/.test(password)
  const hasNumber = /\d/.test(password)
  
  if (!hasLetter || !hasNumber) {
    return { isValid: false, message: '密码必须包含字母和数字' }
  }
  
  return { isValid: true, message: '' }
}

/**
 * 验证确认密码
 * @param {string} password - 密码
 * @param {string} confirmPassword - 确认密码
 * @returns {Object} 验证结果
 */
export const validateConfirmPassword = (password, confirmPassword) => {
  if (!confirmPassword || confirmPassword.trim() === '') {
    return { isValid: false, message: '请确认密码' }
  }
  
  if (password !== confirmPassword) {
    return { isValid: false, message: '两次输入的密码不一致' }
  }
  
  return { isValid: true, message: '' }
}

/**
 * 验证手机号
 * @param {string} phone - 手机号
 * @returns {Object} 验证结果
 */
export const validatePhone = (phone) => {
  if (!phone || phone.trim() === '') {
    return { isValid: true, message: '' } // 手机号是可选的
  }
  
  const phoneRegex = /^1[3-9]\d{9}$/
  if (!phoneRegex.test(phone)) {
    return { isValid: false, message: '请输入有效的手机号码' }
  }
  
  return { isValid: true, message: '' }
}

/**
 * 验证登录表单
 * @param {Object} formData - 表单数据
 * @returns {Object} 验证结果
 */
export const validateLoginForm = (formData) => {
  const { username, password } = formData
  
  const usernameResult = validateLoginUsername(username)
  if (!usernameResult.isValid) {
    return usernameResult
  }
  
  const passwordResult = validateLoginPassword(password)
  if (!passwordResult.isValid) {
    return passwordResult
  }
  
  return { isValid: true, message: '验证通过' }
}

/**
 * 验证注册表单
 * @param {Object} formData - 表单数据
 * @returns {Object} 验证结果
 */
export const validateRegisterForm = (formData) => {
  const { username, email, password, confirmPassword, phone } = formData
  
  // 验证用户名
  const usernameResult = validateUsername(username)
  if (!usernameResult.isValid) {
    return usernameResult
  }
  
  // 验证邮箱
  const emailResult = validateEmail(email)
  if (!emailResult.isValid) {
    return emailResult
  }
  
  // 验证密码
  const passwordResult = validatePassword(password)
  if (!passwordResult.isValid) {
    return passwordResult
  }
  
  // 验证确认密码
  const confirmPasswordResult = validateConfirmPassword(password, confirmPassword)
  if (!confirmPasswordResult.isValid) {
    return confirmPasswordResult
  }
  
  // 验证手机号（可选）
  const phoneResult = validatePhone(phone)
  if (!phoneResult.isValid) {
    return phoneResult
  }
  
  return { isValid: true, message: '验证通过' }
}

/**
 * 获取密码强度等级
 * @param {string} password - 密码
 * @returns {Object} 强度信息 { level: number, text: string, color: string }
 */
export const getPasswordStrength = (password) => {
  if (!password) {
    return { level: 0, text: '无', color: 'gray' }
  }
  
  let score = 0
  
  // 长度评分
  if (password.length >= 8) score += 1
  if (password.length >= 12) score += 1
  
  // 复杂度评分
  if (/[a-z]/.test(password)) score += 1
  if (/[A-Z]/.test(password)) score += 1
  if (/\d/.test(password)) score += 1
  if (/[^a-zA-Z\d]/.test(password)) score += 1
  
  if (score <= 2) {
    return { level: 1, text: '弱', color: 'red' }
  } else if (score <= 4) {
    return { level: 2, text: '中', color: 'yellow' }
  } else {
    return { level: 3, text: '强', color: 'green' }
  }
}
