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
})
