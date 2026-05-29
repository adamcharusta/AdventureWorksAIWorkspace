import type { ReportViewData } from './ReportView'

/**
 * Temporary sample used to preview report rendering (charts + insights) in the workspace until
 * the report API client is generated (`npm run api:gen`) and the chat flow is wired to live data.
 * Remove this file and its usage once CreateReport/AddReportMessage are connected.
 */
export const SAMPLE_REPORT: ReportViewData = {
  question: 'Top product categories by sales',
  insights:
    'Bikes are by far the strongest category, accounting for the large majority of sales. ' +
    'Components, Clothing, and Accessories together make up a small share, suggesting an ' +
    'opportunity to grow accessory attach-rates alongside bike sales.',
  charts: [
    {
      kind: 'Bar',
      title: 'Sales by category',
      categoryColumn: 'Category',
      series: [{ column: 'Sales', label: 'Sales (USD)' }],
      description: 'Total sales per product category.',
    },
    {
      kind: 'Pie',
      title: 'Category share',
      categoryColumn: 'Category',
      series: [{ column: 'Sales', label: 'Sales' }],
      description: null,
    },
  ],
  result: {
    columns: [
      { name: 'Category', dataType: 'nvarchar' },
      { name: 'Sales', dataType: 'decimal' },
    ],
    rows: [
      ['Bikes', 28000],
      ['Components', 3400],
      ['Clothing', 1200],
      ['Accessories', 900],
    ],
    rowCount: 4,
    truncated: false,
    elapsedMilliseconds: 42,
  },
}

const MONTHLY_REVENUE_SAMPLE: ReportViewData = {
  question: 'Monthly revenue in 2013',
  insights:
    'Revenue trends upward through the year with a noticeable lift in the second half. ' +
    'The strongest months are Q4, which is worth planning inventory and marketing around.',
  charts: [
    {
      kind: 'Line',
      title: 'Revenue by month',
      categoryColumn: 'Month',
      series: [{ column: 'Revenue', label: 'Revenue (USD)' }],
      description: 'Monthly revenue across the year.',
    },
  ],
  result: {
    columns: [
      { name: 'Month', dataType: 'nvarchar' },
      { name: 'Revenue', dataType: 'decimal' },
    ],
    rows: [
      ['Jan', 12000],
      ['Feb', 14500],
      ['Mar', 13200],
      ['Apr', 16000],
      ['May', 17800],
      ['Jun', 19000],
      ['Jul', 21000],
      ['Aug', 22500],
      ['Sep', 24000],
      ['Oct', 27000],
      ['Nov', 29500],
      ['Dec', 33000],
    ],
    rowCount: 12,
    truncated: false,
    elapsedMilliseconds: 51,
  },
}

/**
 * A report shown in the workspace report list. Mirrors the shape the live API will provide:
 * a saved report with an AI-suggested (user-editable) title plus its rendered content.
 */
export type WorkspaceReport = ReportViewData & {
  id: string
  title: string
}

/** Temporary seed for the report list until the live report API client is wired. */
export const SAMPLE_REPORTS: WorkspaceReport[] = [
  {
    id: 'sample-sales-by-category',
    title: 'Sales by product category',
    ...SAMPLE_REPORT,
  },
  {
    id: 'sample-monthly-revenue',
    title: 'Monthly revenue trend',
    ...MONTHLY_REVENUE_SAMPLE,
  },
]
