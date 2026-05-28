/**
 * Simple auth store for managing JWT tokens
 */
import { decodeJwtPayload } from './jwt'

const TOKEN_KEY = 'auth_token'
const REFRESH_TOKEN_KEY = 'refresh_token'
const AUTH_CHANGED_EVENT = 'auth-changed'

type AuthListener = () => void

const listeners = new Set<AuthListener>()

function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

function getAuthorizationHeader(): string | null {
  const token = getStoredToken()
  return token ? `Bearer ${token}` : null
}

function notifyAuthChanged() {
  listeners.forEach((listener) => listener())
  if (typeof window !== 'undefined') {
    window.dispatchEvent(new Event(AUTH_CHANGED_EVENT))
  }
}

export const authStore = {
  getToken: getStoredToken,

  getAuthorizationHeader,

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
