export function isAdminRole(role: string | null) {
  return role?.toLowerCase() === 'admin'
}
