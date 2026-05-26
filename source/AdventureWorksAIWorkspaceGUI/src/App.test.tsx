import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import {
  render,
  screen,
  waitForElementToBeRemoved,
} from '@testing-library/react'
import type { ReactNode } from 'react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { apiClient } from './api/client'
import App from './App'
import type { WeatherForecast } from './hooks/useWeatherForecasts'

vi.mock('./api/client', () => ({
  apiClient: {
    GET: vi.fn(),
  },
}))

const mockedGet = vi.mocked(apiClient.GET)

const forecasts: WeatherForecast[] = [
  {
    date: '2026-05-26',
    temperatureC: -2,
    temperatureF: 28,
    summary: 'Freezing',
  },
  { date: '2026-05-27', temperatureC: 12, temperatureF: 53, summary: 'Mild' },
  { date: '2026-05-28', temperatureC: 24, temperatureF: 75, summary: 'Hot' },
]

function renderApp(ui: ReactNode = <App />) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  return render(
    <QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>,
  )
}

describe('<App />', () => {
  beforeEach(() => {
    mockedGet.mockReset()
  })

  it('shows a loading indicator while forecasts are being fetched', () => {
    mockedGet.mockReturnValue(new Promise(() => {}))

    renderApp()

    expect(screen.getByRole('progressbar')).toBeInTheDocument()
    expect(screen.queryByText('Temperature trend')).not.toBeInTheDocument()
  })

  it('renders the temperature trend card after forecasts arrive', async () => {
    mockedGet.mockResolvedValue({
      data: forecasts as never,
      error: undefined,
      response: new Response(),
    })

    renderApp()

    await waitForElementToBeRemoved(() => screen.queryByRole('progressbar'))

    expect(
      screen.getByRole('heading', { name: /weather forecasts/i, level: 1 }),
    ).toBeInTheDocument()
    expect(
      screen.getByRole('heading', { name: /temperature trend/i, level: 6 }),
    ).toBeInTheDocument()
    expect(mockedGet).toHaveBeenCalledWith('/api/weather-forecasts', {
      params: { query: { days: 7 } },
    })
  })

  it('shows an error alert when the forecasts request fails', async () => {
    mockedGet.mockRejectedValue(new Error('Network down'))

    renderApp()

    await waitForElementToBeRemoved(() => screen.queryByRole('progressbar'))

    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent('Failed to load forecasts: Network down')
  })
})
