import type {
  LoginResponse,
  SetFirstPasswordResponse,
} from '@/api/generated/model'

const AUTH_TOKEN_KEY = 'auth_token'
const REFRESH_TOKEN_KEY = 'refresh_token'

function encodeBase64Url(value: Record<string, unknown>) {
  return btoa(JSON.stringify(value))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/g, '')
}

export function createValidJwtToken(claims: Record<string, unknown> = {}) {
  const header = encodeBase64Url({ alg: 'HS256', typ: 'JWT' })
  const payload = encodeBase64Url({
    ...claims,
    exp: 4102444800,
  })

  return `${header}.${payload}.signature`
}

export function createAuthSession(claims: Record<string, unknown> = {}) {
  return {
    accessToken: createValidJwtToken(claims),
    refreshToken: 'refresh-token',
  }
}

export function setAuthenticatedSession(claims: Record<string, unknown> = {}) {
  const session = createAuthSession(claims)
  localStorage.setItem(AUTH_TOKEN_KEY, session.accessToken)
  localStorage.setItem(REFRESH_TOKEN_KEY, session.refreshToken)
  return session
}

export function createLoginResponse(
  overrides: Partial<LoginResponse> = {},
): LoginResponse {
  const session = createAuthSession()
  return {
    accessToken: session.accessToken,
    accessTokenExpiresAt: '2100-01-01T00:00:00Z',
    refreshToken: session.refreshToken,
    refreshTokenExpiresAt: '2100-01-01T00:00:00Z',
    ...overrides,
  }
}

export function createSetFirstPasswordResponse(
  overrides: Partial<SetFirstPasswordResponse> = {},
): SetFirstPasswordResponse {
  const session = createAuthSession()
  return {
    accessToken: session.accessToken,
    accessTokenExpiresAt: '2100-01-01T00:00:00Z',
    refreshToken: session.refreshToken,
    refreshTokenExpiresAt: '2100-01-01T00:00:00Z',
    ...overrides,
  }
}
