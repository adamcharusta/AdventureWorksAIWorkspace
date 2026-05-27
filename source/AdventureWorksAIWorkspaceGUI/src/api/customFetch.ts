import { refreshSession } from '../lib/auth-service'
import { authStore } from '../lib/auth-store'

export class ApiError extends Error {
  readonly status: number
  readonly body: unknown

  constructor(status: number, body: unknown, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.body = body
  }
}

type CustomFetchOptions = RequestInit & {
  skipAuthRefresh?: boolean
}

function isAuthEndpoint(url: string): boolean {
  const path = new URL(url, window.location.origin).pathname
  return (
    path === '/api/auth/login' ||
    path === '/api/auth/refresh' ||
    path === '/api/auth/set-first-password'
  )
}

export const customFetch = async <T>(
  url: string,
  options?: CustomFetchOptions,
): Promise<T> => {
  const runRequest = async () => {
    const headers = new Headers(options?.headers || {})

    // Inject JWT token if available
    const token = authStore.getToken()
    if (token) {
      headers.set('Authorization', `Bearer ${token}`)
    }

    // Set default content type for JSON if not already set
    if (!headers.has('Content-Type')) {
      headers.set('Content-Type', 'application/json')
    }

    return fetch(url, {
      ...options,
      headers,
    })
  }

  let response = await runRequest()

  if (
    response.status === 401 &&
    !options?.skipAuthRefresh &&
    !isAuthEndpoint(url)
  ) {
    const refreshed = await refreshSession()

    if (refreshed) {
      response = await runRequest()
    }
  }

  const rawBody = [204, 205, 304].includes(response.status)
    ? ''
    : await response.text()
  const parsedBody: unknown = rawBody ? safeJsonParse(rawBody) : undefined

  // Handle 401 Unauthorized - clear tokens on auth failure
  if (response.status === 401) {
    authStore.clearTokens()
  }

  if (!response.ok) {
    throw new ApiError(
      response.status,
      parsedBody,
      `Request to ${url} failed with status ${response.status}`,
    )
  }

  return {
    data: parsedBody,
    status: response.status,
    headers: response.headers,
  } as T
}

function safeJsonParse(raw: string): unknown {
  try {
    return JSON.parse(raw)
  } catch {
    return raw
  }
}
