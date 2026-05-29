import { useNavigate } from 'react-router-dom'

import { useAuth } from '@/hooks/use-auth'
import { useThemeMode } from '@/hooks/use-theme-mode'
import { isAdminRole } from '@/lib/auth-roles'
import { toast } from '@/lib/toast'

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
