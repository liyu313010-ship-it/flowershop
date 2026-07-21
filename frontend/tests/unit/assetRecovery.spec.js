import { buildCacheRetryUrl, installAssetCacheRecovery } from '@/utils/assetRecovery'

describe('静态资源缓存故障恢复', () => {
  it('为同源资源保留原参数并加入缓存重试标记', () => {
    expect(buildCacheRetryUrl(
      '/images/主图.jpg?size=large',
      'http://8.148.9.196:8080/products/1',
      'retry-1'
    )).toBe('http://8.148.9.196:8080/images/%E4%B8%BB%E5%9B%BE.jpg?size=large&__cache_retry=retry-1')
  })

  it('不接管跨域、data 和 blob 资源', () => {
    const base = 'http://8.148.9.196:8080/'
    expect(buildCacheRetryUrl('https://example.com/flower.jpg', base)).toBeNull()
    expect(buildCacheRetryUrl('data:image/png;base64,abc', base)).toBeNull()
    expect(buildCacheRetryUrl('blob:http://8.148.9.196:8080/id', base)).toBeNull()
  })

  it('图片首次失败时换成绕过缓存的地址，第二次失败交还组件处理', () => {
    const removeRecovery = installAssetCacheRecovery(window)
    const image = document.createElement('img')
    image.src = '/images/catalog/four-season-garden.webp'
    document.body.appendChild(image)

    const firstError = new Event('error', { cancelable: true })
    image.dispatchEvent(firstError)

    expect(image.dataset.cacheRetried).toBe('true')
    expect(image.src).toContain('__cache_retry=')
    expect(firstError.defaultPrevented).toBe(true)

    const secondError = new Event('error', { cancelable: true })
    image.dispatchEvent(secondError)
    expect(secondError.defaultPrevented).toBe(false)

    removeRecovery()
    image.remove()
  })
})
