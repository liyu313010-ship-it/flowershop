/**
 * 头像处理工具函数
 * 统一处理头像URL拼接逻辑，确保在整个应用中头像显示一致
 */

/**
 * 获取完整的头像URL
 * @param {string} avatarPath - 头像路径（可能是相对路径或完整URL）
 * @returns {string} 完整的头像URL
 */
export const getAvatarUrl = (avatarPath) => {
  // 1. 如果没有头像路径、为空字符串或无效值，直接返回默认头像
  if (!avatarPath || typeof avatarPath !== 'string' || avatarPath.trim() === '') {
    return getDefaultAvatarUrl()
  }

  // 2. 对于blob:类型的URL（本地预览URL），直接返回，不做任何修改
  if (avatarPath.startsWith('blob:')) {
    return avatarPath
  }
  
  // 3. 如果已经是完整URL（包含http），直接返回，不添加时间戳
  if (avatarPath.startsWith('http://') || avatarPath.startsWith('https://')) {
    return avatarPath
  }

  // 4. 对于用户名字符串（不是路径），返回默认头像
  if (!avatarPath.includes('/') && !avatarPath.includes('.')) {
    return getDefaultAvatarUrl()
  }

  // 5. 如果是默认头像路径，直接返回
  if (avatarPath.includes('/images/avatar-')) {
    // 确保路径以/开头
    let normalizedPath = avatarPath.trim()
    if (!normalizedPath.startsWith('/')) {
      normalizedPath = '/' + normalizedPath
    }
    return normalizedPath
  }

  // 6. 处理uploads/avatars路径，确保路径格式正确
  if (avatarPath.includes('uploads/avatars/')) {
    // 确保路径以/开头
    let normalizedPath = avatarPath.trim()
    if (!normalizedPath.startsWith('/')) {
      normalizedPath = '/' + normalizedPath
    }
    return normalizedPath
  }

  // 7. 处理只有文件名的情况，假设是头像文件，添加uploads/avatars路径前缀
  if (avatarPath.includes('.') && !avatarPath.includes('/')) {
    // 构建完整的头像URL
    const fullPath = `/uploads/avatars/${avatarPath}`
    return fullPath
  }

  // 8. 其他情况，确保路径格式正确
  let normalizedPath = avatarPath.trim()
  
  // 确保有正确的前导斜杠
  if (!normalizedPath.startsWith('/')) {
    normalizedPath = '/' + normalizedPath
  }

  return normalizedPath
}

/**
 * 获取默认头像URL
 * @returns {string} 默认头像URL
 */
export const getDefaultAvatarUrl = () => {
  // 切换到另一个存在的默认头像文件，解决avatar-1.svg加载失败问题
  return '/images/avatar-2.svg'
}

/**
 * 获取完整的产品图片URL
 * @param {string} imagePath - 产品图片路径
 * @returns {string} 完整的产品图片URL
 */
export const getProductImageUrl = (imagePath) => {
  // 如果没有图片路径或无效，返回默认产品图片
  if (!imagePath || typeof imagePath !== 'string' || imagePath.trim() === '') {
    return '/images/product-placeholder.svg'
  }

  // 完整 URL（例如对象存储/CDN）直接使用
  if (imagePath.startsWith('http://') || imagePath.startsWith('https://')) {
    return imagePath
  }

  let normalizedPath = imagePath.trim()

  // 兼容旧管理端曾错误保存/拼接出的 /api/images 与 /api/uploads 地址。
  // 图片属于静态资源，不应经过仅处理控制器请求的 /api 代理。
  normalizedPath = normalizedPath.replace(/^\/api(?=\/(?:images|uploads)\/)/i, '')

  // 只有文件名时按上传商品图处理
  if (normalizedPath.includes('.') && !normalizedPath.includes('/')) {
    normalizedPath = `/uploads/products/${normalizedPath}`
  } else if (!normalizedPath.startsWith('/')) {
    normalizedPath = '/' + normalizedPath
  }

  return normalizedPath
}

