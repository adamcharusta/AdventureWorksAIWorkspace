import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, type RenderResult } from '@testing-library/react'
import type { ReactElement, ReactNode } from 'react'
import { type InitialEntry, MemoryRouter } from 'react-router-dom'
import { Toaster } from 'sonner'

import { ThemeModeProvider } from '@/shared/lib/theme-mode'

import { setAuthenticatedSession } from './factories'

type RenderWithProvidersOptions = {
  authClaims?: Record<string, unknown>
  route?: string
  initialEntries?: InitialEntry[]
  isAuthenticated?: boolean
}

export function renderWithProviders(
  ui: ReactElement,
  {
    authClaims,
    route = '/',
    initialEntries,
    isAuthenticated = false,
  }: RenderWithProvidersOptions = {},
): RenderResult {
  if (isAuthenticated) {
    setAuthenticatedSession(authClaims)
  }

  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  })

  const entries = initialEntries ?? [route]

  return render(
    <QueryClientProvider client={queryClient}>
      <ThemeModeProvider>
        <MemoryRouter initialEntries={entries}>{ui}</MemoryRouter>
        <Toaster />
      </ThemeModeProvider>
    </QueryClientProvider>,
  )
}

type RenderRoutesOptions = RenderWithProvidersOptions

export function renderRoutes(
  routes: ReactNode,
  options?: RenderRoutesOptions,
): RenderResult {
  return renderWithProviders(<>{routes}</>, options)
}
