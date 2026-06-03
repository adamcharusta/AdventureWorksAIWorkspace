import { ReportActionsMenu } from '../../src/features/workspace/components/home/ReportActionsMenu'

describe('<ReportActionsMenu /> component', () => {
  it('opens the menu and dispatches report actions', () => {
    const onCopySql = cy.stub().as('copySql')
    const onOpenDeleteDialog = cy.stub().as('openDeleteDialog')
    const onStartRename = cy.stub().as('startRename')

    cy.mount(
      <ReportActionsMenu
        activeReportId="report-1"
        hasActiveReport
        hasSqlQueries
        onCopySql={onCopySql}
        onOpenDeleteDialog={onOpenDeleteDialog}
        onStartRename={onStartRename}
      />,
    )

    cy.get('button[aria-label="Report actions"]').click()
    cy.contains('[role="menuitem"]', 'Rename report').click()
    cy.get('@startRename').should('have.been.calledOnce')

    cy.get('button[aria-label="Report actions"]').click()
    cy.contains('[role="menuitem"]', 'Copy SQL queries').click()
    cy.get('@copySql').should('have.been.calledOnce')

    cy.get('button[aria-label="Report actions"]').click()
    cy.contains('[role="menuitem"]', 'Delete report').click()
    cy.get('@openDeleteDialog').should('have.been.calledOnce')
  })

  it('copies the active report id through the clipboard API', () => {
    cy.window().then((win) => {
      const writeText = cy.stub().as('writeText').resolves()

      Object.defineProperty(win.navigator, 'clipboard', {
        configurable: true,
        value: { writeText },
      })
    })

    cy.mount(
      <ReportActionsMenu
        activeReportId="report-1"
        hasActiveReport
        hasSqlQueries={false}
        onCopySql={() => undefined}
        onOpenDeleteDialog={() => undefined}
        onStartRename={() => undefined}
      />,
    )

    cy.get('button[aria-label="Report actions"]').click()
    cy.contains('[role="menuitem"]', 'Copy report ID').click()

    cy.get('@writeText').should('have.been.calledWith', 'report-1')
  })

  it('disables actions that require an active report', () => {
    cy.mount(
      <ReportActionsMenu
        activeReportId={null}
        hasActiveReport={false}
        hasSqlQueries={false}
        onCopySql={() => undefined}
        onOpenDeleteDialog={() => undefined}
        onStartRename={() => undefined}
      />,
    )

    cy.get('button[aria-label="Report actions"]').click()

    cy.contains('[role="menuitem"]', 'Rename report').should(
      'have.attr',
      'aria-disabled',
      'true',
    )
    cy.contains('[role="menuitem"]', 'Copy SQL queries').should(
      'have.attr',
      'aria-disabled',
      'true',
    )
    cy.contains('[role="menuitem"]', 'Copy report ID').should(
      'have.attr',
      'aria-disabled',
      'true',
    )
    cy.contains('[role="menuitem"]', 'Delete report').should(
      'have.attr',
      'aria-disabled',
      'true',
    )
  })
})
