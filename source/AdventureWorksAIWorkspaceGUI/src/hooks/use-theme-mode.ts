import { useContext } from 'react'

import { ThemeModeContext } from '../lib/theme-mode-context'

export function useThemeMode() {
  const context = useContext(ThemeModeContext)
  if (!context) {
    throw new Error('useThemeMode must be used within ThemeModeProvider')
  }

  return context
}
