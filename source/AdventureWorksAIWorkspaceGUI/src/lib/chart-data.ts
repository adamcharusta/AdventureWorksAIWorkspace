import type { ChartSpec, TabularResult } from './report-types'

/**
 * Joins a {@link ChartSpec} with the rows of a {@link TabularResult} to produce data shaped for
 * MUI X Charts. Column references in the spec are resolved by name (case-insensitive); references
 * that do not exist in the result are ignored.
 */

export interface CartesianSeries {
  label: string
  data: (number | null)[]
}

export interface CartesianChartData {
  categories: string[]
  series: CartesianSeries[]
}

export interface PieDatum {
  id: number
  value: number
  label: string
}

function columnIndex(result: TabularResult, name?: string | null): number {
  if (!name) {
    return -1
  }

  return result.columns.findIndex(
    (column) => column.name.toLowerCase() === name.toLowerCase(),
  )
}

function toNumber(value: unknown): number | null {
  if (value === null || value === undefined || value === '') {
    return null
  }

  const numeric = typeof value === 'number' ? value : Number(value)
  return Number.isFinite(numeric) ? numeric : null
}

export function buildCartesianData(
  spec: ChartSpec,
  result: TabularResult,
): CartesianChartData {
  const categoryIdx = columnIndex(result, spec.categoryColumn)

  const categories = result.rows.map((row, index) =>
    categoryIdx >= 0 ? String(row[categoryIdx] ?? '') : String(index + 1),
  )

  const series = spec.series
    .map((entry) => ({ entry, index: columnIndex(result, entry.column) }))
    .filter(({ index }) => index >= 0)
    .map(({ entry, index }) => ({
      label: entry.label ?? entry.column,
      data: result.rows.map((row) => toNumber(row[index])),
    }))

  return { categories, series }
}

export function buildPieData(
  spec: ChartSpec,
  result: TabularResult,
): PieDatum[] {
  const labelIdx = columnIndex(result, spec.categoryColumn)
  const valueIdx = columnIndex(result, spec.series[0]?.column)

  if (valueIdx < 0) {
    return []
  }

  return result.rows.map((row, index) => ({
    id: index,
    value: toNumber(row[valueIdx]) ?? 0,
    label: labelIdx >= 0 ? String(row[labelIdx] ?? '') : String(index + 1),
  }))
}
