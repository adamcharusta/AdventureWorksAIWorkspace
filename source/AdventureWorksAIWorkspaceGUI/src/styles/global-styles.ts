import type { ThemeOptions } from '@mui/material/styles'

export function getGlobalStyleOverrides(): NonNullable<
  ThemeOptions['components']
> {
  return {
    MuiCssBaseline: {
      styleOverrides: {
        html: {
          height: '100%',
          WebkitFontSmoothing: 'antialiased',
          MozOsxFontSmoothing: 'grayscale',
          overflowX: 'hidden',
        },
        body: {
          height: '100%',
          margin: 0,
          minHeight: '100vh',
          width: '100%',
          overflowX: 'hidden',
        },
        '#root': {
          minHeight: '100vh',
          width: '100%',
          overflowX: 'hidden',
        },
        a: {
          color: 'inherit',
          textDecoration: 'none',
        },
      },
    },
  }
}
