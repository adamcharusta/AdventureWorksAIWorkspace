import { useNavigate } from 'react-router-dom'

import { useAuth } from '@/features/auth/hooks/use-auth'
import { isAdminRole } from '@/features/auth/lib/auth-roles'
import { useThemeMode } from '@/shared/hooks/use-theme-mode'
import { toast } from '@/shared/lib/toast'

export function useHomeShell() {
  const navigate = useNavigate()
  const { username, role, logout } = useAuth()
  const { mode, toggleMode } = useThemeMode()

  const handleLogout = () => {
    logout()
    toast.info('Your session has been closed.', 'Signed out')
    navigate('/login', { replace: true })
  }

  return {
    isAdmin: isAdminRole(role),
    mode,
    onLogout: handleLogout,
    onOpenAdminPanel: () => navigate('/admin'),
    onToggleTheme: toggleMode,
    username,
  }
}
