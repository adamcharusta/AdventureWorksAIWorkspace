import { createAuthTokens } from '../support/auth-helpers'

describe('App E2E', () => {
  beforeEach(() => {
    cy.clearLocalStorage()
  })

  it('redirects unauthenticated user from home to login page', () => {
    cy.visit('/')

    cy.location('pathname').should('eq', '/login')
    cy.contains('h2', 'Sign in').should('be.visible')
  })

  it('signs in and logs out', () => {
    const tokens = createAuthTokens()

    cy.intercept('POST', '**/api/auth/login', {
      statusCode: 200,
      body: tokens,
    }).as('login')

    cy.visit('/login')
    cy.get('input[autocomplete="username"]').type('user@example.com')
    cy.get('input[autocomplete="current-password"]').type('secret')
    cy.contains('button', 'Sign in').click()

    cy.wait('@login')
    cy.location('pathname').should('eq', '/')
    cy.contains('h1', 'Adventure Works').should('be.visible')
    cy.contains('button', 'Log out').click()

    cy.location('pathname').should('eq', '/login')
    cy.contains('h2', 'Sign in').should('be.visible')
  })

  it('redirects to set-first-password after 403 and completes first login', () => {
    const tokens = createAuthTokens()

    cy.intercept('POST', '**/api/auth/login', {
      statusCode: 403,
      body: {},
    }).as('loginFirstPassword')

    cy.intercept('POST', '**/api/auth/set-first-password', {
      statusCode: 200,
      body: tokens,
    }).as('setFirstPassword')

    cy.visit('/login')
    cy.get('input[autocomplete="username"]').type('user@example.com')
    cy.get('input[autocomplete="current-password"]').type('secret')
    cy.contains('button', 'Sign in').click()

    cy.wait('@loginFirstPassword')
    cy.location('pathname').should('eq', '/set-first-password')
    cy.get('input[autocomplete="new-password"]').eq(0).type('NewSecret123!')
    cy.get('input[autocomplete="new-password"]').eq(1).type('NewSecret123!')
    cy.contains('button', 'Save password').click()

    cy.wait('@setFirstPassword')
    cy.location('pathname').should('eq', '/')
    cy.contains('h1', 'Adventure Works').should('be.visible')
  })
})
