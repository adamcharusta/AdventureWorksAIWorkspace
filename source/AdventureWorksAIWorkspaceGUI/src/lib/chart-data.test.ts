import { describe, expect, it } from 'vitest'

import { buildCartesianData, buildPieData } from './chart-data'
import type { ChartSpec, TabularResult } from './report-types'

const result: TabularResult = {
  columns: [
    { name: 'Month', dataType: 'nvarchar' },
    { name: 'Revenue', dataType: 'decimal' },
    { name: 'Orders', dataType: 'int' },
  ],
  rows: [
    ['Jan', 100, 5],
    ['Feb', '150', 7],
    ['Mar', null, 9],
  ],
  rowCount: 3,
  truncated: false,
  elapsedMilliseconds: 1,
}

describe('buildCartesianData', () => {
  it('maps categories and numeric series, coercing strings and nulls', () => {
    const spec: ChartSpec = {
      kind: 'Bar',
      title: 'Revenue',
      categoryColumn: 'Month',
      series: [{ column: 'Revenue', label: 'Revenue' }],
    }

    const data = buildCartesianData(spec, result)

    expect(data.categories).toEqual(['Jan', 'Feb', 'Mar'])
    expect(data.series).toHaveLength(1)
    expect(data.series[0].label).toBe('Revenue')
    expect(data.series[0].data).toEqual([100, 150, null])
  })

  it('drops series whose column does not exist', () => {
    const spec: ChartSpec = {
      kind: 'Bar',
      title: 'Revenue',
      categoryColumn: 'Month',
      series: [
        { column: 'Revenue', label: null },
        { column: 'Missing', label: null },
      ],
    }

    const data = buildCartesianData(spec, result)

    expect(data.series).toHaveLength(1)
    expect(data.series[0].label).toBe('Revenue')
  })

  it('falls back to row indices when the category column is missing', () => {
    const spec: ChartSpec = {
      kind: 'Line',
      title: 'Orders',
      categoryColumn: 'Nope',
      series: [{ column: 'Orders', label: null }],
    }

    const data = buildCartesianData(spec, result)

    expect(data.categories).toEqual(['1', '2', '3'])
  })
})

describe('buildPieData', () => {
  it('maps label and value columns into pie data', () => {
    const spec: ChartSpec = {
      kind: 'Pie',
      title: 'Share',
      categoryColumn: 'Month',
      series: [{ column: 'Revenue', label: null }],
    }

    const data = buildPieData(spec, result)

    expect(data).toEqual([
      { id: 0, value: 100, label: 'Jan' },
      { id: 1, value: 150, label: 'Feb' },
      { id: 2, value: 0, label: 'Mar' },
    ])
  })

  it('returns empty when the value column does not exist', () => {
    const spec: ChartSpec = {
      kind: 'Pie',
      title: 'Share',
      categoryColumn: 'Month',
      series: [{ column: 'Missing', label: null }],
    }

    expect(buildPieData(spec, result)).toEqual([])
  })
})