/**
 * 检查头像是否为默认头像
 * @param {string} avatarPath - 头像路径
 * @returns {boolean} 是否为默认头像
 */
export const isDefaultAvatar = (avatarPath) => {
  // 包含新的默认头像路径
  return !avatarPath || avatarPath === '/images/avatar-1.svg' || avatarPath === '/images/avatar-2.svg'
}

/**
 * 验证头像URL是否有效
 * @param {string} avatarUrl - 头像URL
 * @returns {Promise<boolean>} 头像URL是否有效
 */
export const validateAvatarUrl = async (avatarUrl) => {
  
  if (!avatarUrl || typeof avatarUrl !== 'string' || avatarUrl.trim() === '') {
    return false
  }
  
  // 对于本地预览URL，默认认为有效
  if (avatarUrl.startsWith('blob:')) {
    return true
  }
  
  // 对于完整URL，尝试通过HEAD请求验证
  if (avatarUrl.startsWith('http://') || avatarUrl.startsWith('https://')) {
    try {
      const response = await fetch(avatarUrl, { method: 'HEAD' })
      return response.ok
    } catch (error) {
      return false
    }
  }
  
  // 与getAvatarUrl保持一致，uploads/avatars路径直接从public目录访问
  let urlToCheck = avatarUrl.trim()
  
  // 移除可能存在的/api前缀，确保直接从public目录访问
  if (urlToCheck.startsWith('/api/')) {
    urlToCheck = urlToCheck.replace('/api', '')
  }
  
  // 对于uploads/avatars路径，确保格式正确
  if (urlToCheck.includes('uploads/avatars/')) {
    // 确保路径以/开头
    if (!urlToCheck.startsWith('/')) {
      urlToCheck = '/' + urlToCheck
    }
  }
  
  // 处理images目录的路径
  else if (urlToCheck.includes('/images/')) {
    // 确保路径以/开头
    if (!urlToCheck.startsWith('/')) {
      urlToCheck = '/' + urlToCheck
    }
  }
  
  // 确保URL以/开头
  else if (!urlToCheck.startsWith('/')) {
    urlToCheck = '/' + urlToCheck
  }
  
  // 对于头像文件名，添加uploads/avatars路径前缀
  else if (urlToCheck.includes('.') && !urlToCheck.includes('/')) {
    urlToCheck = `/uploads/avatars/${urlToCheck}`
  }
  
  // 在浏览器中，我们只能通过fetch尝试加载来验证
  try {
    const response = await fetch(urlToCheck, { method: 'HEAD' })
    return response.ok
  } catch (error) {
    return false
  }
}

/**
 * 处理头像加载错误
 * @param {Event} event - 元素加载错误事件
 * @param {string} defaultAvatar - 默认头像URL
 */
export const handleAvatarError = (event, defaultAvatar = null) => {
  try {
    const element = event.target
    const fallbackAvatar = defaultAvatar || getDefaultAvatarUrl()
    
    // 检测元素类型，分别处理img和div背景图片
    if (element.tagName.toLowerCase() === 'img') {
      // 对于img元素，设置src属性
      const failedUrl = new URL(element.src, window.location.origin).href
      const fallbackUrl = new URL(fallbackAvatar, window.location.origin).href
      
      if (failedUrl !== fallbackUrl) {
        element.src = fallbackAvatar
      }
    } else {
      // 对于div元素，设置背景图片
      element.style.backgroundImage = `url(${fallbackAvatar})`
    }
    
    // 头像加载失败，已切换到默认头像
  } catch (error) {
    // 作为最后手段，尝试设置默认头像
    try {
      if (event.target) {
        if (event.target.tagName.toLowerCase() === 'img') {
          event.target.src = getDefaultAvatarUrl()
        } else {
          event.target.style.backgroundImage = `url(${getDefaultAvatarUrl()})`
        }
      }
    } catch (fallbackError) {
      // 忽略错误，不影响用户体验
    }
  }
}
