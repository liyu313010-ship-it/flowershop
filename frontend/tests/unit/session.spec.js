import { isUsableToken, requiresSession, invalidateRejectedSession, endSession } from '@/utils/session'
import { normalizeVideoPath } from '@/utils/mediaPath'

const jwt = exp => `header.${btoa(JSON.stringify({ exp }))}.signature`

describe('session expiry and logout', () => {
  test('rejects missing, malformed and expired tokens', () => {
    for (const token of [null, '', 'garbage', jwt(100), jwt(101)]) {
      expect(isUsableToken(token, 101000)).toBe(false)
    }
    expect(isUsableToken(jwt(102), 101000)).toBe(true)
  })
  test.each(['/Carts', '/chat/unread-count', '/Recommendations/user?limit=8', '/auth/profile'])('protects %s', url => {
    expect(requiresSession(url)).toBe(true)
  })
  test.each(['/auth/login', '/Products', '/Recommendations/global'])('allows guest request %s', url => {
    expect(requiresSession(url)).toBe(false)
  })
  test('silent 401 clears local state once across concurrent failures', () => {
    let token = 'old'
    const clear = jest.fn(() => { token = null })
    const config = { url: '/chat/unread-count', silent: true, headers: { Authorization: 'Bearer old' } }
    expect(invalidateRejectedSession(config, token, clear)).toBe(true)
    expect(invalidateRejectedSession(config, token, clear)).toBe(false)
    expect(clear).toHaveBeenCalledTimes(1)
  })
  test('login, logout and stale responses never invalidate a new login', () => {
    const clear = jest.fn()
    for (const config of [
      { url: '/auth/login', headers: { Authorization: 'Bearer new' } },
      { url: '/auth/logout', skipSessionHandling: true, headers: { Authorization: 'Bearer new' } },
      { url: '/Carts', headers: { Authorization: 'Bearer old' } }
    ]) expect(invalidateRejectedSession(config, 'new', clear)).toBe(false)
    expect(clear).not.toHaveBeenCalled()
  })
  test('logout clears state before network and skips expired credentials', async () => {
    const clear = jest.fn()
    const revoke = jest.fn(() => {
      expect(clear).toHaveBeenCalledTimes(1)
      return Promise.resolve()
    })
    await endSession(jwt(Date.now() / 1000 + 60), clear, revoke)
    expect(revoke).toHaveBeenCalledTimes(1)
    await endSession(jwt(1), clear, revoke)
    expect(clear).toHaveBeenCalledTimes(2)
    expect(revoke).toHaveBeenCalledTimes(1)
  })
  test('versions legacy video cache without changing uploaded video URLs', () => {
    expect(normalizeVideoPath('/videos/2.mp4')).toBe('/videos/2.mp4?v=8362681-20261101')
    expect(normalizeVideoPath('http://8.148.9.196:8080/videos/2.mp4')).toBe('/videos/2.mp4?v=8362681-20261101')
    expect(normalizeVideoPath('/uploads/videos/flower.mp4')).toBe('/uploads/videos/flower.mp4')
  })
})
