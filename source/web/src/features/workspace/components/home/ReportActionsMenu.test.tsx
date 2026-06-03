import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { toast } from '@/shared/lib/toast'
import { renderWithProviders } from '@/test/render-utils'

import { ReportActionsMenu } from './ReportActionsMenu'

vi.mock('@/shared/lib/toast', () => ({
  toast: {
    error: vi.fn(),
    success: vi.fn(),
  },
}))

describe('<ReportActionsMenu />', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('calls rename, copy SQL, and delete actions from the menu', async () => {
    const user = userEvent.setup()
    const onCopySql = vi.fn()
    const onOpenDeleteDialog = vi.fn()
    const onStartRename = vi.fn()

    renderWithProviders(
      <ReportActionsMenu
        activeReportId="report-1"
        hasActiveReport
        hasSqlQueries
        onCopySql={onCopySql}
        onOpenDeleteDialog={onOpenDeleteDialog}
        onStartRename={onStartRename}
      />,
    )

    await openMenu(user)
    await user.click(screen.getByRole('menuitem', { name: /rename report/i }))
    expect(onStartRename).toHaveBeenCalledTimes(1)

    await openMenu(user)
    await user.click(
      screen.getByRole('menuitem', { name: /copy sql queries/i }),
    )
    expect(onCopySql).toHaveBeenCalledTimes(1)

    await openMenu(user)
    await user.click(screen.getByRole('menuitem', { name: /delete report/i }))
    expect(onOpenDeleteDialog).toHaveBeenCalledTimes(1)
  })

  it('copies the active report id to the clipboard', async () => {
    const user = userEvent.setup()
    const writeText = stubClipboard()

    renderWithProviders(
      <ReportActionsMenu
        activeReportId="report-1"
        hasActiveReport
        hasSqlQueries={false}
        onCopySql={() => undefined}
        onOpenDeleteDialog={() => undefined}
        onStartRename={() => undefined}
      />,
    )

    await openMenu(user)
    await user.click(screen.getByRole('menuitem', { name: /copy report id/i }))

    expect(writeText).toHaveBeenCalledWith('report-1')
    expect(toast.success).toHaveBeenCalledWith(
      'Report ID copied to clipboard.',
      'Reports',
    )
  })

  it('shows an error toast when copying the report id fails', async () => {
    const user = userEvent.setup()
    stubClipboard(vi.fn().mockRejectedValue(new Error('Clipboard denied')))

    renderWithProviders(
      <ReportActionsMenu
        activeReportId="report-1"
        hasActiveReport
        hasSqlQueries={false}
        onCopySql={() => undefined}
        onOpenDeleteDialog={() => undefined}
        onStartRename={() => undefined}
      />,
    )

    await openMenu(user)
    await user.click(screen.getByRole('menuitem', { name: /copy report id/i }))

    expect(toast.error).toHaveBeenCalledWith(
      'Could not copy the report ID to the clipboard.',
      'Reports',
    )
  })

  it('disables report-specific actions when no report is active', async () => {
    const user = userEvent.setup()

    renderWithProviders(
      <ReportActionsMenu
        activeReportId={null}
        hasActiveReport={false}
        hasSqlQueries={false}
        onCopySql={() => undefined}
        onOpenDeleteDialog={() => undefined}
        onStartRename={() => undefined}
      />,
    )

    await openMenu(user)

    expect(
      screen.getByRole('menuitem', { name: /rename report/i }),
    ).toHaveAttribute('aria-disabled', 'true')
    expect(
      screen.getByRole('menuitem', { name: /copy sql queries/i }),
    ).toHaveAttribute('aria-disabled', 'true')
    expect(
      screen.getByRole('menuitem', { name: /copy report id/i }),
    ).toHaveAttribute('aria-disabled', 'true')
    expect(
      screen.getByRole('menuitem', { name: /delete report/i }),
    ).toHaveAttribute('aria-disabled', 'true')
  })
})

async function openMenu(user: ReturnType<typeof userEvent.setup>) {
  await user.click(screen.getByRole('button', { name: /report actions/i }))
}

function stubClipboard(writeText = vi.fn().mockResolvedValue(undefined)) {
  Object.defineProperty(navigator, 'clipboard', {
    configurable: true,
    value: { writeText },
  })

  return writeText
}
