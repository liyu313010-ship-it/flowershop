<template>
  <div class="chat-attachment" :class="{ 'is-image': isImage }">
    <div v-if="!message.attachmentAvailable" class="attachment-unavailable">附件已不可用</div>
    <template v-else-if="isImage">
      <button type="button" class="image-preview" :disabled="isLoading" @click="openPreview">
        <img v-if="objectUrl" :src="objectUrl" :alt="message.attachmentName || '聊天图片'" />
        <span v-else>{{ errorMessage || '图片加载中…' }}</span>
      </button>
      <button type="button" class="attachment-download" @click="downloadFile">
        下载原图 · {{ formatSize(message.attachmentSize) }}
      </button>
    </template>
    <button v-else type="button" class="file-card" :disabled="isLoading" @click="downloadFile">
      <span class="file-icon" aria-hidden="true">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
          <path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z" />
          <path d="M14 2v6h6M8 13h8M8 17h5" />
        </svg>
      </span>
      <span class="file-details">
        <strong>{{ message.attachmentName || '附件' }}</strong>
        <small>{{ isLoading ? '正在下载…' : formatSize(message.attachmentSize) }}</small>
      </span>
      <span class="download-arrow" aria-hidden="true">↓</span>
    </button>
    <p v-if="errorMessage && !isImage" class="attachment-error">{{ errorMessage }}</p>
  </div>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import chatService from '@/services/chat'

const props = defineProps({ message: { type: Object, required: true } })
const objectUrl = ref('')
const cachedBlob = ref(null)
const isLoading = ref(false)
const errorMessage = ref('')
const isImage = computed(() => props.message.messageType === 'image'
  || props.message.attachmentContentType?.startsWith('image/'))

const revokeObjectUrl = () => {
  if (objectUrl.value) URL.revokeObjectURL(objectUrl.value)
  objectUrl.value = ''
  cachedBlob.value = null
}

const fetchBlob = async () => {
  if (cachedBlob.value) return cachedBlob.value
  isLoading.value = true
  errorMessage.value = ''
  try {
    cachedBlob.value = await chatService.getAttachment(props.message.id)
    return cachedBlob.value
  } catch (error) {
    errorMessage.value = error?.response?.status === 404 ? '附件已不存在' : '附件加载失败'
    throw error
  } finally {
    isLoading.value = false
  }
}

const loadImage = async () => {
  if (!isImage.value || !props.message.attachmentAvailable) return
  try {
    const blob = await fetchBlob()
    objectUrl.value = URL.createObjectURL(blob)
  } catch {}
}

const openPreview = () => {
  if (objectUrl.value) window.open(objectUrl.value, '_blank', 'noopener,noreferrer')
}

const downloadFile = async () => {
  try {
    const blob = await fetchBlob()
    const url = objectUrl.value || URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = props.message.attachmentName || 'attachment'
    document.body.appendChild(anchor)
    anchor.click()
    anchor.remove()
    if (!objectUrl.value) URL.revokeObjectURL(url)
  } catch {}
}

const formatSize = value => {
  const size = Number(value || 0)
  if (size < 1024) return `${size} B`
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`
  return `${(size / 1024 / 1024).toFixed(1)} MB`
}

onMounted(loadImage)
watch(() => props.message.id, () => { revokeObjectUrl(); loadImage() })
onBeforeUnmount(revokeObjectUrl)
</script>

<style scoped>
.chat-attachment { min-width: 210px; max-width: 320px; }
.image-preview { display: block; width: 100%; min-height: 90px; overflow: hidden; border-radius: 12px; background: rgba(255,255,255,.2); color: inherit; font-size: 12px; }
.image-preview img { display: block; width: 100%; max-height: 260px; object-fit: cover; }
.attachment-download { margin-top: 6px; color: inherit; font-size: 11px; text-decoration: underline; text-underline-offset: 2px; opacity: .86; }
.file-card { width: 100%; display: flex; align-items: center; gap: 10px; padding: 10px; border-radius: 12px; color: inherit; background: rgba(255,255,255,.16); text-align: left; }
.file-icon { width: 34px; height: 34px; display: grid; place-items: center; flex: 0 0 auto; border-radius: 9px; background: rgba(255,255,255,.72); color: #be185d; }
.file-icon svg { width: 20px; height: 20px; stroke-width: 1.7; }
.file-details { min-width: 0; display: flex; flex: 1; flex-direction: column; }
.file-details strong { overflow: hidden; font-size: 12px; text-overflow: ellipsis; white-space: nowrap; }
.file-details small { margin-top: 2px; font-size: 10px; opacity: .72; }
.download-arrow { font-size: 18px; }
.attachment-unavailable, .attachment-error { font-size: 12px; opacity: .75; }
.attachment-error { margin-top: 5px; color: #be123c; }
</style>
