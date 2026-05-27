import type {
  LoginResponse,
  SetFirstPasswordResponse,
} from '../api/generated/model'

const AUTH_TOKEN_KEY = 'auth_token'
const REFRESH_TOKEN_KEY = 'refresh_token'

export function createValidJwtToken() {
  // header: {"alg":"HS256","typ":"JWT"}
  // payload: {"exp":4102444800} // far-future expiry for test stability
  return 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjQxMDI0NDQ4MDB9.signature'
}

export function createAuthSession() {
  return {
    accessToken: createValidJwtToken(),
    refreshToken: 'refresh-token',
  }
}

export function setAuthenticatedSession() {
  const session = createAuthSession()
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
