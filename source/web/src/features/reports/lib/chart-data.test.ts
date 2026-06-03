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

  it('keeps a null series value as a gap when the category is present', () => {
    const spec: ChartSpec = {
      kind: 'Line',
      title: 'Revenue',
      categoryColumn: 'Month',
      series: [{ column: 'Revenue', label: 'Revenue' }],
    }

    const data = buildCartesianData(spec, result)

    expect(data.categories).toEqual(['Jan', 'Feb', 'Mar'])
    expect(data.series[0].data).toEqual([100, 150, null])
  })

  it('drops foreign rows whose category column is blank (packed multi-section result)', () => {
    const packed: TabularResult = {
      columns: [
        { name: 'Section', dataType: 'varchar' },
        { name: 'Product', dataType: 'nvarchar' },
        { name: 'Revenue', dataType: 'decimal' },
        { name: 'Region', dataType: 'nvarchar' },
      ],
      rows: [
        ['Top products', 'Bike A', 500, null],
        ['Top products', 'Bike B', 300, null],
        ['Weak regions', null, 900, 'Europe'],
        ['Weak regions', null, 700, 'Pacific'],
      ],
      rowCount: 4,
      truncated: false,
      elapsedMilliseconds: 1,
    }

    const spec: ChartSpec = {
      kind: 'Bar',
      title: 'Top products by revenue',
      categoryColumn: 'Product',
      series: [{ column: 'Revenue', label: 'Revenue' }],
    }

    const data = buildCartesianData(spec, packed)

    expect(data.categories).toEqual(['Bike A', 'Bike B'])
    expect(data.series[0].data).toEqual([500, 300])
  })

  it('disambiguates duplicate category labels', () => {
    const dupes: TabularResult = {
      columns: [
        { name: 'Product', dataType: 'nvarchar' },
        { name: 'Revenue', dataType: 'decimal' },
      ],
      rows: [
        ['Road Bike', 10],
        ['Road Bike', 20],
        ['Road Bike', 30],
      ],
      rowCount: 3,
      truncated: false,
      elapsedMilliseconds: 1,
    }

    const spec: ChartSpec = {
      kind: 'Bar',
      title: 'Revenue',
      categoryColumn: 'Product',
      series: [{ column: 'Revenue', label: 'Revenue' }],
    }

    const data = buildCartesianData(spec, dupes)

    expect(data.categories).toEqual([
      'Road Bike',
      'Road Bike (2)',
      'Road Bike (3)',
    ])
    expect(data.series[0].data).toEqual([10, 20, 30])
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

  it('drops foreign rows whose label is blank (packed multi-section result)', () => {
    const packed: TabularResult = {
      columns: [
        { name: 'Region', dataType: 'nvarchar' },
        { name: 'Revenue', dataType: 'decimal' },
      ],
      rows: [
        ['Europe', 900],
        [null, 0],
        ['Pacific', 700],
      ],
      rowCount: 3,
      truncated: false,
      elapsedMilliseconds: 1,
    }

    const spec: ChartSpec = {
      kind: 'Pie',
      title: 'Revenue by region',
      categoryColumn: 'Region',
      series: [{ column: 'Revenue', label: null }],
    }

    expect(buildPieData(spec, packed)).toEqual([
      { id: 0, value: 900, label: 'Europe' },
      { id: 1, value: 700, label: 'Pacific' },
    ])
  })
})
