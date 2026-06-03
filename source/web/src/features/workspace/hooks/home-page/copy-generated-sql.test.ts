import { beforeEach, describe, expect, it, vi } from 'vitest'

import type { GeneratedSqlQueryDto } from '@/api/generated/model'
import { toast } from '@/shared/lib/toast'

import { copyGeneratedSqlQueries } from './copy-generated-sql'

vi.mock('@/shared/lib/toast', () => ({
  toast: {
    error: vi.fn(),
    info: vi.fn(),
    success: vi.fn(),
  },
}))

describe('copyGeneratedSqlQueries', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('shows an informational toast when there are no generated queries', async () => {
    const writeText = stubClipboard()

    await copyGeneratedSqlQueries([])

    expect(writeText).not.toHaveBeenCalled()
    expect(toast.info).toHaveBeenCalledWith(
      'This report has no generated SQL to copy.',
      'Reports',
    )
  })

  it('copies a single generated query to the clipboard', async () => {
    const writeText = stubClipboard()

    await copyGeneratedSqlQueries([sqlQuery('SELECT 1')])

    expect(writeText).toHaveBeenCalledWith('-- Query 1\nSELECT 1')
    expect(toast.success).toHaveBeenCalledWith(
      'SQL query copied to clipboard.',
      'Reports',
    )
  })

  it('copies multiple generated queries with query headers', async () => {
    const writeText = stubClipboard()

    await copyGeneratedSqlQueries([sqlQuery('SELECT 1'), sqlQuery('SELECT 2')])

    expect(writeText).toHaveBeenCalledWith(
      '-- Query 1\nSELECT 1\n\n-- Query 2\nSELECT 2',
    )
    expect(toast.success).toHaveBeenCalledWith(
      '2 SQL queries copied to clipboard.',
      'Reports',
    )
  })

  it('shows an error toast when clipboard writing fails', async () => {
    stubClipboard(vi.fn().mockRejectedValue(new Error('Clipboard denied')))

    await copyGeneratedSqlQueries([sqlQuery('SELECT 1')])

    expect(toast.error).toHaveBeenCalledWith(
      'Could not copy the SQL to the clipboard.',
      'Reports',
    )
  })
})

function stubClipboard(writeText = vi.fn().mockResolvedValue(undefined)) {
  Object.defineProperty(navigator, 'clipboard', {
    configurable: true,
    value: { writeText },
  })

  return writeText
}

function sqlQuery(sqlText: string): GeneratedSqlQueryDto {
  return {
    id: crypto.randomUUID(),
    userPrompt: 'Show sales',
    sqlText,
    validationStatus: 'Valid',
    executionStatus: 'Executed',
    createdAt: new Date().toISOString(),
  } as GeneratedSqlQueryDto
}
