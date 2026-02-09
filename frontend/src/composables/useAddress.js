import { ref, computed } from 'vue'
import userService from '@/services/userService'
import { notifySuccess, notifyError } from '@/utils/notify'
import { getProvinces, getCities, getDistricts, getProvinceName, getCityName, getDistrictName, getProvinceCodeByName, getCityCodeByName, getDistrictCodeByName, ensureRegionDataLoaded } from '@/utils/regionData'

export function useAddress() {
  const addresses = ref([])
  const loading = ref(false)
  const showAddModal = ref(false)
  const showEditModal = ref(false)
  const editingId = ref(null)
  
  // 地区数据
  const provinces = ref([])
  const cities = ref([])
  const districts = ref([])
  
  // 表单数据
  const form = ref({
    name: '',
    phone: '',
    province: '',
    city: '',
    district: '',
    detailAddress: '',
    isDefault: false
  })

  // 初始化地区数据
  const initRegionData = async () => {
    await ensureRegionDataLoaded()
    provinces.value = getProvinces()
  }

  // 加载地址列表
  const loadAddresses = async () => {
    loading.value = true
    try {
      const res = await userService.getAddresses()
      if (res.success) {
        // 格式化地址数据
        const list = res.data || []
        addresses.value = Array.isArray(list) ? list.map(addr => ({
          id: addr.id || addr.Id,
          name: addr.name || addr.RecipientName || addr.recipientName || '',
          phone: addr.phone || addr.PhoneNumber || addr.phoneNumber || '',
          province: addr.province || addr.Province || '',
          city: addr.city || addr.City || '',
          district: addr.district || addr.District || '',
          detailAddress: addr.detailAddress || addr.DetailAddress || '',
          fullAddress: addr.fullAddress || `${addr.province || addr.Province || ''}${addr.city || addr.City || ''}${addr.district || addr.District || ''}${addr.detailAddress || addr.DetailAddress || ''}`,
          postalCode: addr.postalCode || addr.PostalCode || '',
          isDefault: addr.isDefault || addr.IsDefault || false
        })) : []
      } else {
        notifyError(res.data?.message || '获取地址列表失败')
        addresses.value = []
      }
    } catch (err) {
      console.error('加载地址失败:', err)
      notifyError('获取地址列表失败')
      addresses.value = []
    } finally {
      loading.value = false
    }
  }

  // 重置表单
  const resetForm = () => {
    form.value = {
      name: '',
      phone: '',
      province: '',
      city: '',
      district: '',
      detailAddress: '',
      isDefault: false
    }
    cities.value = []
    districts.value = []
    editingId.value = null
  }

  // 打开添加模态框
  const openAddModal = () => {
    resetForm()
    showAddModal.value = true
  }

  // 打开编辑模态框
  const openEditModal = (address) => {
    editingId.value = address.id
    
    // 尝试匹配地区代码
    const pCode = getProvinceCodeByName(address.province) || address.province
    const cCode = getCityCodeByName(pCode, address.city) || address.city
    const dCode = getDistrictCodeByName(pCode, cCode, address.district) || address.district

    form.value = {
      name: address.name,
      phone: address.phone,
      province: pCode || '',
      city: cCode || '',
      district: dCode || '',
      detailAddress: address.detailAddress,
      isDefault: address.isDefault
    }

    // 加载级联数据
    if (pCode) {
      cities.value = getCities(pCode)
      if (cCode) {
        districts.value = getDistricts(pCode, cCode)
      }
    }

    showEditModal.value = true
  }

  // 处理省份变化
  const handleProvinceChange = (code) => {
    cities.value = []
    districts.value = []
    form.value.city = ''
    form.value.district = ''
    if (code) {
      cities.value = getCities(code)
    }
  }

  // 处理城市变化
  const handleCityChange = (code) => {
    districts.value = []
    form.value.district = ''
    if (form.value.province && code) {
      districts.value = getDistricts(form.value.province, code)
    }
  }

  // 提交表单（添加或更新）
  const submitForm = async () => {
    if (!form.value.name || !form.value.phone || !form.value.province || !form.value.city || !form.value.district || !form.value.detailAddress) {
      notifyError('请填写完整地址信息')
      return false
    }

    loading.value = true
    try {
      const payload = {
        RecipientName: form.value.name,
        PhoneNumber: form.value.phone,
        Province: getProvinceName(form.value.province),
        City: getCityName(form.value.province, form.value.city),
        District: getDistrictName(form.value.province, form.value.city, form.value.district),
        DetailAddress: form.value.detailAddress,
        IsDefault: form.value.isDefault,
        PostalCode: null
      }

      let res
      if (editingId.value) {
        res = await userService.updateAddress(editingId.value, payload)
      } else {
        res = await userService.addAddress(payload)
      }

      if (res.success) {
        notifySuccess(editingId.value ? '地址更新成功' : '地址添加成功')
        await loadAddresses()
        showAddModal.value = false
        showEditModal.value = false
        resetForm()
        return true
      } else {
        notifyError(res.data?.message || (editingId.value ? '更新失败' : '添加失败'))
        return false
      }
    } catch (err) {
      notifyError(editingId.value ? '更新失败' : '添加失败')
      return false
    } finally {
      loading.value = false
    }
  }

  // 删除地址
  const removeAddress = async (id) => {
    if (!confirm('确定要删除这个地址吗？')) return
    
    try {
      const res = await userService.deleteAddress(id)
      if (res.success) {
        notifySuccess('地址删除成功')
        // 乐观更新
        addresses.value = addresses.value.filter(a => a.id !== id)
        await loadAddresses() // 确保同步
      } else {
        notifyError(res.data?.message || '删除失败')
      }
    } catch (err) {
      notifyError('删除失败')
    }
  }

  // 设置默认地址
  const setAsDefault = async (id) => {
    try {
      const res = await userService.setDefaultAddress(id)
      if (res.success) {
        notifySuccess('已设置为默认地址')
        await loadAddresses()
      } else {
        notifyError(res.data?.message || '设置失败')
      }
    } catch (err) {
      notifyError('设置失败')
    }
  }

  return {
    addresses,
    loading,
    showAddModal,
    showEditModal,
    form,
    provinces,
    cities,
    districts,
    initRegionData,
    loadAddresses,
    openAddModal,
    openEditModal,
    handleProvinceChange,
    handleCityChange,
    submitForm,
    removeAddress,
    setAsDefault
  }
}
