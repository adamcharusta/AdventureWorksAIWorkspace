/**
 * Parses a JSON string, returning `fallback` instead of throwing when the
 * input is not valid JSON. The fallback lets callers choose their own
 * "unparseable" sentinel (e.g. the raw text, or `undefined`).
 */
export function safeJsonParse<TFallback>(
  raw: string,
  fallback: TFallback,
): unknown | TFallback {
  try {
    return JSON.parse(raw)
  } catch {
    return fallback
  }
}
