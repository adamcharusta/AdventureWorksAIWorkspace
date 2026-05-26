import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

import type { WeatherForecastDto } from '../../src/api/generated/model'
import App from '../../src/App'

function mountApp() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })
  return cy.mount(
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>,
  )
}

describe('<App /> component', () => {
  it('renders the temperature trend card after forecasts arrive', () => {
    cy.fixture<WeatherForecastDto[]>('weather-forecasts.json').then(
      (forecasts) => {
        cy.intercept('GET', '/api/weather-forecasts?days=7', forecasts).as(
          'getForecasts',
        )
      },
    )

    mountApp()

    cy.wait('@getForecasts')
    cy.contains('h1', 'Weather forecasts').should('be.visible')
    cy.contains('h6', 'Temperature trend').should('be.visible')
  })

  it('shows an error alert when the request fails', () => {
    cy.intercept('GET', '/api/weather-forecasts?days=7', {
      forceNetworkError: true,
    }).as('getForecastsError')

    mountApp()

    cy.wait('@getForecastsError')
    cy.get('[role="alert"]').should('contain.text', 'Failed to load forecasts')
  })
})
