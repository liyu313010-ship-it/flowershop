// This is only a client-side expiry check; the API still verifies JWT signatures.
export function isUsableToken(token, now = Date.now()) {
  try {
    const part = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
    const payload = JSON.parse(atob(part.padEnd(Math.ceil(part.length / 4) * 4, '=')))
    return Number.isFinite(payload.exp) && payload.exp * 1000 > now
  } catch {
    return false
  }
}

export function isCredentialRequest(url = '') {
  return /\/auth\/(login|register|forgot-password|reset-password)(?:[/?]|$)/i.test(url)
}

export function requiresSession(url = '') {
  return /\/(?:Carts|chat|Favorite|Profile|User|Addresses|Orders|admin|OrderStatusHistory)(?:[/?]|$)|\/Recommendations\/user(?:[/?]|$)|\/auth\/(?:profile|refresh|change-password)(?:[/?]|$)/i.test(url)
}

// An old in-flight response must never log out a newly signed-in account.
export function invalidateRejectedSession(config, currentToken, clearSession) {
  if (config?.skipSessionHandling || isCredentialRequest(config?.url)) return false
  const authorization = config?.headers?.Authorization || config?.headers?.authorization
  if (!currentToken || authorization !== `Bearer ${currentToken}`) return false
  clearSession()
  return true
}

export async function endSession(token, clearSession, revoke) {
  clearSession()
  if (isUsableToken(token)) await revoke(token)
}
