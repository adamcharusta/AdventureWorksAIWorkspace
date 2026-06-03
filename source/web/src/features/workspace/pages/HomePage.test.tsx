import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { Route, Routes, useLocation } from 'react-router-dom'
import { describe, expect, it } from 'vitest'

import { renderWithProviders } from '@/test/render-utils'
import { server } from '@/test/server'

import HomePage from './HomePage'

function LocationPath() {
  const location = useLocation()

  return <span data-testid="location-path">{location.pathname}</span>
}

function renderHomePage(route = '/') {
  return renderWithProviders(
    <>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/raport/:reportId" element={<HomePage />} />
      </Routes>
      <LocationPath />
    </>,
    { route, isAuthenticated: true },
  )
}

describe('<HomePage />', () => {
  it('toggles both menu and chat drawers', async () => {
    const user = userEvent.setup()

    renderHomePage()

    const collapseMenuButton = screen.getByRole('button', {
      name: /collapse menu/i,
    })
    const collapseChatButton = screen.getByRole('button', {
      name: /collapse chat drawer/i,
    })

    await user.click(collapseMenuButton)
    await user.click(collapseChatButton)

    expect(
      screen.getByRole('button', { name: /expand menu/i }),
    ).toBeInTheDocument()
    expect(
      screen.getByRole('button', { name: /expand chat drawer/i }),
    ).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /expand menu/i }))
    await user.click(
      screen.getByRole('button', { name: /expand chat drawer/i }),
    )

    expect(
      screen.getByRole('button', { name: /collapse menu/i }),
    ).toBeInTheDocument()
    expect(
      screen.getByRole('button', { name: /collapse chat drawer/i }),
    ).toBeInTheDocument()
  })

  it('shows saved reports from the API and lets the user rename a report', async () => {
    const user = userEvent.setup()
    let reportTitle = 'Sales by product category'
    const reportSummary = {
      id: 'report-1',
      status: 'Ready',
      isFavorite: false,
      createdAt: '2026-05-29T00:00:00Z',
      updatedAt: '2026-05-29T01:00:00Z',
    }

    server.use(
      http.get('*/api/reports', () =>
        HttpResponse.json({
          reports: [{ ...reportSummary, title: reportTitle }],
        }),
      ),
      http.get('*/api/reports/report-1', () =>
        HttpResponse.json({
          ...reportSummary,
          title: reportTitle,
          originalPrompt: 'Show sales by product category',
          summary: 'Bikes produce most of the sales.',
          messages: [],
          generatedSqlQueries: [],
        }),
      ),
      http.put('*/api/reports/report-1/title', async ({ request }) => {
        const body = (await request.json()) as { title: string }
        reportTitle = body.title

        return HttpResponse.json({
          ...reportSummary,
          title: reportTitle,
        })
      }),
    )

    renderHomePage('/raport/report-1')

    expect(
      await screen.findByRole('button', {
        name: /sales by product category/i,
      }),
    ).toBeInTheDocument()
    expect(
      await screen.findByRole('heading', {
        name: /sales by product category/i,
        level: 1,
      }),
    ).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /report actions/i }))
    await user.click(screen.getByRole('menuitem', { name: /rename report/i }))
    await user.clear(screen.getByRole('textbox', { name: /report title/i }))
    await user.type(
      screen.getByRole('textbox', { name: /report title/i }),
      'Category sales overview',
    )
    await user.click(screen.getByRole('button', { name: /save title/i }))

    expect(
      await screen.findByRole('heading', {
        name: /category sales overview/i,
        level: 1,
      }),
    ).toBeInTheDocument()
  })

  it('uses the URL to open reports and returns to the root URL for a new report', async () => {
    const user = userEvent.setup()

    server.use(
      http.get('*/api/reports', () =>
        HttpResponse.json({
          reports: [
            {
              id: 'report-1',
              title: 'Sales by product category',
              status: 'Ready',
              isFavorite: false,
              createdAt: '2026-05-29T00:00:00Z',
              updatedAt: '2026-05-29T01:00:00Z',
            },
          ],
        }),
      ),
      http.get('*/api/reports/report-1', () =>
        HttpResponse.json({
          id: 'report-1',
          title: 'Sales by product category',
          originalPrompt: 'Show sales by product category',
          summary: 'Bikes produce most of the sales.',
          status: 'Ready',
          isFavorite: false,
          createdAt: '2026-05-29T00:00:00Z',
          updatedAt: '2026-05-29T01:00:00Z',
          messages: [],
          generatedSqlQueries: [],
        }),
      ),
    )

    renderHomePage()

    expect(
      await screen.findByText(/use the ai chat to create the next analysis/i),
    ).toBeInTheDocument()

    await user.click(
      await screen.findByRole('button', {
        name: /sales by product category/i,
      }),
    )

    expect(screen.getByTestId('location-path')).toHaveTextContent(
      '/raport/report-1',
    )
    expect(
      await screen.findByRole('heading', {
        name: /sales by product category/i,
        level: 1,
      }),
    ).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /new report/i }))

    expect(screen.getByTestId('location-path')).toHaveTextContent('/')
    expect(
      await screen.findByText(/use the ai chat to create the next analysis/i),
    ).toBeInTheDocument()
  })

  it('creates a report from chat and renders charts with tabular data', async () => {
    const user = userEvent.setup()
    const reports: Array<{
      id: string
      title: string
      status: string
      isFavorite: boolean
      createdAt: string
      updatedAt: string
    }> = []

    const reportDetails = {
      id: 'report-2',
      title: 'Sales by category',
      originalPrompt: 'Show sales by category',
      summary: 'Bikes lead sales, with accessories trailing.',
      status: 'Ready',
      isFavorite: false,
      createdAt: '2026-05-29T00:00:00Z',
      updatedAt: '2026-05-29T01:00:00Z',
      result: {
        columns: [
          { name: 'Category', dataType: 'nvarchar' },
          { name: 'Sales', dataType: 'decimal' },
        ],
        rows: [
          ['Bikes', 28000],
          ['Accessories', 900],
        ],
        rowCount: 2,
        truncated: false,
        elapsedMilliseconds: 42,
      },
      charts: [
        {
          kind: 'Bar',
          title: 'Sales by category',
          categoryColumn: 'Category',
          series: [{ column: 'Sales', label: 'Sales' }],
          description: 'Total sales per category.',
        },
      ],
      messages: [
        {
          id: 'message-1',
          role: 'User',
          content: 'Show sales by category',
          sortOrder: 1,
          relatedSqlQueryId: null,
          createdAt: '2026-05-29T00:00:00Z',
        },
        {
          id: 'message-2',
          role: 'Assistant',
          content: 'Bikes lead sales, with accessories trailing.',
          sortOrder: 2,
          relatedSqlQueryId: 'sql-1',
          createdAt: '2026-05-29T00:01:00Z',
        },
      ],
      generatedSqlQueries: [
        {
          id: 'sql-1',
          sourceMessageId: 'message-1',
          userPrompt: 'Show sales by category',
          sqlText: 'SELECT Category, Sales FROM SalesByCategory;',
          explanation: null,
          validationStatus: 'Valid',
          validationMessage: null,
          executionStatus: 'Executed',
          executionMessage: null,
          inputTokens: 10,
          outputTokens: 20,
          resultRowCount: 2,
          resultColumnCount: 2,
          durationMs: 42,
          createdAt: '2026-05-29T00:00:30Z',
        },
      ],
    }

    server.use(
      http.get('*/api/reports', () => HttpResponse.json({ reports })),
      http.get('*/api/reports/report-2', () =>
        HttpResponse.json(reportDetails),
      ),
      http.post('*/api/reports', () => {
        reports.unshift({
          id: reportDetails.id,
          title: reportDetails.title,
          status: reportDetails.status,
          isFavorite: reportDetails.isFavorite,
          createdAt: reportDetails.createdAt,
          updatedAt: reportDetails.updatedAt,
        })

        return HttpResponse.json({
          report: reportDetails,
          userMessage: reportDetails.messages[0],
          assistantMessage: reportDetails.messages[1],
          sqlQuery: reportDetails.generatedSqlQueries[0],
          outcome: 'Executed',
          message: null,
          result: reportDetails.result,
          charts: reportDetails.charts,
        })
      }),
    )

    renderHomePage()

    await user.type(
      await screen.findByRole('textbox', { name: /chat message/i }),
      'Show sales by category',
    )
    await user.click(screen.getByRole('button', { name: /send/i }))

    expect(
      await screen.findByRole('heading', {
        name: /sales by category/i,
        level: 1,
      }),
    ).toBeInTheDocument()
    expect(screen.getByTestId('location-path')).toHaveTextContent(
      '/raport/report-2',
    )
    expect(screen.getAllByText(/bikes lead sales/i).length).toBeGreaterThan(0)
    expect(screen.getByText(/data \(2 rows\)/i)).toBeInTheDocument()
    expect(screen.getByText('Category')).toBeInTheDocument()
    expect(screen.getAllByText('Sales').length).toBeGreaterThan(0)
  })
})
