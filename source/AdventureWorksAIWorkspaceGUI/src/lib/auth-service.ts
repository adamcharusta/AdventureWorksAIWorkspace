import { authStore } from './auth-store'

type RefreshResponseBody = {
  accessToken: string
  refreshToken: string
}

let refreshPromise: Promise<boolean> | null = null

function isRefreshResponseBody(value: unknown): value is RefreshResponseBody {
  if (!value || typeof value !== 'object') {
    return false
  }

  const candidate = value as Record<string, unknown>
  return (
    typeof candidate.accessToken === 'string' &&
    typeof candidate.refreshToken === 'string'
  )
}

function parseJson(raw: string): unknown {
  try {
    return JSON.parse(raw)
  } catch {
    return undefined
  }
}

async function executeRefresh(): Promise<boolean> {
  const refreshToken = authStore.getRefreshToken()

  if (!refreshToken) {
    authStore.clearTokens()
    return false
  }

  const response = await fetch('/api/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken }),
  })

  if (!response.ok) {
    authStore.clearTokens()
    return false
  }

  const body = parseJson(await response.text())

  if (!isRefreshResponseBody(body)) {
    authStore.clearTokens()
    return false
  }

  authStore.setToken(body.accessToken)
  authStore.setRefreshToken(body.refreshToken)
  return true
}

export async function refreshSession(): Promise<boolean> {
  if (!refreshPromise) {
    refreshPromise = executeRefresh().finally(() => {
      refreshPromise = null
    })
  }

  return refreshPromise
}

export async function ensureValidSession(): Promise<boolean> {
  if (!authStore.isAuthenticated()) {
    return false
  }

  if (authStore.isAccessTokenValid()) {
    return true
  }

  return refreshSession()
}
