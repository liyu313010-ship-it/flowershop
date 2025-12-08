<template>
  <span>
    <template v-for="(seg, i) in segments" :key="i">
      <span v-if="seg.type === 'text'">{{ seg.value }}</span>
      <a v-else href="javascript:void(0)" @click="handleClick(seg.value)" class="text-huanyu-pink-600 hover:underline cursor-pointer">@{{ seg.value }}</a>
    </template>
  </span>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { productService } from '@/services/product'
import { notifyError } from '@/utils/notify'

const props = defineProps({
  text: { type: String, default: '' }
})

const router = useRouter()

const segments = computed(() => {
  const result = []
  const regex = /@([^\s@]+)/g
  let lastIndex = 0
  let m
  while ((m = regex.exec(props.text)) !== null) {
    const start = m.index
    if (start > lastIndex) result.push({ type: 'text', value: props.text.slice(lastIndex, start) })
    result.push({ type: 'product', value: m[1] })
    lastIndex = regex.lastIndex
  }
  if (lastIndex < props.text.length) result.push({ type: 'text', value: props.text.slice(lastIndex) })
  return result
})

const handleClick = async (token) => {
  const idNum = Number(token)
  if (Number.isFinite(idNum) && idNum > 0) {
    try { await router.push(`/product/${idNum}`) } catch { window.location.href = `/product/${idNum}` }
    return
  }
  try {
    const res = await productService.searchProducts({ Keyword: token, Page: 1, PageSize: 50 })
    const items = (res && (res.Items || res.items)) || (res?.data && (res.data.Items || res.data.items)) || []
    const normalize = (s) => String(s || '').replace(/\s+/g, '').toLowerCase()
    const t = normalize(token)
    const item = Array.isArray(items)
      ? (items.find(x => normalize(x?.Name || x?.name) === t)
        || items.find(x => String(x?.Name || x?.name || '').startsWith(token))
        || items.find(x => String(x?.Name || x?.name || '').includes(token))
        || items[0])
      : null
    const id = item?.id || item?.Id
    if (id) {
      try { await router.push(`/product/${id}`) } catch { window.location.href = `/product/${id}` }
    } else {
      notifyError('未找到对应商品')
    }
  } catch {
    notifyError('未找到对应商品')
  }
}
</script>
