import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'

import { authStore } from '@/features/auth/lib/auth-store'
import { server } from '@/test/server'

import { customFetch } from './customFetch'

describe('customFetch', () => {
  it('injects the cached JWT access token into request headers', async () => {
    authStore.setToken('cached-jwt')

    server.use(
      http.get('*/api/users', ({ request }) => {
        expect(request.headers.get('Authorization')).toBe('Bearer cached-jwt')

        return HttpResponse.json({ users: [] })
      }),
    )

    await expect(customFetch('/api/users')).resolves.toMatchObject({
      status: 200,
    })
  })

  it('uses the refreshed cached JWT when retrying after a 401', async () => {
    authStore.setToken('expired-jwt')
    authStore.setRefreshToken('refresh-token')

    let usersRequestCount = 0

    server.use(
      http.get('*/api/users', ({ request }) => {
        usersRequestCount += 1

        if (usersRequestCount === 1) {
          expect(request.headers.get('Authorization')).toBe(
            'Bearer expired-jwt',
          )

          return new HttpResponse(null, { status: 401 })
        }

        expect(request.headers.get('Authorization')).toBe('Bearer fresh-jwt')

        return HttpResponse.json({ users: [] })
      }),
      http.post('*/api/auth/refresh', async ({ request }) => {
        const body = (await request.json()) as { refreshToken?: string }

        expect(request.headers.get('Authorization')).toBe('Bearer expired-jwt')
        expect(body.refreshToken).toBe('refresh-token')

        return HttpResponse.json({
          accessToken: 'fresh-jwt',
          refreshToken: 'fresh-refresh-token',
        })
      }),
    )

    await expect(customFetch('/api/users')).resolves.toMatchObject({
      status: 200,
    })

    expect(usersRequestCount).toBe(2)
    expect(authStore.getToken()).toBe('fresh-jwt')
  })

  it('refreshes a 401 response even when final session clearing is disabled', async () => {
    authStore.setToken('expired-jwt')
    authStore.setRefreshToken('refresh-token')

    let usersRequestCount = 0

    server.use(
      http.get('*/api/users', ({ request }) => {
        usersRequestCount += 1

        if (usersRequestCount === 1) {
          expect(request.headers.get('Authorization')).toBe(
            'Bearer expired-jwt',
          )

          return new HttpResponse(null, { status: 401 })
        }

        expect(request.headers.get('Authorization')).toBe('Bearer fresh-jwt')

        return HttpResponse.json({ users: [] })
      }),
      http.post('*/api/auth/refresh', () =>
        HttpResponse.json({
          accessToken: 'fresh-jwt',
          refreshToken: 'fresh-refresh-token',
        }),
      ),
    )

    await expect(
      customFetch('/api/users', { clearSessionOnUnauthorized: false }),
    ).resolves.toMatchObject({
      status: 200,
    })

    expect(usersRequestCount).toBe(2)
    expect(authStore.getToken()).toBe('fresh-jwt')
  })

  it('keeps cached tokens when refresh fails and session clearing is disabled', async () => {
    authStore.setToken('expired-jwt')
    authStore.setRefreshToken('refresh-token')

    server.use(
      http.get('*/api/users', () => new HttpResponse(null, { status: 401 })),
      http.post(
        '*/api/auth/refresh',
        () => new HttpResponse(null, { status: 401 }),
      ),
    )

    await expect(
      customFetch('/api/users', { clearSessionOnUnauthorized: false }),
    ).rejects.toMatchObject({
      status: 401,
    })

    expect(authStore.getToken()).toBe('expired-jwt')
    expect(authStore.getRefreshToken()).toBe('refresh-token')
  })
})
