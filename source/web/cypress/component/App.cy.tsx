import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'

import App from '../../src/app/App'
import { ThemeModeProvider } from '../../src/shared/lib/theme-mode'
import { createAuthTokens } from '../support/auth-helpers'

function mountApp(initialEntries: string[] = ['/']) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return cy.mount(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={initialEntries}>
        <ThemeModeProvider>
          <App />
        </ThemeModeProvider>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('<App /> component', () => {
  beforeEach(() => {
    cy.clearLocalStorage()
  })

  it('redirects unauthenticated user to login', () => {
    mountApp(['/'])

    cy.contains('h2', 'Sign in').should('be.visible')
  })

  it('shows home page when authenticated', () => {
    const tokens = createAuthTokens()
    localStorage.setItem('auth_token', tokens.accessToken)
    localStorage.setItem('refresh_token', tokens.refreshToken)

    mountApp(['/'])

    cy.contains('h1', 'Adventure Works').should('be.visible')
  })

  it('logs out and returns to sign in', () => {
    const tokens = createAuthTokens()
    localStorage.setItem('auth_token', tokens.accessToken)
    localStorage.setItem('refresh_token', tokens.refreshToken)

    mountApp(['/'])

    cy.contains('h1', 'Adventure Works').should('be.visible')
    cy.contains('button', 'Log out').click()

    cy.contains('h2', 'Sign in').should('be.visible')
  })
})
