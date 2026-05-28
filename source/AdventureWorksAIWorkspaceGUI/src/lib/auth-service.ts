import { authStore } from './auth-store'

type RefreshResponseBody = {
  accessToken: string
  refreshToken: string
}

type RefreshSessionOptions = {
  clearSessionOnFailure?: boolean
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

function createRefreshHeaders(): Headers {
  const headers = new Headers({ 'Content-Type': 'application/json' })
  const authorizationHeader = authStore.getAuthorizationHeader()

  if (authorizationHeader) {
    headers.set('Authorization', authorizationHeader)
  }

  return headers
}

function clearTokensIfNeeded(clearSessionOnFailure: boolean) {
  if (clearSessionOnFailure) {
    authStore.clearTokens()
  }
}

async function executeRefresh({
  clearSessionOnFailure = true,
}: RefreshSessionOptions = {}): Promise<boolean> {
  const refreshToken = authStore.getRefreshToken()

  if (!refreshToken) {
    clearTokensIfNeeded(clearSessionOnFailure)
    return false
  }

  const response = await fetch('/api/auth/refresh', {
    method: 'POST',
    headers: createRefreshHeaders(),
    body: JSON.stringify({ refreshToken }),
  })

  if (!response.ok) {
    clearTokensIfNeeded(clearSessionOnFailure)
    return false
  }

  const body = parseJson(await response.text())

  if (!isRefreshResponseBody(body)) {
    clearTokensIfNeeded(clearSessionOnFailure)
    return false
  }

  authStore.setToken(body.accessToken)
  authStore.setRefreshToken(body.refreshToken)
  return true
}

export async function refreshSession(
  options?: RefreshSessionOptions,
): Promise<boolean> {
  if (!refreshPromise) {
    refreshPromise = executeRefresh(options).finally(() => {
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
