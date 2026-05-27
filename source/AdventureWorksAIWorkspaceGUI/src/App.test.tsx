import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'

import App from './App'
import { renderWithProviders } from './test/render-utils'

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
