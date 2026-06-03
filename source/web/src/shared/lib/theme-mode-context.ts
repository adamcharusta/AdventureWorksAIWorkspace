import type { PaletteMode } from '@mui/material/styles'
import { createContext } from 'react'

export type ThemeModeContextValue = {
  mode: PaletteMode
  toggleMode: () => void
}

export const ThemeModeContext = createContext<ThemeModeContextValue | null>(
  null,
)
