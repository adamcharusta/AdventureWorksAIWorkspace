import AddRoundedIcon from '@mui/icons-material/AddRounded'
import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings'
import DarkModeRoundedIcon from '@mui/icons-material/DarkModeRounded'
import LightModeRoundedIcon from '@mui/icons-material/LightModeRounded'
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded'
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded'
import type { PaletteMode } from '@mui/material/styles'
import { createElement } from 'react'

import type { ReportSummaryDto } from '@/api/generated/model'

import { buildReportMenuItems } from './home-report-menu-items'

type BuildHomeMenuItemsInput = {
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
  onToggleTheme: () => void
  reports: ReportSummaryDto[]
}

export function buildHomeMenuItems({
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
}: BuildHomeMenuItemsInput) {
  const reportItems = buildReportMenuItems({
    activeReportId,
    isReportsError,
    isReportsLoading,
    onSelectReport,
    reports,
  })
  const topItems = [
    {
      label: 'New report',
      icon: createElement(AddRoundedIcon),
      onClick: onNewReport,
      selected: isNewReportSelected,
    },
    {
      label: 'Refresh reports',
      icon: createElement(RefreshRoundedIcon),
      onClick: onRefreshReports,
    },
  ]
  const bottomItems = [
    ...(isAdmin
      ? [
          {
            label: 'Admin Panel',
            icon: createElement(AdminPanelSettingsIcon),
            onClick: onOpenAdminPanel,
          },
        ]
      : []),
    {
      label: 'Toggle theme',
      icon: createElement(
        mode === 'dark' ? LightModeRoundedIcon : DarkModeRoundedIcon,
      ),
      onClick: onToggleTheme,
    },
    {
      label: 'Log out',
      icon: createElement(LogoutRoundedIcon),
      onClick: onLogout,
    },
  ]

  return { bottomItems, reportItems, topItems }
}
