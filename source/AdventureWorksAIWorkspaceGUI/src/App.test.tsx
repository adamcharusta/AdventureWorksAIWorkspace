import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import {
  render,
  screen,
  waitForElementToBeRemoved,
} from '@testing-library/react'
import { http, HttpResponse } from 'msw'
import type { ReactNode } from 'react'
import { describe, expect, it } from 'vitest'

import type { WeatherForecastDto } from './api/generated/model'
import { getGetWeatherForecastsMockHandler } from './api/generated/weather-forecasts/weather-forecasts.msw'
import App from './App'
import { server } from './test/server'

const forecasts: WeatherForecastDto[] = [
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
  it('shows a loading indicator while forecasts are being fetched', () => {
    server.use(http.get('*/api/weather-forecasts', () => new Promise(() => {})))

    renderApp()

    expect(screen.getByRole('progressbar')).toBeInTheDocument()
    expect(screen.queryByText('Temperature trend')).not.toBeInTheDocument()
  })

  it('renders the temperature trend card after forecasts arrive', async () => {
    server.use(getGetWeatherForecastsMockHandler(forecasts))

    renderApp()

    await waitForElementToBeRemoved(() => screen.queryByRole('progressbar'))

    expect(
      screen.getByRole('heading', { name: /weather forecasts/i, level: 1 }),
    ).toBeInTheDocument()
    expect(
      screen.getByRole('heading', { name: /temperature trend/i, level: 6 }),
    ).toBeInTheDocument()
  })

  it('shows an error alert when the forecasts request fails', async () => {
    server.use(
      http.get(
        '*/api/weather-forecasts',
        () => new HttpResponse(null, { status: 500 }),
      ),
    )

    renderApp()

    await waitForElementToBeRemoved(() => screen.queryByRole('progressbar'))

    const alert = await screen.findByRole('alert')
    expect(alert).toHaveTextContent(/Failed to load forecasts/i)
  })
})
