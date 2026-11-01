export function normalizeVideoPath(path) {
  if (!path || typeof path !== 'string') return ''
  try {
    const url = new URL(path.trim(), 'http://local.invalid')
    // Old tabs cached an HTML response under this missing video URL.
    if (url.pathname === '/videos/2.mp4') return '/videos/2.mp4?v=8362681-20261101'
    return path.trim()
  } catch { return '' }
}
