const CACHE_RETRY_PARAMETER = '__cache_retry'

export const buildCacheRetryUrl = (
  source,
  baseUrl = window.location.href,
  retryToken = Date.now().toString(36)
) => {
  if (!source) return null

  try {
    const url = new URL(source, baseUrl)
    const base = new URL(baseUrl)

    if (url.origin !== base.origin || !/^https?:$/.test(url.protocol)) return null

    url.searchParams.set(CACHE_RETRY_PARAMETER, retryToken)
    return url.href
  } catch {
    return null
  }
}

const getAssetSource = (asset) => {
  if (asset instanceof HTMLSourceElement) return asset.src
  if (asset instanceof HTMLVideoElement) return asset.currentSrc || asset.src
  if (asset instanceof HTMLImageElement) return asset.currentSrc || asset.src
  return ''
}

const retryAsset = (asset, retryUrl) => {
  if (asset instanceof HTMLSourceElement) {
    asset.src = retryUrl
    asset.parentElement?.load?.()
    return
  }

  asset.src = retryUrl
  if (asset instanceof HTMLVideoElement) asset.load()
}

/**
 * 捕获同源图片和视频的首次加载失败，用新查询参数绕过损坏的 HTTP 缓存。
 * 第二次仍失败时不再拦截，让组件原有的占位图/错误提示逻辑继续执行。
 */
export const installAssetCacheRecovery = (targetWindow = window) => {
  const retriedAssets = new WeakSet()

  const handleAssetError = (event) => {
    const asset = event.target
    const isRecoverableAsset = asset instanceof HTMLImageElement
      || asset instanceof HTMLVideoElement
      || asset instanceof HTMLSourceElement

    if (!isRecoverableAsset || retriedAssets.has(asset)) return

    const retryUrl = buildCacheRetryUrl(
      getAssetSource(asset),
      targetWindow.location.href
    )
    if (!retryUrl) return

    retriedAssets.add(asset)
    asset.dataset.cacheRetried = 'true'
    event.preventDefault()
    event.stopImmediatePropagation()
    retryAsset(asset, retryUrl)
  }

  targetWindow.addEventListener('error', handleAssetError, true)
  return () => targetWindow.removeEventListener('error', handleAssetError, true)
}
