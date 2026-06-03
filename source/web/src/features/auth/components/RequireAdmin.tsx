import { Navigate, Outlet } from 'react-router-dom'

import { useAuth } from '@/features/auth/hooks/use-auth'
import { isAdminRole } from '@/features/auth/lib/auth-roles'

export function RequireAdmin() {
  const { role } = useAuth()

  if (!isAdminRole(role)) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}
