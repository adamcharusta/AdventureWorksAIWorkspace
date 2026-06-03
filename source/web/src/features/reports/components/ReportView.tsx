import Box from '@mui/material/Box'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

import { isTableChart } from '@/features/reports/lib/chart-kind'
import type {
  ChartSpec,
  TabularResult,
} from '@/features/reports/lib/report-types'

import { ReportChart } from './ReportChart'
import { ReportConclusions } from './ReportConclusions'
import { ReportInsights } from './ReportInsights'
import { ReportResultTable } from './ReportResultTable'

export type ReportViewSection = {
  id: string
  question: string
  title: string
  insights: string
  conclusions?: string | null
  charts: ChartSpec[]
  result: TabularResult | null
}

export type ReportViewData = {
  question: string
  insights: string
  conclusions?: string | null
  charts: ChartSpec[]
  result: TabularResult | null
  sections?: ReportViewSection[]
}

type ReportViewProps = {
  report: ReportViewData
}

export function ReportView({ report }: ReportViewProps) {
  const sections =
    report.sections && report.sections.length > 0
      ? report.sections
      : [
          {
            id: 'primary-section',
            question: report.question,
            title: report.question,
            insights: report.insights,
            conclusions: report.conclusions,
            charts: report.charts,
            result: report.result,
          },
        ]
  const hasMultipleSections = sections.length > 1

  return (
    <Stack spacing={2.5}>
      {sections.map((section, sectionIndex) => (
        <ReportSection
          key={section.id}
          section={section}
          showDivider={hasMultipleSections && sectionIndex > 0}
          showPrompt={hasMultipleSections}
        />
      ))}
    </Stack>
  )
}

type ReportSectionProps = {
  section: ReportViewSection
  showDivider: boolean
  showPrompt: boolean
}

function ReportSection({
  section,
  showDivider,
  showPrompt,
}: ReportSectionProps) {
  const { charts, conclusions, insights, question, result, title } = section
  const hasTableChart = charts.some(isTableChart)

  return (
    <Stack
      spacing={2}
      sx={{
        borderTopColor: 'divider',
        borderTopStyle: showDivider ? 'solid' : 'none',
        borderTopWidth: showDivider ? 1 : 0,
        pt: showDivider ? 2.5 : 0,
      }}
    >
      <Box>
        <Typography sx={{ fontWeight: 700 }} variant="h6">
          {title}
        </Typography>
        {showPrompt && question !== title ? (
          <Typography color="text.secondary" variant="body2">
            {question}
          </Typography>
        ) : null}
      </Box>

      <ReportInsights insights={insights} />

      {conclusions ? <ReportConclusions conclusions={conclusions} /> : null}

      {result && charts.length > 0 ? (
        <Stack spacing={2}>
          {charts.map((spec, index) => (
            <ReportChart
              key={`${section.id}-${spec.title}-${index}`}
              spec={spec}
              result={result}
            />
          ))}
        </Stack>
      ) : null}

      {result && !hasTableChart ? (
        <Box>
          <Typography color="text.secondary" sx={{ mb: 1 }} variant="overline">
            Data ({result.rowCount} rows)
          </Typography>
          <ReportResultTable result={result} />
        </Box>
      ) : null}
    </Stack>
  )
}
