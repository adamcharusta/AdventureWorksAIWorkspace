import { alpha, type ThemeOptions } from '@mui/material/styles'

export function getGlobalStyleOverrides(): NonNullable<
  ThemeOptions['components']
> {
  return {
    MuiCssBaseline: {
      styleOverrides: (theme) => {
        const scrollbarThumb = alpha(theme.palette.text.primary, 0.28)
        const scrollbarThumbHover = alpha(theme.palette.text.primary, 0.44)
        const scrollbarThumbActive = alpha(theme.palette.text.primary, 0.56)

        return {
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
          '*': {
            scrollbarColor: `${scrollbarThumb} transparent`,
            scrollbarWidth: 'thin',
          },
          '*::-webkit-scrollbar': {
            height: 8,
            width: 8,
          },
          '*::-webkit-scrollbar-track': {
            backgroundColor: 'transparent',
          },
          '*::-webkit-scrollbar-thumb': {
            backgroundClip: 'padding-box',
            backgroundColor: scrollbarThumb,
            border: '2px solid transparent',
            borderRadius: 999,
            minHeight: 32,
          },
          '*::-webkit-scrollbar-thumb:hover': {
            backgroundColor: scrollbarThumbHover,
          },
          '*::-webkit-scrollbar-thumb:active': {
            backgroundColor: scrollbarThumbActive,
          },
          '*::-webkit-scrollbar-corner': {
            backgroundColor: 'transparent',
          },
          a: {
            color: 'inherit',
            textDecoration: 'none',
          },
        }
      },
    },
  }
}
