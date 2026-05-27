import { useCallback, useSyncExternalStore } from 'react'

import { authStore } from './auth-store'

export const useAuth = () => {
  const token = useSyncExternalStore(
    authStore.subscribe,
    authStore.getToken,
    authStore.getToken,
  )

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
    login,
    logout,
  }
}
