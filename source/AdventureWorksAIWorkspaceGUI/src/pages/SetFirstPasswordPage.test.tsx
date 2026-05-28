import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { Route, Routes } from 'react-router-dom'
import { describe, expect, it } from 'vitest'

import { createSetFirstPasswordResponse } from '@/test/factories'
import { renderWithProviders } from '@/test/render-utils'
import { server } from '@/test/server'

import { SetFirstPasswordPage } from './SetFirstPasswordPage'

function renderSetFirstPasswordPage({
  isAuthenticated = false,
  initialEntries,
}: {
  isAuthenticated?: boolean
  initialEntries?: Array<string | { pathname: string; state?: unknown }>
} = {}) {
  return renderWithProviders(
    <Routes>
      <Route path="/set-first-password" element={<SetFirstPasswordPage />} />
      <Route path="/" element={<div>Home target</div>} />
      <Route path="/login" element={<div>Login target</div>} />
      <Route path="/dashboard" element={<div>Dashboard target</div>} />
    </Routes>,
    {
      route: '/set-first-password',
      isAuthenticated,
      initialEntries,
    },
  )
}

const validStateEntry = {
  pathname: '/set-first-password',
  state: {
    identifier: 'user@example.com',
    fromPath: '/dashboard',
  },
}

describe('<SetFirstPasswordPage />', () => {
  it('redirects to login when identifier is missing', () => {
    renderSetFirstPasswordPage()

    expect(screen.getByText('Login target')).toBeInTheDocument()
  })

  it('redirects authenticated users to home', () => {
    renderSetFirstPasswordPage({ isAuthenticated: true })

    expect(screen.getByText('Home target')).toBeInTheDocument()
  })

  it('shows mismatch error when passwords do not match', async () => {
    const user = userEvent.setup()

    renderSetFirstPasswordPage({ initialEntries: [validStateEntry] })

    const [newPasswordInput, confirmNewPasswordInput] =
      screen.getAllByLabelText(/new password/i, { selector: 'input' })

    await user.type(newPasswordInput, 'secret1')
    await user.type(confirmNewPasswordInput, 'secret2')
    await user.click(screen.getByRole('button', { name: /save password/i }))

    expect(
      await screen.findByText(/passwords do not match\./i),
    ).toBeInTheDocument()
  })

  it('shows validation message when API returns 400', async () => {
    const user = userEvent.setup()

    server.use(
      http.post('*/api/auth/set-first-password', () =>
        HttpResponse.json(
          {
            errors: {
              newPassword: ['Password is too weak'],
            },
          },
          { status: 400 },
        ),
      ),
    )

    renderSetFirstPasswordPage({ initialEntries: [validStateEntry] })

    const [newPasswordInput, confirmNewPasswordInput] =
      screen.getAllByLabelText(/new password/i, { selector: 'input' })

    await user.type(newPasswordInput, 'secret')
    await user.type(confirmNewPasswordInput, 'secret')
    await user.click(screen.getByRole('button', { name: /save password/i }))

    expect(await screen.findByText(/password is too weak/i)).toBeInTheDocument()
  })

  it('stores tokens and navigates after success', async () => {
    const user = userEvent.setup()

    server.use(
      http.post('*/api/auth/set-first-password', () =>
        HttpResponse.json(createSetFirstPasswordResponse(), { status: 200 }),
      ),
    )

    renderSetFirstPasswordPage({ initialEntries: [validStateEntry] })

    const [newPasswordInput, confirmNewPasswordInput] =
      screen.getAllByLabelText(/new password/i, { selector: 'input' })

    await user.type(newPasswordInput, 'secret')
    await user.type(confirmNewPasswordInput, 'secret')
    await user.click(screen.getByRole('button', { name: /save password/i }))

    expect(await screen.findByText('Home target')).toBeInTheDocument()
    expect(localStorage.getItem('auth_token')).toBeTruthy()
    expect(localStorage.getItem('refresh_token')).toBeTruthy()
  })
})
