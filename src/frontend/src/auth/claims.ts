export const internalUserIdClaim = 'https://city-pulse.com/internal_user_id'

export function readInternalUserId(user: Record<string, unknown> | undefined): string | undefined {
  const value = user?.[internalUserIdClaim]
  return typeof value === 'string' ? value : undefined
}

export function readPermissions(accessToken: string): string[] {
  const payload = accessToken.split('.')[1]
  if (!payload) {
    return []
  }
  const json = new TextDecoder().decode(
    Uint8Array.from(atob(payload.replaceAll('-', '+').replaceAll('_', '/')), (c) =>
      c.charCodeAt(0),
    ),
  )
  const claims: { permissions?: unknown } = JSON.parse(json)
  return Array.isArray(claims.permissions)
    ? claims.permissions.filter((p): p is string => typeof p === 'string')
    : []
}
