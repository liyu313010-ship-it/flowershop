import { access, mkdir, writeFile } from 'node:fs/promises'
import { dirname, join } from 'node:path'

const origin = new URL(process.argv[2] || 'http://8.148.9.196:8080')
const distDir = process.argv[3] || 'dist'
const queue = [new URL('/', origin)]
const visited = new Set()
let preserved = 0

const assetPattern = /(?:["'(=]|url\(\s*)(\/assets\/[A-Za-z0-9_.-]+|\.\/[A-Za-z0-9_.-]+\.(?:js|css|map|woff2?|ttf|svg|png|jpe?g|webp|avif|gif))/g

const exists = async (path) => {
  try {
    await access(path)
    return true
  } catch {
    return false
  }
}

while (queue.length > 0 && visited.size < 500) {
  const url = queue.shift()
  const key = url.href
  if (visited.has(key)) continue
  visited.add(key)

  let response
  try {
    response = await fetch(url, { signal: AbortSignal.timeout(10_000) })
  } catch (error) {
    console.warn(`[assets] 跳过不可访问的生产资源 ${url}: ${error.message}`)
    continue
  }
  if (!response.ok) continue

  const bytes = new Uint8Array(await response.arrayBuffer())
  const contentType = response.headers.get('content-type') || ''
  const isText = /(?:text|javascript|json|xml|svg)/i.test(contentType)
    || /\.(?:html|js|css|map|svg)$/i.test(url.pathname)

  if (url.pathname.startsWith('/assets/')) {
    const relativePath = decodeURIComponent(url.pathname.slice('/assets/'.length))
    if (!relativePath || relativePath.includes('..')) continue
    const destination = join(distDir, 'assets', relativePath)
    if (!(await exists(destination))) {
      await mkdir(dirname(destination), { recursive: true })
      await writeFile(destination, bytes)
      preserved += 1
    }
  }

  if (!isText) continue
  const text = new TextDecoder().decode(bytes)
  for (const match of text.matchAll(assetPattern)) {
    const candidate = match[1]
    const resolved = candidate.startsWith('/assets/')
      ? new URL(candidate, origin)
      : new URL(candidate, url)
    if (resolved.origin === origin.origin && resolved.pathname.startsWith('/assets/')) {
      queue.push(resolved)
    }
  }
}

console.log(`[assets] 已从当前生产版本保留 ${preserved} 个旧哈希资源，避免发布中的旧页面模块失效。`)
