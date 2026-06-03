import Box from '@mui/material/Box'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import { BarChart } from '@mui/x-charts/BarChart'
import { LineChart } from '@mui/x-charts/LineChart'
import { PieChart } from '@mui/x-charts/PieChart'

import {
  buildCartesianData,
  buildPieData,
} from '@/features/reports/lib/chart-data'
import { getChartKind } from '@/features/reports/lib/chart-kind'
import type {
  ChartSpec,
  TabularResult,
} from '@/features/reports/lib/report-types'

import { ReportResultTable } from './ReportResultTable'

const CHART_HEIGHT = 320

type ReportChartProps = {
  spec: ChartSpec
  result: TabularResult
}

function renderChart(spec: ChartSpec, result: TabularResult) {
  const kind = getChartKind(spec)

  if (kind === 'table') {
    return <ReportResultTable result={result} />
  }

  if (kind === 'pie') {
    const data = buildPieData(spec, result)
    if (data.length === 0) {
      return null
    }

    return <PieChart height={CHART_HEIGHT} series={[{ data }]} />
  }

  if (kind === 'bar' || kind === 'line' || kind === 'area') {
    const { categories, series } = buildCartesianData(spec, result)
    if (categories.length === 0 || series.length === 0) {
      return null
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

  return null
}

export function ReportChart({ spec, result }: ReportChartProps) {
  const chart = renderChart(spec, result)

  if (!chart) {
    return null
  }

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
      <Box sx={{ width: '100%' }}>{chart}</Box>
    </Paper>
  )
}
