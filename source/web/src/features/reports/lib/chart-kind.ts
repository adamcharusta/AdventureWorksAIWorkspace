import type { ChartSpec } from './report-types'

const chartKindLabels: Record<string, string> = {
  '0': 'table',
  '1': 'bar',
  '2': 'line',
  '3': 'pie',
  '4': 'area',
}

export function getChartKind(spec: Pick<ChartSpec, 'kind'>): string {
  return chartKindLabels[String(spec.kind)] ?? String(spec.kind).toLowerCase()
}

export function isTableChart(spec: Pick<ChartSpec, 'kind'>): boolean {
  return getChartKind(spec) === 'table'
}
