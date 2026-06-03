import type { PaletteMode } from '@mui/material/styles'

import type { ReportSummaryDto } from '@/api/generated/model'
import { MenuDrawer } from '@/features/workspace/components/drawers/MenuDrawer'

import { buildHomeMenuItems } from './home-menu-items'

type HomeMenuDrawerProps = {
  activeReportId: string | null
  isAdmin: boolean
  isNewReportSelected: boolean
  isReportsError: boolean
  isReportsLoading: boolean
  mode: PaletteMode
  onLogout: () => void
  onNewReport: () => void
  onOpenAdminPanel: () => void
  onRefreshReports: () => void
  onSelectReport: (reportId: string) => void
  onToggle: () => void
  onToggleTheme: () => void
  open: boolean
  reports: ReportSummaryDto[]
  username?: string | null
}

export function HomeMenuDrawer({
  activeReportId,
  isAdmin,
  isNewReportSelected,
  isReportsError,
  isReportsLoading,
  mode,
  onLogout,
  onNewReport,
  onOpenAdminPanel,
  onRefreshReports,
  onSelectReport,
  onToggle,
  onToggleTheme,
  open,
  reports,
  username,
}: HomeMenuDrawerProps) {
  const { bottomItems, reportItems, topItems } = buildHomeMenuItems({
    activeReportId,
    isAdmin,
    isNewReportSelected,
    isReportsError,
    isReportsLoading,
    mode,
    onLogout,
    onNewReport,
    onOpenAdminPanel,
    onRefreshReports,
    onSelectReport,
    onToggleTheme,
    reports,
  })

  return (
    <MenuDrawer
      bottomItems={bottomItems}
      items={reportItems}
      open={open}
      onToggle={onToggle}
      title={`Hello ${username ?? 'there'}!`}
      subtitle="Saved reports"
      topItems={topItems}
    />
  )
}
