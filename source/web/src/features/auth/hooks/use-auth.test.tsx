import { renderHook } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { useAuth } from './use-auth'

function encodeBase64Url(value: unknown): string {
  return btoa(JSON.stringify(value))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/g, '')
}

function createJwt(payload: Record<string, unknown>): string {
  const header = { alg: 'HS256', typ: 'JWT' }
  return `${encodeBase64Url(header)}.${encodeBase64Url(payload)}.signature`
}

describe('useAuth', () => {
  it('returns unauthenticated state when token is missing', () => {
    const { result } = renderHook(() => useAuth())

    expect(result.current.isAuthenticated).toBe(false)
    expect(result.current.userId).toBeNull()
    expect(result.current.username).toBeNull()
    expect(result.current.role).toBeNull()
  })

  it('extracts username and role from jwt claims', () => {
    localStorage.setItem(
      'auth_token',
      createJwt({
        exp: 4102444800,
        sub: 'user-1',
        name: 'alice',
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': 'Admin',
      }),
    )

    const { result } = renderHook(() => useAuth())

    expect(result.current.isAuthenticated).toBe(true)
    expect(result.current.userId).toBe('user-1')
    expect(result.current.username).toBe('alice')
    expect(result.current.role).toBe('Admin')
  })

  it('uses first non-empty role when token contains role array', () => {
    localStorage.setItem(
      'auth_token',
      createJwt({
        exp: 4102444800,
        preferred_username: 'bob',
        roles: ['', 'Manager', 'Viewer'],
      }),
    )

    const { result } = renderHook(() => useAuth())

    expect(result.current.username).toBe('bob')
    expect(result.current.role).toBe('Manager')
  })

  it('returns null claims for malformed jwt', () => {
    localStorage.setItem('auth_token', 'invalid.token')

    const { result } = renderHook(() => useAuth())

    expect(result.current.isAuthenticated).toBe(true)
    expect(result.current.username).toBeNull()
    expect(result.current.role).toBeNull()
  })
})
