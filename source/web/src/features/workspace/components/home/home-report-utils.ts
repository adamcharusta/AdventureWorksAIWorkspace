import type { ReportDetailsDto } from '@/api/generated/model'
import type { ReportViewData } from '@/features/reports/components/ReportView'

/**
 * Formats backend ISO timestamps for compact workspace metadata. The browser locale is intentional:
 * this timestamp is informational UI text, not a persisted or comparable value.
 */
export function formatDateTime(value: string): string {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}

/**
 * Builds the frontend-only report rendering model from the API details DTO.
 *
 * The API stores two presentation shapes:
 * - latest report-level snapshot (`summary`, `result`, `charts`) for backwards compatibility and
 *   quick access;
 * - optional per-turn `sections`, used when follow-up questions append or refine analysis blocks.
 *
 * Returns `null` only when the report has no renderable summary, result, or sections, allowing the
 * workspace to fall back to showing the original prompt.
 */
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
