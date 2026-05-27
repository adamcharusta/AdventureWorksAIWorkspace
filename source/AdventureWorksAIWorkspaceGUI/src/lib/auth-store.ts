/**
 * Simple auth store for managing JWT tokens
 */

const TOKEN_KEY = 'auth_token'
const REFRESH_TOKEN_KEY = 'refresh_token'
const AUTH_CHANGED_EVENT = 'auth-changed'

type AuthListener = () => void

const listeners = new Set<AuthListener>()

function notifyAuthChanged() {
  listeners.forEach((listener) => listener())
  if (typeof window !== 'undefined') {
    window.dispatchEvent(new Event(AUTH_CHANGED_EVENT))
  }
}

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const parts = token.split('.')
    if (parts.length !== 3) {
      return null
    }

    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const padded = base64.padEnd(
      base64.length + ((4 - (base64.length % 4)) % 4),
      '=',
    )
    const payload = atob(padded)
    return JSON.parse(payload) as Record<string, unknown>
  } catch {
    return null
  }
}

export const authStore = {
  getToken: (): string | null => {
    return localStorage.getItem(TOKEN_KEY)
  },

  setToken: (token: string): void => {
    localStorage.setItem(TOKEN_KEY, token)
    notifyAuthChanged()
  },

  getRefreshToken: (): string | null => {
    return localStorage.getItem(REFRESH_TOKEN_KEY)
  },

  setRefreshToken: (token: string): void => {
    localStorage.setItem(REFRESH_TOKEN_KEY, token)
    notifyAuthChanged()
  },

  clearTokens: (): void => {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    notifyAuthChanged()
  },

  isAuthenticated: (): boolean => {
    return authStore.getToken() !== null
  },

  isAccessTokenValid: (leewayInSeconds = 30): boolean => {
    const token = authStore.getToken()
    if (!token) {
      return false
    }

    const payload = decodeJwtPayload(token)
    const exp = payload?.exp

    if (typeof exp !== 'number') {
      return false
    }

    const nowInSeconds = Math.floor(Date.now() / 1000)
    return exp - leewayInSeconds > nowInSeconds
  },

  subscribe: (listener: AuthListener): (() => void) => {
    listeners.add(listener)
    return () => {
      listeners.delete(listener)
    }
  },
}
