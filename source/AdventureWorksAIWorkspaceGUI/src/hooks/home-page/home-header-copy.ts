import type { ReportDetailsDto, ReportSummaryDto } from '@/api/generated/model'

type HomeHeaderCopyInput = {
  hasActiveReport: boolean
  isNewReportSelected: boolean
  selectedReport: ReportDetailsDto | null
  selectedReportSummary: ReportSummaryDto | null
}

export function getHomeHeaderCopy({
  hasActiveReport,
  isNewReportSelected,
  selectedReport,
  selectedReportSummary,
}: HomeHeaderCopyInput) {
  return {
    description: isNewReportSelected
      ? 'Describe the analysis you want and the AI will generate it.'
      : (selectedReport?.originalPrompt ??
        (hasActiveReport
          ? 'Loading report...'
          : 'Business intelligence workspace')),
    title: isNewReportSelected
      ? 'New report'
      : (selectedReport?.title ??
        selectedReportSummary?.title ??
        'Adventure Works'),
  }
}
