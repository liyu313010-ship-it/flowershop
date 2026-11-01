import api from './api'

const API_BASE_URL = '/api'

const userService = {
  // 获取用户信息
  async getUserInfo() {
    try {
      const token = localStorage.getItem('token')
      // 修复API路径，添加/api前缀以匹配后端路由
      const response = await fetch('/api/auth/me', {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      })
      
      let data = null
      try {
        // 先检查响应是否为空
        const responseText = await response.text()
        if (responseText.trim()) {
          data = JSON.parse(responseText)
        }
      } catch (jsonError) {
        return {
          success: false,
          data: { message: '服务器响应格式错误' }
        }
      }
      
      // 确保将后端返回的FullName字段映射到name字段
      if (data) {
        // 处理不同的字段名映射
        if (data.FullName && !data.name) {
          data.name = data.FullName
        }
        // 映射性别字段
        if (typeof data.gender === 'undefined' && typeof data.Gender !== 'undefined') {
          data.gender = data.Gender
        }
        // 确保id字段存在
        if (!data.id && data.Id) {
          data.id = data.Id
        }
        // 确保lastLoginAt字段存在
        if (!data.lastLoginAt && data.LastLoginAt) {
          data.lastLoginAt = data.LastLoginAt
        }
      }
      
      return {
        success: response.ok,
        data: data
      }
    } catch (error) {
        return { 
          success: false, 
          data: { message: error.message || '获取用户信息失败，请检查网络连接' } 
        }
    }
  },

  // 更新用户信息
  async updateUserInfo(userInfo) {
    try {
      const token = localStorage.getItem('token')
      // 调整字段名以匹配后端的UpdateUserDto
      const formattedUserInfo = {
        FullName: userInfo.name,
        Email: userInfo.email,
        Phone: userInfo.phone,
        Gender: userInfo.gender, // 确保gender字段被发送到后端
        Address: userInfo.address || ''
      }
      
      // 标准化avatar URL格式（如果提供了）
      if (userInfo.avatar) {
        let avatarUrl = userInfo.avatar
        // 确保URL以'/uploads/avatars/'开头
        if (!avatarUrl.startsWith('/uploads/avatars/')) {
          // 如果URL已经包含uploads/avatars，只保留这部分及之后的内容
          const avatarMatch = avatarUrl.match(/(uploads\/avatars\/.*)/)
          if (avatarMatch && avatarMatch[1]) {
            avatarUrl = `/${avatarMatch[1]}`
          } else {
            // 否则使用默认路径结构
            const fileName = avatarUrl.split('/').pop()
            avatarUrl = `/uploads/avatars/${fileName}`
          }
        }
        formattedUserInfo.Avatar = avatarUrl
      }
      const response = await fetch(`${API_BASE_URL}/auth/profile`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(formattedUserInfo)
      })
      
      let data = null
      try {
        // 先检查响应是否为空
        const responseText = await response.text()
        if (responseText.trim()) {
          data = JSON.parse(responseText)
        } else {
          // 根据响应状态判断是否成功
          if (response.ok) {
            // 服务器成功但返回空响应，返回用户提交的数据作为成功响应
            return {
              success: true,
              data: { ...userInfo }
            }
          }
        }
      } catch (jsonError) {
        return {
          success: false,
          data: { message: '服务器响应格式错误' }
        }
      }
      
      return {
        success: response.ok,
        data: data || { message: response.ok ? '操作成功' : '操作失败' }
      }
    } catch (error) {
      return {
        success: false,
        data: { message: error.message || '更新用户信息失败' }
      }
    }
  },

  // 修改密码
  async changePassword(passwordData) {
    try {
      const token = localStorage.getItem('token')
      const response = await fetch(`${API_BASE_URL}/auth/change-password`, {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(passwordData)
      })
      
      let data = null
      try {
        // 先检查响应是否为空
        const responseText = await response.text()
        if (responseText.trim()) {
          data = JSON.parse(responseText)
        } else {
          if (response.ok) {
            data = { message: '密码修改成功' }
          }
        }
      } catch (jsonError) {
        return {
          success: false,
          data: { message: '服务器响应格式错误' }
        }
      }
      
      return {
        success: response.ok,
        data: data || { message: response.ok ? '密码修改成功' : '密码修改失败' }
      }
    } catch (error) {
      return {
        success: false,
        data: { message: error.message || '修改密码失败' }
      }
    }
  },

  // 获取收货地址 - 增强版，支持多种数据格式和错误处理
  async getAddresses() {
    try {
      // 尝试通过API获取地址列表
      const res = await api.get('/Addresses', { silent: true })
      
      // 从不同格式的响应中提取地址数据
      const addressData = this.extractAddressDataFromResponse(res)
      
      // 数据验证和清洗
      let formattedAddresses = []
      if (addressData && Array.isArray(addressData) && addressData.length > 0) {
        // 确保返回标准格式的地址数据
        formattedAddresses = addressData.map(addr => this.formatAddressObject(addr))
      }
      
      // 如果获取到有效地址，返回结果
      if (formattedAddresses.length > 0) {
        return {
          success: true,
          data: formattedAddresses
        }
      }
      
      // 返回空列表，不再回退到用户表 Address
      return {
        success: true,
        data: [],
        message: '暂无收货地址'
      }
    } catch (error) {
      // 返回空列表而不是回退，避免删除后仍显示
      return {
        success: true,
        data: [],
        message: '无法获取收货地址信息'
      }
    }
  },
  
  // 从不同格式的响应中提取地址数据
  extractAddressDataFromResponse(response) {
    // 尝试多种可能的数据路径
    const possiblePaths = [
      response?.data,
      response?.data?.data,
      response?.Data,
      response
    ]
    
    for (const path of possiblePaths) {
      if (Array.isArray(path)) {
        return path
      }
    }
    
    return null
  },
  
  // 格式化地址对象，统一字段名称和数据结构
  formatAddressObject(addr) {
    const province = addr.province || addr.Province || ''
    const city = addr.city || addr.City || ''
    const district = addr.district || addr.District || ''
    const detailAddress = addr.detailAddress || addr.DetailAddress || ''
    
    // 确保fullAddress字段存在且完整
    const fullAddress = addr.fullAddress || 
                       addr.FullAddress || 
                       `${province}${city}${district}${detailAddress}`
    
    return {
      id: addr.id || addr.Id || null,
      name: addr.name || addr.recipientName || addr.RecipientName || addr.Name || '',
      recipientName: addr.recipientName || addr.RecipientName || addr.name || addr.Name || '',
      phone: addr.phone || addr.phoneNumber || addr.PhoneNumber || addr.Phone || '',
      phoneNumber: addr.phoneNumber || addr.PhoneNumber || addr.phone || addr.Phone || '',
      province: province,
      city: city,
      district: district,
      detailAddress: detailAddress,
      fullAddress: fullAddress,
      isDefault: !! (addr.isDefault || addr.IsDefault || false),
      postalCode: addr.postalCode || addr.PostalCode || '',
      // 添加时间戳，用于前端缓存控制
      lastUpdated: new Date().toISOString()
    }
  },
  
  // 备用获取地址方法
  async getAddressFallback() {
    try {

      
      // 方法1：尝试通过api.get获取用户信息
      try {
        const me = await api.get('/auth/me')
        
        if (me && (me.data || me.Data)) {
          const userData = me.data || me.Data
          const addressText = userData.address || userData.Address || ''
          
          if (addressText && typeof addressText === 'string') {
            // 尝试解析地址字符串
            const parsed = this.parseAddressText(addressText)
            
            const fallbackAddress = this.formatAddressObject({
              id: 'fallback_' + Date.now(), // 避免缓存问题
              name: userData.name || userData.Name || userData.username || userData.Username || '用户',
              phone: userData.phone || userData.Phone || userData.phoneNumber || userData.PhoneNumber || '',
              province: parsed.province,
              city: parsed.city,
              district: parsed.district,
              detailAddress: parsed.detail || addressText,
              fullAddress: addressText,
              isDefault: true
            })
            

            return {
              success: true,
              data: [fallbackAddress],
              message: '使用备用地址方案'
            }
          }
        }
      } catch (error) {
        // 忽略错误，尝试备用方案2
      }
      
      // 方法2：尝试通过fetch获取用户信息
      try {
        const token = localStorage.getItem('token')
        const response = await fetch('/api/auth/me', {
          method: 'GET',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        })
        
        if (response.ok) {
          const data = await response.json()
          
          const addressText = data.address || data.Address || ''
          if (addressText && typeof addressText === 'string') {
            const parsed = this.parseAddressText(addressText)
            
            const fallbackAddress = this.formatAddressObject({
              id: 'fallback_fetch_' + Date.now(),
              name: data.name || data.Name || '用户',
              phone: data.phone || data.Phone || '',
              province: parsed.province,
              city: parsed.city,
              district: parsed.district,
              detailAddress: parsed.detail || addressText,
              fullAddress: addressText,
              isDefault: true
            })
            
            return {
              success: true,
              data: [fallbackAddress],
              message: '使用备用地址方案2'
            }
          }
        }
      } catch (error) {
        // 忽略错误
      }
      
      // 如果用户信息中没有地址，返回空列表
      return {
        success: true,
        data: []
      }
    } catch (error) {
      throw error
    }
  },
  
  // 简单的地址文本解析函数
  parseAddressText(addressText) {
    // 这是一个简化版的地址解析函数
    // 尝试使用|分隔符解析
    if (addressText.includes('|')) {
      const parts = addressText.split('|').map(s => s.trim())
      return {
        province: parts.length > 3 ? parts[3] : '',
        city: parts.length > 4 ? parts[4] : '',
        district: parts.length > 5 ? parts[5] : '',
        detail: parts[0] || addressText
      }
    }
    
    // 基本返回解析结果
    return {
      province: '',
      city: '',
      district: '',
      detail: addressText
    }
  },
  
  // 地址数据验证函数
  validateAddressData(addressData) {
    if (!addressData) return false
    
    // 验证必要字段
    const name = addressData.name || addressData.recipientName || addressData.RecipientName || addressData.Name
    const phone = addressData.phone || addressData.phoneNumber || addressData.PhoneNumber || addressData.Phone
    const detailAddress = addressData.detailAddress || addressData.DetailAddress
    
    // 姓名、电话和详细地址是必要的
    if (!name || name.trim().length === 0) {
      return false
    }
    
    // 简单的手机号验证
    if (!phone || !/^1[3-9]\d{9}$/.test(phone)) {
      return false
    }
    
    if (!detailAddress || detailAddress.trim().length === 0) {
      return false
    }
    
    return true
  },

  // 添加收货地址 - 增强版，支持数据验证和格式统一
  async addAddress(addressData) {
    try {
      // 验证必要字段
      if (!this.validateAddressData(addressData)) {
        throw new Error('地址信息不完整，请填写必要的收货信息')
      }
      
      // 格式化地址数据，确保与后端API兼容
      const formattedData = {
        RecipientName: addressData.name || addressData.recipientName || addressData.RecipientName,
        PhoneNumber: addressData.phone || addressData.phoneNumber || addressData.PhoneNumber,
        Province: addressData.province || addressData.Province,
        City: addressData.city || addressData.City,
        District: addressData.district || addressData.District,
        DetailAddress: addressData.detailAddress || addressData.DetailAddress,
        IsDefault: addressData.isDefault || addressData.IsDefault || false
      }
      let res
      try {
        res = await api.post('/Addresses', formattedData, { silent: true })
      } catch (primaryError) {
        const status = primaryError?.response?.status
        if (status === 404 || status === 500) {
          const token = localStorage.getItem('token')
          const response = await fetch('/api/Auth/addresses', {
            method: 'POST',
            headers: {
              'Authorization': `Bearer ${token}`,
              'Content-Type': 'application/json'
            },
            body: JSON.stringify(formattedData)
          })
          if (!response.ok) {
            const text = await response.text()
            let errMsg = '地址添加失败'
            try { const obj = JSON.parse(text); errMsg = obj.error || obj.message || errMsg } catch {}
            throw new Error(errMsg)
          }
          res = await response.json()
        } else {
          throw primaryError
        }
      }
      
      // 返回标准格式的结果
      return {
        success: true,
        data: res.data || res.Data || res,
        message: '地址添加成功'
      }
    } catch (error) {
      const errorMessage = error.response?.data?.error || 
                           error.response?.data?.Error || 
                           error.response?.data?.message || 
                           error.response?.data?.Message || 
                           error.message || 
                           '地址添加失败，请重试'
      return { success: false, data: { message: errorMessage } }
    }
  },

  // 更新收货地址 - 增强版，支持数据验证和错误处理
  async updateAddress(addressId, addressData) {
    try {
      // 验证必要字段
      if (!addressId) {
        throw new Error('地址ID不能为空')
      }
      
      if (!this.validateAddressData(addressData)) {
        throw new Error('地址信息不完整，请填写必要的收货信息')
      }
      
      // 格式化地址数据，确保与后端API兼容
      const formattedData = {
        RecipientName: addressData.name || addressData.recipientName || addressData.RecipientName,
        PhoneNumber: addressData.phone || addressData.phoneNumber || addressData.PhoneNumber,
        Province: addressData.province || addressData.Province,
        City: addressData.city || addressData.City,
        District: addressData.district || addressData.District,
        DetailAddress: addressData.detailAddress || addressData.DetailAddress,
        IsDefault: addressData.isDefault || addressData.IsDefault || false
      }
      
      const res = await api.put(`/Addresses/${addressId}`, formattedData, { silent: true })
      
      // 返回标准格式的结果
      return {
        success: true,
        data: res.data || res.Data || res,
        message: '地址更新成功'
      }
    } catch (error) {
      const errorMessage = error.response?.data?.error || 
                           error.response?.data?.Error || 
                           error.response?.data?.message || 
                           error.response?.data?.Message || 
                           error.message || 
                           '地址更新失败，请重试'
      return { success: false, data: { message: errorMessage } }
    }
  },

  // 删除收货地址 - 增强版，支持错误处理
  async deleteAddress(addressId) {
    try {
      // 解析地址ID，支持字符串/数字/对象
      const idCandidate = typeof addressId === 'object' ? (addressId.id || addressId.Id) : addressId
      const idNum = Number(idCandidate)
      if (!Number.isFinite(idNum) || idNum <= 0) {
        throw new Error('地址ID不能为空')
      }
      
      // 特殊处理备用地址ID
      if (typeof idCandidate === 'string' && idCandidate.startsWith('fallback')) {
        return {
          success: true,
          message: '备用地址已处理'
        }
      }
      
      const res = await api.delete(`/Addresses/${idNum}`, { silent: true })
      
      // 返回标准格式的结果
      return {
        success: true,
        data: res.data || res.Data || { message: '地址删除成功' },
        message: '地址删除成功'
      }
    } catch (error) {
      // 提供更详细的错误信息
      const errorMessage = error.response?.data?.message || 
                         error.response?.data?.Message || 
                         error.message || 
                         '地址删除失败，请重试'
      
      return { success: false, data: { message: errorMessage } }
    }
  },

  async setDefaultAddress(addressId) {
    try {
      const token = localStorage.getItem('token')
      const response = await fetch(`/api/Addresses/${addressId}/default`, {
        method: 'PATCH',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      })
      
      let data = null
      try {
        const responseText = await response.text()
        if (responseText.trim()) {
          data = JSON.parse(responseText)
        }
      } catch (jsonError) {
        return {
          success: false,
          data: { message: '服务器响应格式错误' }
        }
      }
      
      return {
        success: response.ok,
        data: data || { message: response.ok ? '设置默认地址成功' : '设置默认地址失败' }
      }
    } catch (error) {
      return {
        success: false,
        data: { message: error.message || '设置默认地址失败' }
      }
    }
  },

  // 获取订单统计
  async getOrderStats() {
    try {
      const token = localStorage.getItem('token')
      const response = await fetch(`${API_BASE_URL}/auth/order-stats`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      })
      
      let data = null
      try {
        // 先检查响应是否为空
        const responseText = await response.text()
        if (responseText.trim()) {
          data = JSON.parse(responseText)
        } else {
          // 返回默认的订单统计数据
          data = {
            totalOrders: 0,
            totalAmount: 0,
            pendingOrders: 0,
            completedOrders: 0
          }
        }
      } catch (jsonError) {
        // 即使JSON解析失败，也返回默认的订单统计数据
        return {
          success: false,
          data: {
            message: '服务器响应格式错误',
            totalOrders: 0,
            totalAmount: 0,
            pendingOrders: 0,
            completedOrders: 0
          }
        }
      }
      
      return {
        success: response.ok,
        data: data
      }
    } catch (error) {
      return { 
        success: false, 
        data: {
          message: error.message || '获取订单统计失败',
          totalOrders: 0,
          totalAmount: 0,
          pendingOrders: 0,
          completedOrders: 0
        } 
      }
    }
  },

  // 上传头像 - 确保正确上传到数据库Avatar字段
  async uploadAvatar(formData) {
    try {
      // 检查是否已经是FormData对象，如果不是则创建
      const dataToSend = formData instanceof FormData ? formData : new FormData()
      
      // 如果传入的不是FormData且是文件对象，则添加到dataToSend
      if (!(formData instanceof FormData) && formData instanceof File) {
        dataToSend.append('file', formData)
      }
      
      // 确保文件已正确添加到FormData
      if (!dataToSend.has('file')) {
        throw new Error('请选择要上传的头像文件')
      }
      
      const token = localStorage.getItem('token')
      if (!token) {
        throw new Error('用户未登录，无法上传头像')
      }
      
      // 发送真实的API请求到后端
      const response = await fetch('/api/auth/upload-avatar', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`
          // 注意：不要设置Content-Type，浏览器会自动设置正确的multipart/form-data
        },
        body: dataToSend
      })
      
      // 检查响应状态
      if (!response.ok) {
        throw new Error(`API返回错误状态码: ${response.status}`)
      }
      
      // 解析响应
      let data = null
      try {
        data = await response.json()
      } catch (jsonError) {
        // 尝试获取原始响应文本
        const responseText = await response.text()
        throw new Error('API响应格式错误: ' + jsonError.message)
      }
      
      // 检查响应是否表示失败
      if (data && (data.Success === false || data.success === false)) {
        throw new Error(data.Message || data.message || 'API返回失败状态')
      }
      
      // 后端返回的响应格式可能是 { Success: true, Message: "头像上传成功", AvatarUrl: avatarUrl }
      // 或者 { success: true, message: "头像上传成功", data: { AvatarUrl: avatarUrl } }
      let avatarUrl = null
      
      // 检查响应中的所有可能位置
      const urlFields = ['AvatarUrl', 'avatarUrl', 'avatar', 'url', 'avatar_url', 'imageUrl', 'fileUrl']
      
      // 1. 首先检查顶级字段
      for (const field of urlFields) {
        if (data && data[field]) {
          avatarUrl = data[field]
          break
        }
      }
      
      // 2. 如果没有找到，检查data对象中的字段
      if (!avatarUrl && data && data.data) {
        for (const field of urlFields) {
          if (data.data[field]) {
            avatarUrl = data.data[field]
            break
          }
        }
      }
      
      // 3. 检查data对象本身是否就是URL
      if (!avatarUrl && data && data.data && typeof data.data === 'string') {
        if (data.data.includes('uploads/avatars/') || /\.(jpg|jpeg|png|gif|webp)$/i.test(data.data)) {
          avatarUrl = data.data
        }
      }
      
      // 4. 如果没有找到URL，尝试从message中提取文件名，然后构建URL
      if (!avatarUrl && data && (data.Message || data.message)) {
        const message = data.Message || data.message
        // 尝试从message中提取文件名
        const fileNameMatch = message.match(/"([^"]+)"/)
        if (fileNameMatch && fileNameMatch[1]) {
          // 构建头像URL
          avatarUrl = `/uploads/avatars/${fileNameMatch[1]}`
        }
      }
      
      // 确保头像URL格式正确，添加uploads/avatars前缀（如果需要）
      if (avatarUrl) {
        // 如果只有文件名，添加uploads/avatars前缀
        if (avatarUrl.includes('.') && !avatarUrl.includes('/') && !avatarUrl.startsWith('http')) {
          avatarUrl = `/uploads/avatars/${avatarUrl}`
        }
        // 确保以/开头
        else if (!avatarUrl.startsWith('http') && !avatarUrl.startsWith('/')) {
          avatarUrl = '/' + avatarUrl
        }
      }
      
      // 如果获取到了URL，返回成功
      if (avatarUrl) {
        return {
          success: true,
          message: data.Message || data.message || '头像上传成功',
          data: { avatarUrl },
          avatarUrl,
          AvatarUrl: avatarUrl // 添加这个字段以保持与后端响应格式一致
        }
      }
      
      throw new Error('未能从API响应中获取头像URL')
    } catch (error) {
      // 返回统一格式的错误响应
      return {
        success: false,
        message: error.message || '头像上传失败',
        Success: false,
        data: { error: error.message || '头像上传失败' }
      }
    }
  }
}

export default userService
