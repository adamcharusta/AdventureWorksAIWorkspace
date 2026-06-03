import type { ChartSpec } from './report-types'

const chartKindLabels: Record<string, string> = {
  '0': 'table',
  '1': 'bar',
  '2': 'line',
  '3': 'pie',
  '4': 'area',
}

/**
 * Normalizes API chart kinds into lowercase render keys.
 *
 * The current API serializes enum values as strings (`"Bar"`), while older snapshots/tests may still
 * contain numeric enum values (`1`). This helper keeps render logic independent from that transport
 * detail.
 */
export function getChartKind(spec: Pick<ChartSpec, 'kind'>): string {
  return chartKindLabels[String(spec.kind)] ?? String(spec.kind).toLowerCase()
}

/**
 * Identifies chart specs that should render as a table instead of as a graphical MUI chart.
 */
export function isTableChart(spec: Pick<ChartSpec, 'kind'>): boolean {
  return getChartKind(spec) === 'table'
}
