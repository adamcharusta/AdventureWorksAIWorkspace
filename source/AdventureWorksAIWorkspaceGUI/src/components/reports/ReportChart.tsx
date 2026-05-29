import Box from '@mui/material/Box'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import { BarChart } from '@mui/x-charts/BarChart'
import { LineChart } from '@mui/x-charts/LineChart'
import { PieChart } from '@mui/x-charts/PieChart'

import { buildCartesianData, buildPieData } from '@/lib/chart-data'
import type { ChartSpec, TabularResult } from '@/lib/report-types'

import { ReportResultTable } from './ReportResultTable'

const CHART_HEIGHT = 320
const chartKindLabels: Record<string, string> = {
  '0': 'table',
  '1': 'bar',
  '2': 'line',
  '3': 'pie',
  '4': 'area',
}

type ReportChartProps = {
  spec: ChartSpec
  result: TabularResult
}

function renderChart(spec: ChartSpec, result: TabularResult) {
  const kind =
    chartKindLabels[String(spec.kind)] ?? String(spec.kind).toLowerCase()

  if (kind === 'pie') {
    const data = buildPieData(spec, result)
    if (data.length === 0) {
      return <ReportResultTable result={result} />
    }

    return <PieChart height={CHART_HEIGHT} series={[{ data }]} />
  }

  if (kind === 'bar' || kind === 'line' || kind === 'area') {
    const { categories, series } = buildCartesianData(spec, result)
    if (categories.length === 0 || series.length === 0) {
      return <ReportResultTable result={result} />
    }

    if (kind === 'bar') {
      return (
        <BarChart
          height={CHART_HEIGHT}
          xAxis={[{ scaleType: 'band', data: categories }]}
          series={series.map((entry) => ({
            data: entry.data,
            label: entry.label,
          }))}
        />
      )
    }

    return (
      <LineChart
        height={CHART_HEIGHT}
        xAxis={[{ scaleType: 'point', data: categories }]}
        series={series.map((entry) => ({
          data: entry.data,
          label: entry.label,
          area: kind === 'area',
        }))}
      />
    )
  }

  return <ReportResultTable result={result} />
}

export function ReportChart({ spec, result }: ReportChartProps) {
  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Typography sx={{ fontWeight: 600 }} variant="subtitle1">
        {spec.title}
      </Typography>
      {spec.description ? (
        <Typography color="text.secondary" sx={{ mb: 1 }} variant="body2">
          {spec.description}
        </Typography>
      ) : null}
      <Box sx={{ width: '100%' }}>{renderChart(spec, result)}</Box>
    </Paper>
  )
}
