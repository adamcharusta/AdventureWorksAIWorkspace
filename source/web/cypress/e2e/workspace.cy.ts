import { seedAuthSession } from '../support/auth-helpers'

const createdReport = {
  id: 'report-territory-sales',
  title: 'Sales by territory',
  originalPrompt: 'Show sales by territory',
  summary: 'Territory revenue is led by North America.',
  conclusions: 'North America should remain the primary planning focus.',
  status: 'Ready',
  isFavorite: false,
  createdAt: '2026-06-03T12:00:00.000Z',
  updatedAt: '2026-06-03T12:00:00.000Z',
  result: {
    columns: [
      { name: 'Territory', dataType: 'nvarchar' },
      { name: 'Revenue', dataType: 'decimal' },
    ],
    rows: [
      ['North America', 42000],
      ['Europe', 21000],
    ],
    rowCount: 2,
    truncated: false,
    elapsedMilliseconds: 34,
  },
  charts: [
    {
      kind: 'Bar',
      title: 'Revenue by territory',
      categoryColumn: 'Territory',
      series: [{ column: 'Revenue', label: 'Revenue' }],
      description: 'Revenue grouped by sales territory.',
    },
  ],
  sections: [],
  messages: [
    {
      id: 'message-user',
      role: 'User',
      content: 'Show sales by territory',
      sortOrder: 1,
      relatedSqlQueryId: 'query-1',
      createdAt: '2026-06-03T12:00:00.000Z',
    },
    {
      id: 'message-assistant',
      role: 'Assistant',
      content: 'Territory revenue is led by North America.',
      sortOrder: 2,
      relatedSqlQueryId: 'query-1',
      createdAt: '2026-06-03T12:00:01.000Z',
    },
  ],
  generatedSqlQueries: [
    {
      id: 'query-1',
      sourceMessageId: 'message-user',
      userPrompt: 'Show sales by territory',
      sqlText: 'SELECT Territory, Revenue FROM Sales.vTerritoryRevenue',
      explanation: 'Aggregates revenue by territory.',
      validationStatus: 'Valid',
      validationMessage: null,
      executionStatus: 'Executed',
      executionMessage: null,
      inputTokens: 100,
      outputTokens: 50,
      resultRowCount: 2,
      resultColumnCount: 2,
      durationMs: 34,
      createdAt: '2026-06-03T12:00:00.000Z',
    },
  ],
}

describe('Workspace E2E', () => {
  beforeEach(() => {
    cy.clearLocalStorage()
  })

  it('creates a report from the AI chat and renders the generated dashboard', () => {
    let reports: unknown[] = []

    cy.intercept('GET', '**/api/reports', (request) => {
      request.reply({ statusCode: 200, body: { reports } })
    }).as('getReports')
    cy.intercept('GET', `**/api/reports/${createdReport.id}`, (request) => {
      request.reply({ statusCode: 200, body: createdReport })
    }).as('getReportDetails')
    cy.intercept('POST', '**/api/reports', (request) => {
      expect(request.body).to.deep.equal({
        message: 'Show sales by territory',
      })

      reports = [
        {
          id: createdReport.id,
          title: createdReport.title,
          status: createdReport.status,
          isFavorite: createdReport.isFavorite,
          createdAt: createdReport.createdAt,
          updatedAt: createdReport.updatedAt,
        },
      ]

      request.reply({
        statusCode: 200,
        body: {
          report: createdReport,
          userMessage: createdReport.messages[0],
          assistantMessage: createdReport.messages[1],
          sqlQuery: createdReport.generatedSqlQueries[0],
          outcome: 'Executed',
          message: createdReport.summary,
          result: createdReport.result,
          charts: createdReport.charts,
        },
      })
    }).as('createReport')

    cy.visit('/', {
      onBeforeLoad: (win) => {
        seedAuthSession(win, {
          name: 'Casey Analyst',
          role: 'User',
          sub: 'cypress-user',
        })
      },
    })

    cy.wait('@getReports')
    cy.contains('No saved reports yet').should('be.visible')

    cy.get('[aria-label="Chat message"]').type('Show sales by territory')
    cy.get('[aria-label="Send"]').click()

    cy.wait('@createReport')
    cy.location('pathname').should('eq', `/raport/${createdReport.id}`)
    cy.contains('h1', 'Sales by territory').should('be.visible')
    cy.contains('Territory revenue is led by North America.').should(
      'be.visible',
    )
    cy.contains('Revenue by territory').should('be.visible')
    cy.contains('North America').should('be.visible')
  })
})
