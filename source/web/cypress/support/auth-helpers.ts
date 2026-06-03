type JwtClaims = Record<string, unknown>

function encodeBase64Url(value: unknown): string {
  return btoa(JSON.stringify(value))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/g, '')
}

export function createJwt(
  expiresInSeconds = 3600,
  claims: JwtClaims = {},
): string {
  const header = { alg: 'HS256', typ: 'JWT' }
  const payload = {
    sub: claims.sub ?? 'cypress-user',
    ...claims,
    exp: Math.floor(Date.now() / 1000) + expiresInSeconds,
  }

  return `${encodeBase64Url(header)}.${encodeBase64Url(payload)}.signature`
}

export function createAuthTokens(
  expiresInSeconds = 3600,
  claims: JwtClaims = {},
): {
  accessToken: string
  refreshToken: string
} {
  return {
    accessToken: createJwt(expiresInSeconds, claims),
    refreshToken: 'refresh-token',
  }
}

export function seedAuthSession(win: Window, claims: JwtClaims = {}) {
  const tokens = createAuthTokens(3600, claims)

  win.localStorage.setItem('auth_token', tokens.accessToken)
  win.localStorage.setItem('refresh_token', tokens.refreshToken)

  return tokens
}
