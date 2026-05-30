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

function isBlank(value: unknown): boolean {
  return value === null || value === undefined || String(value).trim() === ''
}

/**
 * Makes category labels unique while keeping them readable. Duplicate labels would collapse into a
 * single band/point on the axis (a band scale keys items by value), which breaks bar/point
 * positioning and axis hover, so repeats are disambiguated with a numeric suffix.
 */
function uniqueCategories(categories: string[]): string[] {
  const seen = new Map<string, number>()

  return categories.map((category) => {
    const count = seen.get(category) ?? 0
    seen.set(category, count + 1)
    return count === 0 ? category : `${category} (${count + 1})`
  })
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

  // When a category column is specified, keep only rows that belong to this chart (a non-blank
  // category). A single result set may pack several sections behind a discriminator column, where
  // each section leaves the other sections' columns null; without this filter those foreign rows
  // become empty categories that collapse together and break axis hover.
  const rows =
    categoryIdx >= 0
      ? result.rows.filter((row) => !isBlank(row[categoryIdx]))
      : result.rows

  const categories = uniqueCategories(
    rows.map((row, index) =>
      categoryIdx >= 0 ? String(row[categoryIdx] ?? '') : String(index + 1),
    ),
  )

  const series = spec.series
    .map((entry) => ({ entry, index: columnIndex(result, entry.column) }))
    .filter(({ index }) => index >= 0)
    .map(({ entry, index }) => ({
      label: entry.label ?? entry.column,
      data: rows.map((row) => toNumber(row[index])),
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

  // Drop rows that do not belong to this slice set (blank label) for the same reason as the
  // cartesian builder: a packed, multi-section result set leaves foreign rows' labels null.
  const rows =
    labelIdx >= 0
      ? result.rows.filter((row) => !isBlank(row[labelIdx]))
      : result.rows

  return rows.map((row, index) => ({
    id: index,
    value: toNumber(row[valueIdx]) ?? 0,
    label: labelIdx >= 0 ? String(row[labelIdx] ?? '') : String(index + 1),
  }))
}
