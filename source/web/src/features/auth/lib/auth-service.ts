import { safeJsonParse } from '@/shared/lib/json'

import { authStore } from './auth-store'

type RefreshResponseBody = {
  accessToken: string
  refreshToken: string
}

type RefreshSessionOptions = {
  clearSessionOnFailure?: boolean
}

// Single-flight guard: several API calls can discover an expired access token at the same time, but
// they should all wait for one refresh request instead of racing and overwriting stored tokens.
let refreshPromise: Promise<boolean> | null = null

/**
 * Runtime shape check for `/api/auth/refresh`.
 *
 * Generated types are not available at this fetch boundary because the refresh flow must run before
 * the authenticated API client can safely retry requests.
 */
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

/**
 * Builds refresh headers using the current access token when one exists.
 *
 * Some backends accept refresh-token-only requests; keeping the access token when available gives
 * the API enough context for audit/logging or stricter refresh policies.
 */
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

/**
 * Calls the refresh endpoint and updates stored tokens on success.
 *
 * `clearSessionOnFailure` lets callers decide whether a failed refresh should immediately log the
 * user out or simply report `false` while keeping the current token state untouched.
 */
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

  const body = safeJsonParse(await response.text(), undefined)

  if (!isRefreshResponseBody(body)) {
    clearTokensIfNeeded(clearSessionOnFailure)
    return false
  }

  authStore.setToken(body.accessToken)
  authStore.setRefreshToken(body.refreshToken)
  return true
}

/**
 * Refreshes the session, sharing the same in-flight promise across concurrent callers.
 */
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

/**
 * Returns whether the user currently has a usable session, refreshing expired access tokens when
 * possible.
 */
export async function ensureValidSession(): Promise<boolean> {
  if (!authStore.isAuthenticated()) {
    return false
  }

  if (authStore.isAccessTokenValid()) {
    return true
  }

  return refreshSession()
}
