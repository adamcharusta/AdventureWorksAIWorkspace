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
  const sections = report.sections?.map((section) => ({
    id: section.id,
    question: section.question,
    title: section.title,
    insights: section.insights,
    conclusions: section.conclusions ?? null,
    charts: section.charts ?? [],
    result: section.result ?? null,
  }))

  if (
    !report.result &&
    !report.summary &&
    (!sections || sections.length === 0)
  ) {
    return null
  }

  return {
    question: report.originalPrompt,
    insights: report.summary ?? 'The report was generated successfully.',
    conclusions: report.conclusions ?? null,
    charts: report.charts ?? [],
    result: report.result ?? null,
    sections,
  }
}
