import { useCallback } from 'react'

import { authStore } from './auth-store'

export const useAuth = () => {
  const login = useCallback((token: string, refreshToken?: string) => {
    authStore.setToken(token)
    if (refreshToken) {
      authStore.setRefreshToken(refreshToken)
    }
  }, [])

  const logout = useCallback(() => {
    authStore.clearTokens()
  }, [])

  const token = authStore.getToken()
  const isAuthenticated = authStore.isAuthenticated()

  return {
    token,
    isAuthenticated,
    login,
    logout,
  }
}
