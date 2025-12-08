<template>
  <div class="min-h-screen bg-gray-50">
    <div class="container mx-auto px-4 py-8">
      <div class="bg-white rounded-lg shadow p-6 mb-6">
        <div class="flex justify-between items-center mb-4">
          <h2 class="text-xl font-semibold">优惠券管理</h2>
          <div class="space-x-2">
            <button @click="openCreate" class="bg-huanyu-pink-600 hover:bg-huanyu-pink-700 text-white px-4 py-2 rounded-lg">新建优惠券</button>
            <button @click="openGrant" class="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-lg">发放优惠券</button>
          </div>
        </div>
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">Code</th>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">类型</th>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">数值</th>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">门槛</th>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">每人限领</th>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">总发放</th>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">已使用</th>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">状态</th>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">操作</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="c in coupons" :key="c.id">
                <td class="px-4 py-2 text-sm">{{ c.code }}</td>
                <td class="px-4 py-2 text-sm">{{ c.discountType }}</td>
                <td class="px-4 py-2 text-sm">{{ c.discountType==='percent' ? (c.value+'%') : ('¥'+c.value) }}</td>
                <td class="px-4 py-2 text-sm">¥{{ c.minOrderAmount }}</td>
                <td class="px-4 py-2 text-sm">{{ c.usageLimitPerUser ?? '-' }}</td>
                <td class="px-4 py-2 text-sm">{{ c.usageLimit ?? '-' }}</td>
                <td class="px-4 py-2 text-sm">{{ c.usedCount }}</td>
                <td class="px-4 py-2 text-sm">{{ c.status }}</td>
                <td class="px-4 py-2 text-sm">
                  <button @click="openEdit(c)" class="text-huanyu-pink-600 hover:text-huanyu-pink-900 mr-3">编辑</button>
                  <button @click="viewClaims(c)" class="text-blue-600 hover:text-blue-900 mr-3">领取</button>
                  <button @click="chooseGrant(c)" class="text-green-600 hover:text-green-900 mr-3">发放</button>
                  <button @click="remove(c.id)" class="text-red-600 hover:text-red-900">删除</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <el-dialog v-model="showForm" :title="form.id ? '编辑优惠券' : '新建优惠券'" width="600px">
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="text-sm">Code</label>
            <input v-model="form.code" class="border rounded w-full p-2" />
          </div>
          <div>
            <label class="text-sm">类型</label>
            <select v-model="form.discountType" class="border rounded w-full p-2">
              <option value="amount">固定金额</option>
              <option value="percent">百分比</option>
            </select>
          </div>
          <div>
            <label class="text-sm">数值</label>
            <input v-model.number="form.value" type="number" class="border rounded w-full p-2" />
          </div>
          <div>
            <label class="text-sm">门槛金额</label>
            <input v-model.number="form.minOrderAmount" type="number" class="border rounded w-full p-2" />
          </div>
          <div>
            <label class="text-sm">每人限领</label>
            <input v-model.number="form.usageLimitPerUser" type="number" class="border rounded w-full p-2" />
          </div>
          <div>
            <label class="text-sm">总发放上限</label>
            <input v-model.number="form.usageLimit" type="number" class="border rounded w-full p-2" />
          </div>
          <div>
            <label class="text-sm">状态</label>
            <select v-model="form.status" class="border rounded w-full p-2">
              <option value="active">active</option>
              <option value="inactive">inactive</option>
            </select>
          </div>
          <div>
            <label class="text-sm">起始时间</label>
            <input v-model="form.startAt" type="datetime-local" class="border rounded w-full p-2" />
          </div>
          <div>
            <label class="text-sm">结束时间</label>
            <input v-model="form.endAt" type="datetime-local" class="border rounded w-full p-2" />
          </div>
        </div>
        <template #footer>
          <div class="flex justify-end space-x-2">
            <button @click="showForm=false" class="px-4 py-2 border rounded">取消</button>
            <button @click="save" class="px-4 py-2 bg-huanyu-pink-600 text-white rounded">保存</button>
          </div>
        </template>
      </el-dialog>

      <el-dialog v-model="showGrant" title="发放优惠券" width="500px">
        <div class="space-y-4">
          <div>
            <label class="text-sm">用户ID</label>
            <input v-model.number="grant.userId" type="number" class="border rounded w-full p-2" />
          </div>
          <div>
            <label class="text-sm">优惠券</label>
            <select v-model.number="grant.couponId" class="border rounded w-full p-2">
              <option v-for="c in coupons" :key="c.id" :value="c.id">{{ c.code }} - {{ c.discountType==='percent' ? (c.value+'%') : ('¥'+c.value) }}</option>
            </select>
          </div>
        </div>
        <template #footer>
          <div class="flex justify-end space-x-2">
            <button @click="showGrant=false" class="px-4 py-2 border rounded">取消</button>
            <button @click="doGrant" class="px-4 py-2 bg-blue-600 text-white rounded">发放</button>
          </div>
        </template>
      </el-dialog>

      <el-dialog v-model="showClaims" title="领取列表" width="600px">
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead class="bg-gray-50">
              <tr>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">用户ID</th>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">状态</th>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">领取时间</th>
                <th class="px-4 py-2 text-left text-xs font-medium text-gray-500">使用时间</th>
              </tr>
            </thead>
            <tbody class="bg-white divide-y divide-gray-200">
              <tr v-for="uc in claims" :key="uc.Id || uc.id">
                <td class="px-4 py-2 text-sm">{{ uc.UserId || uc.userId }}</td>
                <td class="px-4 py-2 text-sm">{{ uc.Status || uc.status }}</td>
                <td class="px-4 py-2 text-sm">{{ (uc.ClaimedAt || uc.claimedAt) }}</td>
                <td class="px-4 py-2 text-sm">{{ (uc.UsedAt || uc.usedAt) || '-' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </el-dialog>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import AdminNav from '@/components/admin/AdminNav.vue'
import adminService from '@/services/adminService'
import { couponService } from '@/services/coupon'
import { ElMessage } from 'element-plus'

const coupons = ref([])
const showForm = ref(false)
const showClaims = ref(false)
const showGrant = ref(false)
const claims = ref([])
const form = ref({ id: null, code: '', discountType: 'amount', value: 0, minOrderAmount: 0, usageLimitPerUser: null, usageLimit: null, status: 'active', startAt: '', endAt: '' })
const grant = ref({ userId: null, couponId: null })

const load = async () => {
  try {
    const res = await adminService.getCoupons({ page: 1, limit: 100 })
    const data = res?.data?.data || res?.data || []
    coupons.value = data.map(c => ({ id: c.id || c.Id, code: c.code || c.Code, discountType: c.discountType || c.DiscountType, value: c.value || c.Value, minOrderAmount: c.minOrderAmount || c.MinOrderAmount || 0, usageLimitPerUser: c.usageLimitPerUser ?? c.UsageLimitPerUser ?? null, usageLimit: c.usageLimit ?? c.UsageLimit ?? null, usedCount: c.usedCount ?? c.UsedCount ?? 0, status: c.status || c.Status, startAt: c.startAt || c.StartAt, endAt: c.endAt || c.EndAt }))
  } catch (e) { coupons.value = [] }
}

const openCreate = () => { showForm.value = true; form.value = { id: null, code: '', discountType: 'amount', value: 0, minOrderAmount: 0, usageLimitPerUser: null, usageLimit: null, status: 'active', startAt: '', endAt: '' } }
const openEdit = (c) => {
  showForm.value = true;
  // 将 ISO 格式的日期转换为 datetime-local 输入框能识别的格式 (yyyy-MM-ddTHH:mm)
  const formatToDateTimeLocal = (isoString) => {
    if (!isoString) return '';
    const date = new Date(isoString);
    // 转换为本地时间并格式化为 yyyy-MM-ddTHH:mm
    return date.toISOString().slice(0, 16);
  };
  
  form.value = {
    ...c,
    startAt: formatToDateTimeLocal(c.startAt),
    endAt: formatToDateTimeLocal(c.endAt)
  };
}
const openGrant = () => { showGrant.value = true }
const chooseGrant = (c) => { grant.value.couponId = c.id; showGrant.value = true }

const save = async () => {
  try {
    // 确保数值字段是数字类型
    const validData = {
      Code: form.value.code.trim(),
      DiscountType: form.value.discountType,
      Value: parseFloat(form.value.value),
      MinOrderAmount: parseFloat(form.value.minOrderAmount),
      UsageLimitPerUser: form.value.usageLimitPerUser ? parseInt(form.value.usageLimitPerUser) : null,
      UsageLimit: form.value.usageLimit ? parseInt(form.value.usageLimit) : null,
      Status: form.value.status,
      StartAt: form.value.startAt ? new Date(form.value.startAt).toISOString() : null,
      EndAt: form.value.endAt ? new Date(form.value.endAt).toISOString() : null
    }
    
    console.log('保存优惠券请求数据:', validData)
    
    // 验证必填字段
    if (!validData.Code) {
      ElMessage.error('请输入优惠券代码')
      return
    }
    
    if (validData.Value <= 0) {
      ElMessage.error('优惠券数值必须大于0')
      return
    }
    
    // 发送请求
    let response
    if (form.value.id) {
      response = await adminService.updateCoupon(form.value.id, validData)
    } else {
      response = await adminService.createCoupon(validData)
    }
    
    console.log('保存优惠券响应数据:', response)
    
    ElMessage.success('保存成功')
    showForm.value = false
    await load()
  } catch (e) {
    console.error('保存优惠券失败:', e)
    // 显示详细错误信息
    const errorMsg = e.response?.data?.message || e.message || '保存失败'
    ElMessage.error(`保存失败: ${errorMsg}`)
  }
}

const remove = async (id) => { if (!confirm('确认删除?')) return; try { await adminService.deleteCoupon(id); await load(); } catch (e) { ElMessage.error('删除失败') } }
const viewClaims = async (c) => { try { const res = await adminService.getCouponClaims(c.id, { page: 1, limit: 100 }); claims.value = res?.data?.data || res?.data || []; showClaims.value = true } catch (e) { claims.value = []; showClaims.value = true } }

const doGrant = async () => {
  try {
    if (!grant.value.userId || !grant.value.couponId) { ElMessage.error('请填写用户ID与优惠券'); return }
    const res = await couponService.grantToUser(grant.value.userId, grant.value.couponId)
    ElMessage.success(res?.data?.message || '发放成功')
    showGrant.value = false
  } catch (e) {
    const msg = e?.response?.data?.message || e?.message || '发放失败'
    ElMessage.error(msg)
  }
}

onMounted(load)
</script>

<style scoped>
</style>
