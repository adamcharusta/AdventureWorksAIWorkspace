import { ReportView } from '../../src/features/reports/components/ReportView'
import { SAMPLE_REPORT } from '../../src/features/reports/components/sample-report'

describe('<ReportView /> component', () => {
  it('renders report insights, charts, and result table', () => {
    cy.mount(<ReportView report={SAMPLE_REPORT} />)

    cy.contains('Top product categories by sales').should('be.visible')
    cy.contains('Bikes are by far the strongest category').should('be.visible')
    cy.contains('Sales by category').should('be.visible')
    cy.contains('Category share').should('be.visible')
    cy.contains('[role="columnheader"]', 'Category').should('be.visible')
    cy.contains('[role="columnheader"]', 'Sales').should('be.visible')
    cy.contains('td', 'Bikes').should('be.visible')
  })

  it('renders appended report sections as separate analysis blocks', () => {
    cy.mount(
      <ReportView
        report={{
          question: 'Sales analysis',
          insights: 'Latest summary',
          charts: [],
          result: null,
          sections: [
            {
              id: 'section-1',
              title: 'Initial sales overview',
              question: 'Analyze sales',
              insights: 'The original section remains visible.',
              charts: [],
              result: null,
            },
            {
              id: 'section-2',
              title: 'Added customer segment',
              question: 'Add customer data',
              insights: 'The follow-up section is appended.',
              charts: [],
              result: null,
            },
          ],
        }}
      />,
    )

    cy.contains('Initial sales overview').should('be.visible')
    cy.contains('Added customer segment').should('be.visible')
    cy.contains('The original section remains visible.').should('be.visible')
    cy.contains('The follow-up section is appended.').should('be.visible')
  })
})
