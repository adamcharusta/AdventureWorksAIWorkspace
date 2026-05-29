/**
 * Frontend contract types for AI report results and chart specifications.
 *
 * These mirror the backend records (TabularResult, ChartSpec, ReportPresentation).
 * They are intentionally hand-written for now; once the OpenAPI client is regenerated
 * (`npm run api:gen`) with the report endpoints, downstream code should switch to the
 * generated model types and these can be removed or re-exported from them.
 */

export type ChartKind = 'Table' | 'Bar' | 'Line' | 'Pie' | 'Area' | number

export interface ChartSeries {
  column: string
  label?: string | null
}

export interface ChartSpec {
  kind: ChartKind
  title: string
  categoryColumn?: string | null
  series: ChartSeries[]
  description?: string | null
}

export interface TabularColumn {
  name: string
  dataType: string
}

export interface TabularResult {
  columns: TabularColumn[]
  rows: unknown[][]
  rowCount: number
  truncated: boolean
  elapsedMilliseconds: number
}

export interface ReportPresentation {
  insights: string
  charts: ChartSpec[]
}
