import type { ReportViewData } from '@/components/reports/ReportView'
import type { ReportDetailsDto } from '@/lib/report-api'

export function formatDateTime(value: string): string {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

export function createReportViewData(
  report: ReportDetailsDto,
): ReportViewData | null {
  if (!report.result && !report.summary) {
    return null
  }

  return {
    question: report.originalPrompt,
    insights: report.summary ?? 'The report was generated successfully.',
    charts: report.charts ?? [],
    result: report.result ?? null,
  }
}
