import ArticleRoundedIcon from '@mui/icons-material/ArticleRounded'
import ChatBubbleOutlineRoundedIcon from '@mui/icons-material/ChatBubbleOutlineRounded'
import ErrorOutlineRoundedIcon from '@mui/icons-material/ErrorOutlineRounded'
import CircularProgress from '@mui/material/CircularProgress'
import { createElement } from 'react'

import type { ReportSummaryDto } from '@/api/generated/model'
import type { MenuDrawerItem } from '@/features/workspace/components/drawers/MenuDrawer'

type BuildReportMenuItemsInput = {
  activeReportId: string | null
  isReportsError: boolean
  isReportsLoading: boolean
  onSelectReport: (reportId: string) => void
  reports: ReportSummaryDto[]
}

export function buildReportMenuItems({
  activeReportId,
  isReportsError,
  isReportsLoading,
  onSelectReport,
  reports,
}: BuildReportMenuItemsInput): MenuDrawerItem[] {
  if (reports.length > 0) {
    return reports.map((item) => ({
      label: item.title,
      icon: createElement(ChatBubbleOutlineRoundedIcon),
      selected: item.id === activeReportId,
      onClick: () => onSelectReport(item.id),
    }))
  }

  return [
    {
      label: isReportsLoading
        ? 'Loading reports'
        : isReportsError
          ? 'Reports unavailable'
          : 'No reports yet',
      icon: isReportsLoading
        ? createElement(CircularProgress, { color: 'inherit', size: 18 })
        : createElement(
            isReportsError ? ErrorOutlineRoundedIcon : ArticleRoundedIcon,
          ),
      disabled: true,
    },
  ]
}
