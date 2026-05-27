import type { ThemeOptions } from '@mui/material/styles'

export function getGlobalStyleOverrides(): NonNullable<
  ThemeOptions['components']
> {
  return {
    MuiCssBaseline: {
      styleOverrides: {
        html: {
          WebkitFontSmoothing: 'antialiased',
          MozOsxFontSmoothing: 'grayscale',
        },
        body: {
          margin: 0,
          minHeight: '100vh',
          minWidth: '100vw',
        },
        a: {
          color: 'inherit',
          textDecoration: 'none',
        },
      },
    },
  }
}
