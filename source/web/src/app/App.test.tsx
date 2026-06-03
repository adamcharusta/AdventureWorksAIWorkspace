import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'

import { getGetUsersMockHandler } from '@/api/generated/users/users.msw'
import { renderWithProviders } from '@/test/render-utils'
import { server } from '@/test/server'

import App from './App'

describe('<App />', () => {
  it('renders home page by default for authenticated users', async () => {
    renderWithProviders(<App />, { route: '/', isAuthenticated: true })

    expect(
      await screen.findByRole('heading', {
        name: /adventure works/i,
        level: 1,
      }),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /log out/i })).toBeInTheDocument()
  })

  it('redirects unauthenticated users from protected route to login page', () => {
    renderWithProviders(<App />, { route: '/', isAuthenticated: false })

    expect(
      screen.getByRole('heading', { name: /sign in/i, level: 2 }),
    ).toBeInTheDocument()
  })

  it('renders admin panel for authenticated admin users', async () => {
    server.use(getGetUsersMockHandler({ users: [] }))

    renderWithProviders(<App />, {
      authClaims: { role: 'Admin' },
      route: '/admin',
      isAuthenticated: true,
    })

    expect(
      await screen.findByRole('heading', { name: /admin panel/i, level: 1 }),
    ).toBeInTheDocument()
  })

  it('redirects authenticated non-admin users away from admin panel', async () => {
    renderWithProviders(<App />, {
      authClaims: { role: 'Viewer' },
      route: '/admin',
      isAuthenticated: true,
    })

    expect(
      await screen.findByRole('heading', {
        name: /adventure works/i,
        level: 1,
      }),
    ).toBeInTheDocument()
    expect(
      screen.queryByRole('heading', { name: /admin panel/i, level: 1 }),
    ).not.toBeInTheDocument()
  })

  it('logs out user and navigates to login page', async () => {
    const user = userEvent.setup()

    renderWithProviders(<App />, { route: '/', isAuthenticated: true })

    await user.click(await screen.findByRole('button', { name: /log out/i }))

    expect(localStorage.getItem('auth_token')).toBeNull()
    expect(localStorage.getItem('refresh_token')).toBeNull()
    expect(
      screen.getByRole('heading', { name: /sign in/i, level: 2 }),
    ).toBeInTheDocument()
  })

  it('renders not found page for unknown route', () => {
    renderWithProviders(<App />, {
      route: '/unknown-route',
      isAuthenticated: false,
    })

    expect(
      screen.getByRole('heading', { name: /page not found/i, level: 1 }),
    ).toBeInTheDocument()
  })
})
