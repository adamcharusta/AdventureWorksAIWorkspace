import { ApiError } from '@/api/customFetch'
import type { HttpValidationProblemDetails } from '@/api/generated/model'

type ProblemDetailsBody = Partial<HttpValidationProblemDetails> & {
  detail?: string | null
  title?: string | null
}

/**
 * Extracts a human-readable message from an API error.
 *
 * The backend uses FluentValidation and returns RFC 7807 problem details, so
 * failures such as a duplicate login arrive either as an `errors` map or as a
 * `detail`/`title` field. We surface the first validation message, then fall
 * back to `detail`/`title`, and finally to the provided generic message.
 */
export function getApiErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof ApiError)) {
    return fallback
  }

  const body = error.body
  if (!body || typeof body !== 'object') {
    return fallback
  }

  const problem = body as ProblemDetailsBody

  const firstFieldError = Object.values(problem.errors ?? {})
    .flat()
    .find((message): message is string => Boolean(message))
  if (firstFieldError) {
    return firstFieldError
  }

  if (typeof problem.detail === 'string' && problem.detail.trim()) {
    return problem.detail
  }

  if (typeof problem.title === 'string' && problem.title.trim()) {
    return problem.title
  }

  return fallback
}
