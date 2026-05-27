function encodeBase64Url(value: unknown): string {
  return btoa(JSON.stringify(value))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/g, '')
}

export function createJwt(expiresInSeconds = 3600): string {
  const header = { alg: 'HS256', typ: 'JWT' }
  const payload = {
    sub: 'cypress-user',
    exp: Math.floor(Date.now() / 1000) + expiresInSeconds,
  }

  return `${encodeBase64Url(header)}.${encodeBase64Url(payload)}.signature`
}

export function createAuthTokens(expiresInSeconds = 3600): {
  accessToken: string
  refreshToken: string
} {
  return {
    accessToken: createJwt(expiresInSeconds),
    refreshToken: 'refresh-token',
  }
}
