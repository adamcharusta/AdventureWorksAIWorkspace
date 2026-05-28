import { useCallback, useMemo, useSyncExternalStore } from 'react'

import { authStore } from '../lib/auth-store'

const ROLE_CLAIM_KEYS = [
  'role',
  'roles',
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
] as const

const USERNAME_CLAIM_KEYS = [
  'name',
  'unique_name',
  'preferred_username',
  'username',
] as const

const USER_ID_CLAIM_KEYS = [
  'sub',
  'nameid',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
] as const

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

    return JSON.parse(atob(padded)) as Record<string, unknown>
  } catch {
    return null
  }
}

function getClaimValue(
  payload: Record<string, unknown>,
  keys: readonly string[],
) {
  for (const key of keys) {
    const value = payload[key]
    if (typeof value === 'string' && value.trim().length > 0) {
      return value
    }

    if (Array.isArray(value)) {
      const firstTextValue = value.find(
        (item): item is string =>
          typeof item === 'string' && item.trim().length > 0,
      )

      if (firstTextValue) {
        return firstTextValue
      }
    }
  }

  return null
}

export const useAuth = () => {
  const token = useSyncExternalStore(
    authStore.subscribe,
    authStore.getToken,
    authStore.getToken,
  )

  const payload = useMemo(() => {
    return token ? decodeJwtPayload(token) : null
  }, [token])

  const username = useMemo(() => {
    return payload ? getClaimValue(payload, USERNAME_CLAIM_KEYS) : null
  }, [payload])

  const userId = useMemo(() => {
    return payload ? getClaimValue(payload, USER_ID_CLAIM_KEYS) : null
  }, [payload])

  const role = useMemo(() => {
    return payload ? getClaimValue(payload, ROLE_CLAIM_KEYS) : null
  }, [payload])

  const login = useCallback((token: string, refreshToken?: string) => {
    authStore.setToken(token)
    if (refreshToken) {
      authStore.setRefreshToken(refreshToken)
    }
  }, [])

  const logout = useCallback(() => {
    authStore.clearTokens()
  }, [])

  const isAuthenticated = token !== null

  return {
    token,
    isAuthenticated,
    userId,
    username,
    role,
    login,
    logout,
  }
}
