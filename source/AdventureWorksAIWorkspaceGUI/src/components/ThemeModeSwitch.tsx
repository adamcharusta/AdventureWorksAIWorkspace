import DarkModeRoundedIcon from '@mui/icons-material/DarkModeRounded'
import LightModeRoundedIcon from '@mui/icons-material/LightModeRounded'
import IconButton from '@mui/material/IconButton'
import Tooltip from '@mui/material/Tooltip'

import { useThemeMode } from '../hooks/use-theme-mode'

export function ThemeModeSwitch() {
  const { mode, toggleMode } = useThemeMode()
  const isDarkMode = mode === 'dark'

  return (
    <Tooltip
      title={isDarkMode ? 'Switch to light theme' : 'Switch to dark theme'}
    >
      <IconButton
        aria-label={
          isDarkMode ? 'Switch to light theme' : 'Switch to dark theme'
        }
        onClick={toggleMode}
        color="inherit"
      >
        {isDarkMode ? <LightModeRoundedIcon /> : <DarkModeRoundedIcon />}
      </IconButton>
    </Tooltip>
  )
}
