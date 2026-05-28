import CssBaseline from '@mui/material/CssBaseline'
import {
  createTheme,
  type PaletteMode,
  ThemeProvider,
} from '@mui/material/styles'
import { type ReactNode, useCallback, useMemo, useState } from 'react'

import { getGlobalStyleOverrides } from '@/styles/global-styles'

import { ThemeModeContext } from './theme-mode-context'

const THEME_MODE_KEY = 'theme_mode'

function getInitialThemeMode(): PaletteMode {
  const stored = localStorage.getItem(THEME_MODE_KEY)
  if (stored === 'light' || stored === 'dark') {
    return stored
  }

  if (
    typeof window === 'undefined' ||
    typeof window.matchMedia !== 'function'
  ) {
    return 'light'
  }

  return window.matchMedia('(prefers-color-scheme: dark)').matches
    ? 'dark'
    : 'light'
}

export function ThemeModeProvider({ children }: { children: ReactNode }) {
  const [mode, setMode] = useState<PaletteMode>(() => getInitialThemeMode())

  const toggleMode = useCallback(() => {
    setMode((current) => {
      const next: PaletteMode = current === 'light' ? 'dark' : 'light'
      localStorage.setItem(THEME_MODE_KEY, next)
      return next
    })
  }, [])

  const value = useMemo(() => ({ mode, toggleMode }), [mode, toggleMode])

  const theme = useMemo(
    () =>
      createTheme({
        palette: {
          mode,
        },
        components: getGlobalStyleOverrides(),
      }),
    [mode],
  )

  return (
    <ThemeModeContext.Provider value={value}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        {children}
      </ThemeProvider>
    </ThemeModeContext.Provider>
  )
}
