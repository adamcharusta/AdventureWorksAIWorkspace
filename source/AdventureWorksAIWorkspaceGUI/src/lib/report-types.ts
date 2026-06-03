/**
 * Frontend contract types for AI report results and chart specifications.
 *
 * These now re-export the Orval-generated model so there is a single source of truth shared with
 * the rest of the API client. `ReportPresentation` is kept here because it is a frontend-only shape
 * that is not part of any API response (and therefore not generated).
 */

export type {
  ChartSeries,
  ChartSpec,
  TabularColumn,
  TabularResult,
} from '@/api/generated/model'
export { ChartKind } from '@/api/generated/model'

import type { ChartSpec } from '@/api/generated/model'

export interface ReportPresentation {
  insights: string
  conclusions?: string | null
  charts: ChartSpec[]
}
