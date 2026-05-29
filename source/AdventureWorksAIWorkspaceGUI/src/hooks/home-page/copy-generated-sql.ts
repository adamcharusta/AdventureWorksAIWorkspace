import type { GeneratedSqlQueryDto } from '@/lib/report-api'
import { toast } from '@/lib/toast'

export async function copyGeneratedSqlQueries(
  sqlQueries: GeneratedSqlQueryDto[],
) {
  if (sqlQueries.length === 0) {
    toast.info('This report has no generated SQL to copy.', 'Reports')
    return
  }

  const text = sqlQueries
    .map((query, index) => `-- Query ${index + 1}\n${query.sqlText}`)
    .join('\n\n')

  try {
    await navigator.clipboard.writeText(text)
    toast.success(
      sqlQueries.length === 1
        ? 'SQL query copied to clipboard.'
        : `${sqlQueries.length} SQL queries copied to clipboard.`,
      'Reports',
    )
  } catch {
    toast.error('Could not copy the SQL to the clipboard.', 'Reports')
  }
}
