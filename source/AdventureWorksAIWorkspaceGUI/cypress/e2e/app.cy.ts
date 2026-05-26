describe('App E2E', () => {
  it('renders the temperature trend chart from stubbed forecasts', () => {
    cy.intercept('GET', '/api/weather-forecasts?days=7', {
      fixture: 'weather-forecasts.json',
    }).as('getForecasts')

    cy.visit('/')

    cy.wait('@getForecasts')
    cy.contains('h1', 'Weather forecasts').should('be.visible')
    cy.contains('h6', 'Temperature trend').should('be.visible')
  })

  it('shows an error alert when the API returns 500', () => {
    cy.intercept('GET', '/api/weather-forecasts?days=7', {
      statusCode: 500,
      body: { message: 'boom' },
    }).as('getForecastsError')

    cy.visit('/')

    cy.wait('@getForecastsError')
    cy.get('[role="alert"]').should('contain.text', 'Failed to load forecasts')
  })
})
