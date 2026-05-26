export class ApiError extends Error {
  readonly status: number
  readonly body: unknown

  constructor(status: number, body: unknown, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.body = body
  }
}

export const customFetch = async <T>(
  url: string,
  options?: RequestInit,
): Promise<T> => {
  const response = await fetch(url, options)

  const rawBody = [204, 205, 304].includes(response.status)
    ? ''
    : await response.text()
  const parsedBody: unknown = rawBody ? safeJsonParse(rawBody) : undefined

  if (!response.ok) {
    throw new ApiError(
      response.status,
      parsedBody,
      `Request to ${url} failed with status ${response.status}`,
    )
  }

  return {
    data: parsedBody,
    status: response.status,
    headers: response.headers,
  } as T
}

function safeJsonParse(raw: string): unknown {
  try {
    return JSON.parse(raw)
  } catch {
    return raw
  }
}
