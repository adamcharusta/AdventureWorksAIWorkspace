/**
 * Decodes the payload (second segment) of a JWT without verifying its
 * signature. Returns `null` for malformed tokens or unparseable payloads.
 *
 * Signature verification happens on the backend; the client only reads
 * non-sensitive claims (roles, username, expiry) for UI decisions.
 */
export function decodeJwtPayload(
  token: string,
): Record<string, unknown> | null {
  try {
    const parts = token.split('.')
    if (parts.length !== 3) {
      return null
    }

    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const padded = base64.padEnd(
      base64.length + ((4 - (base64.length % 4)) % 4),
      '=',
    )

    return JSON.parse(atob(padded)) as Record<string, unknown>
  } catch {
    return null
  }
}
