import { seedAuthSession } from '../support/auth-helpers'

describe('Admin Panel E2E', () => {
  beforeEach(() => {
    cy.clearLocalStorage()
  })

  it('redirects non-admin users away from the admin panel', () => {
    cy.intercept('GET', '**/api/reports', {
      statusCode: 200,
      body: { reports: [] },
    }).as('getReports')

    cy.visit('/admin', {
      onBeforeLoad: (win) => {
        seedAuthSession(win, {
          name: 'Casey Analyst',
          role: 'User',
          sub: 'regular-user',
        })
      },
    })

    cy.wait('@getReports')
    cy.location('pathname').should('eq', '/')
    cy.contains('No saved reports yet').should('be.visible')
  })

  it('creates and deletes a user account as an admin', () => {
    const users = [
      {
        id: 'admin-user',
        userName: 'admin',
        email: 'admin@example.com',
        role: 'Admin',
      },
      {
        id: 'analyst-user',
        userName: 'analyst',
        email: 'analyst@example.com',
        role: 'User',
      },
    ]

    cy.intercept('GET', '**/api/users/roles', {
      statusCode: 200,
      body: { roles: ['User', 'Admin'] },
    }).as('getRoles')
    cy.intercept('GET', '**/api/users', (request) => {
      request.reply({ statusCode: 200, body: { users } })
    }).as('getUsers')
    cy.intercept('POST', '**/api/users', (request) => {
      expect(request.body).to.deep.equal({
        userName: 'report_manager',
        email: 'report.manager@example.com',
        role: 'User',
      })

      users.push({
        id: 'report-manager',
        userName: 'report_manager',
        email: 'report.manager@example.com',
        role: 'User',
      })

      request.reply({
        statusCode: 200,
        body: users[users.length - 1],
      })
    }).as('createUser')
    cy.intercept('DELETE', '**/api/users/analyst-user', (request) => {
      users.splice(
        users.findIndex((user) => user.id === 'analyst-user'),
        1,
      )
      request.reply({ statusCode: 204 })
    }).as('deleteUser')

    cy.visit('/admin', {
      onBeforeLoad: (win) => {
        seedAuthSession(win, {
          name: 'Ada Admin',
          role: 'Admin',
          sub: 'admin-user',
        })
      },
    })

    cy.wait(['@getRoles', '@getUsers'])
    cy.contains('h1', 'Admin Panel').should('be.visible')
    cy.contains('analyst@example.com').should('be.visible')

    cy.get('input[autocomplete="username"]').type('report_manager')
    cy.get('input[autocomplete="email"]').type('report.manager@example.com')
    cy.contains('button', 'Create user').click()

    cy.wait('@createUser')
    cy.contains('report.manager@example.com').should('be.visible')

    cy.get('button[aria-label="Delete analyst"]').click()
    cy.contains('Delete analyst?').should('be.visible')
    cy.contains('button', 'Delete user').click()

    cy.wait('@deleteUser')
    cy.contains('analyst@example.com').should('not.exist')
  })
})
