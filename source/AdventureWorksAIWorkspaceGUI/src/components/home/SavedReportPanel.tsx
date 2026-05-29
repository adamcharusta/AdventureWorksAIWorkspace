import Box from '@mui/material/Box'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

import {
  ReportView,
  type ReportViewData,
} from '@/components/reports/ReportView'
import type { ReportDetailsDto } from '@/lib/report-api'

import { formatDateTime } from './home-report-utils'

type SavedReportPanelProps = {
  report: ReportDetailsDto
  reportViewData: ReportViewData | null
}

export function SavedReportPanel({
  report,
  reportViewData,
}: SavedReportPanelProps) {
  return (
    <Stack spacing={2.5}>
      <Typography color="text.secondary" variant="caption">
        Updated {formatDateTime(report.updatedAt)}
      </Typography>

      {reportViewData ? (
        <ReportView report={reportViewData} />
      ) : (
        <ReportTextSection
          title="Original prompt"
          content={report.originalPrompt}
        />
      )}
    </Stack>
  )
}

type ReportTextSectionProps = {
  content: string
  title: string
}

function ReportTextSection({ content, title }: ReportTextSectionProps) {
  return (
    <Box>
      <Typography color="text.secondary" sx={{ mb: 0.75 }} variant="overline">
        {title}
      </Typography>
      <Typography sx={{ whiteSpace: 'pre-wrap' }} variant="body2">
        {content}
      </Typography>
    </Box>
  )
}
