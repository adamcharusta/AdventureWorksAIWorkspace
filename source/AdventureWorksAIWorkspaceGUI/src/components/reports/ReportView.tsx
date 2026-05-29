import Box from '@mui/material/Box'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

import type { ChartSpec, TabularResult } from '@/lib/report-types'

import { ReportChart } from './ReportChart'
import { ReportInsights } from './ReportInsights'
import { ReportResultTable } from './ReportResultTable'

export type ReportViewData = {
  question: string
  insights: string
  charts: ChartSpec[]
  result: TabularResult | null
}

type ReportViewProps = {
  report: ReportViewData
}

export function ReportView({ report }: ReportViewProps) {
  const { question, insights, charts, result } = report

  return (
    <Stack spacing={2.5}>
      <Typography sx={{ fontWeight: 700 }} variant="h6">
        {question}
      </Typography>

      <ReportInsights insights={insights} />

      {result && charts.length > 0 ? (
        <Stack spacing={2}>
          {charts.map((spec, index) => (
            <ReportChart
              key={`${spec.title}-${index}`}
              spec={spec}
              result={result}
            />
          ))}
        </Stack>
      ) : null}

      {result ? (
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
