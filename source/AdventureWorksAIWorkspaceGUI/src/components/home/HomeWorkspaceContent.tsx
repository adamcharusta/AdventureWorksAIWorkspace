import AutoAwesomeRoundedIcon from '@mui/icons-material/AutoAwesomeRounded'
import ErrorOutlineRoundedIcon from '@mui/icons-material/ErrorOutlineRounded'
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded'
import Button from '@mui/material/Button'
import CircularProgress from '@mui/material/CircularProgress'

import type { ReportDetailsDto } from '@/api/generated/model'
import type { ReportViewData } from '@/components/reports/ReportView'

import { HomeWorkspaceState } from './HomeWorkspaceState'
import { SavedReportPanel } from './SavedReportPanel'

type HomeWorkspaceContentProps = {
  activeReportId: string | null
  isReportDetailsError: boolean
  isReportDetailsLoading: boolean
  isReportsError: boolean
  isReportsLoading: boolean
  onRefreshActiveReport: () => void
  onRefreshReports: () => void
  reportViewData: ReportViewData | null
  reportsCount: number
  selectedReport: ReportDetailsDto | null
}

export function HomeWorkspaceContent({
  activeReportId,
  isReportDetailsError,
  isReportDetailsLoading,
  isReportsError,
  isReportsLoading,
  onRefreshActiveReport,
  onRefreshReports,
  reportViewData,
  reportsCount,
  selectedReport,
}: HomeWorkspaceContentProps) {
  if (isReportsLoading) {
    return (
      <HomeWorkspaceState
        icon={<CircularProgress size={34} />}
        title="Loading reports"
      />
    )
  }

  if (isReportsError) {
    return (
      <HomeWorkspaceState
        icon={<ErrorOutlineRoundedIcon color="error" fontSize="large" />}
        title="Reports unavailable"
        description="Saved reports could not be loaded."
        action={<RetryButton onClick={onRefreshReports} />}
      />
    )
  }

  if (!activeReportId) {
    return (
      <HomeWorkspaceState
        icon={<AutoAwesomeRoundedIcon color="primary" fontSize="large" />}
        title={reportsCount === 0 ? 'No saved reports yet' : 'New report'}
        description={
          reportsCount === 0
            ? 'Generated reports will appear in the sidebar.'
            : 'Use the AI chat to create the next analysis.'
        }
      />
    )
  }

  if (isReportDetailsLoading) {
    return (
      <HomeWorkspaceState
        icon={<CircularProgress size={34} />}
        title="Loading report"
      />
    )
  }

  if (isReportDetailsError || !selectedReport) {
    return (
      <HomeWorkspaceState
        icon={<ErrorOutlineRoundedIcon color="error" fontSize="large" />}
        title="Report unavailable"
        description="The selected report could not be loaded."
        action={<RetryButton onClick={onRefreshActiveReport} />}
      />
    )
  }

  return (
    <SavedReportPanel report={selectedReport} reportViewData={reportViewData} />
  )
}

function RetryButton({ onClick }: { onClick: () => void }) {
  return (
    <Button
      startIcon={<RefreshRoundedIcon />}
      variant="outlined"
      onClick={onClick}
    >
      Retry
    </Button>
  )
}
