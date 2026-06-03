import { screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { renderWithProviders } from '@/test/render-utils'

import { ReportView } from './ReportView'
import { SAMPLE_REPORT } from './sample-report'

describe('<ReportView />', () => {
  it('renders the question, insights, chart titles and data table', () => {
    renderWithProviders(<ReportView report={SAMPLE_REPORT} />)

    expect(screen.getByText(SAMPLE_REPORT.question)).toBeInTheDocument()
    expect(screen.getByText('Insights')).toBeInTheDocument()
    expect(
      screen.getByText(/Bikes are by far the strongest/i),
    ).toBeInTheDocument()

    // Chart titles are rendered by ReportChart regardless of SVG layout in jsdom.
    expect(screen.getByText('Sales by category')).toBeInTheDocument()
    expect(screen.getByText('Category share')).toBeInTheDocument()

    // The data table renders the result columns.
    expect(
      screen.getByRole('columnheader', { name: 'Category' }),
    ).toBeInTheDocument()
    expect(
      screen.getByRole('columnheader', { name: 'Sales' }),
    ).toBeInTheDocument()
  })

  it('renders conclusions when present and omits the block when absent', () => {
    const { rerender } = renderWithProviders(
      <ReportView
        report={{
          question: 'Revenue trend',
          insights: 'Revenue grew through the year.',
          conclusions:
            'The trend is clearly upward; if it holds, next year would be well above this one.',
          charts: [],
          result: null,
        }}
      />,
    )

    expect(screen.getByText('Conclusions')).toBeInTheDocument()
    expect(screen.getByText(/trend is clearly upward/i)).toBeInTheDocument()

    rerender(
      <ReportView
        report={{
          question: 'Revenue trend',
          insights: 'Revenue grew through the year.',
          conclusions: null,
          charts: [],
          result: null,
        }}
      />,
    )

    expect(screen.queryByText('Conclusions')).not.toBeInTheDocument()
  })

  it('omits charts when there is no result', () => {
    renderWithProviders(
      <ReportView
        report={{
          question: 'Empty report',
          insights: 'Nothing to show.',
          charts: [],
          result: null,
        }}
      />,
    )

    expect(screen.getByText('Empty report')).toBeInTheDocument()
    expect(screen.getByText('Nothing to show.')).toBeInTheDocument()
    expect(screen.queryByText('Sales by category')).not.toBeInTheDocument()
  })

  it('does not render the raw data table when a table chart already shows the result', () => {
    renderWithProviders(
      <ReportView
        report={{
          question: 'Inventory risk',
          insights: 'Several products have low stock coverage.',
          charts: [
            {
              kind: 'Table',
              title: 'Product details',
              categoryColumn: null,
              series: [],
              description: 'Rows with the highest shortage risk.',
            },
          ],
          result: {
            columns: [
              { name: 'Product', dataType: 'nvarchar' },
              { name: 'Stock', dataType: 'int' },
            ],
            rows: [
              ['Water Bottle', 252],
              ['Classic Vest', 180],
            ],
            rowCount: 2,
            truncated: false,
            elapsedMilliseconds: 12,
          },
        }}
      />,
    )

    expect(screen.getByText('Product details')).toBeInTheDocument()
    expect(screen.queryByText('Data (2 rows)')).not.toBeInTheDocument()
    expect(screen.getAllByRole('table')).toHaveLength(1)
  })

  it('renders appended report sections without replacing earlier sections', () => {
    renderWithProviders(
      <ReportView
        report={{
          question: 'Sales analysis',
          insights: 'Latest summary',
          charts: [],
          result: null,
          sections: [
            {
              id: 'section-1',
              title: 'Initial sales overview',
              question: 'Analyze sales',
              insights: 'The original section remains visible.',
              charts: [],
              result: null,
            },
            {
              id: 'section-2',
              title: 'Added customer segment',
              question: 'Add customer data',
              insights: 'The follow-up section is appended.',
              charts: [],
              result: null,
            },
          ],
        }}
      />,
    )

    expect(screen.getByText('Initial sales overview')).toBeInTheDocument()
    expect(screen.getByText('Added customer segment')).toBeInTheDocument()
    expect(screen.getByText(/original section remains/i)).toBeInTheDocument()
    expect(
      screen.getByText(/follow-up section is appended/i),
    ).toBeInTheDocument()
  })
})
