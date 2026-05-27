import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { Route, Routes } from 'react-router-dom'
import { describe, expect, it } from 'vitest'

import { createLoginResponse } from '../test/factories'
import { renderWithProviders } from '../test/render-utils'
import { server } from '../test/server'
import { LoginPage } from './LoginPage'

function renderLoginPage({
  isAuthenticated = false,
  initialEntries,
}: {
  isAuthenticated?: boolean
  initialEntries?: Array<string | { pathname: string; state?: unknown }>
} = {}) {
  return renderWithProviders(
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/" element={<div>Home target</div>} />
      <Route path="/dashboard" element={<div>Dashboard target</div>} />
      <Route path="/set-first-password" element={<div>Set first target</div>} />
    </Routes>,
    {
      route: '/login',
      isAuthenticated,
      initialEntries,
    },
  )
}

describe('<LoginPage />', () => {
  it('redirects authenticated users to home', () => {
    renderLoginPage({ isAuthenticated: true })

    expect(screen.getByText('Home target')).toBeInTheDocument()
  })

  it('shows error for invalid credentials', async () => {
    const user = userEvent.setup()

    server.use(
      http.post(
        '*/api/auth/login',
        () => new HttpResponse(null, { status: 401 }),
      ),
    )

    renderLoginPage()

    await user.type(screen.getByLabelText(/username or email/i), 'user')
    await user.type(
      screen.getByLabelText(/password/i, { selector: 'input' }),
      'wrong',
    )
    await user.click(screen.getByRole('button', { name: /sign in/i }))

    expect(
      await screen.findByText(/invalid credentials\. please try again\./i),
    ).toBeInTheDocument()
  })

  it('redirects to first-password page when login returns 403', async () => {
    const user = userEvent.setup()

    server.use(
      http.post(
        '*/api/auth/login',
        () => new HttpResponse(null, { status: 403 }),
      ),
    )

    renderLoginPage()

    await user.type(screen.getByLabelText(/username or email/i), 'user')
    await user.type(
      screen.getByLabelText(/password/i, { selector: 'input' }),
      'secret',
    )
    await user.click(screen.getByRole('button', { name: /sign in/i }))

    expect(await screen.findByText('Set first target')).toBeInTheDocument()
  })

  it('stores tokens and navigates after successful login', async () => {
    const user = userEvent.setup()

    server.use(
      http.post('*/api/auth/login', () =>
        HttpResponse.json(createLoginResponse(), { status: 200 }),
      ),
    )

    renderLoginPage()

    await user.type(screen.getByLabelText(/username or email/i), 'user')
    await user.type(
      screen.getByLabelText(/password/i, { selector: 'input' }),
      'secret',
    )
    await user.click(screen.getByRole('button', { name: /sign in/i }))

    expect(await screen.findByText('Home target')).toBeInTheDocument()
    expect(localStorage.getItem('auth_token')).toBeTruthy()
    expect(localStorage.getItem('refresh_token')).toBeTruthy()
  })
})
