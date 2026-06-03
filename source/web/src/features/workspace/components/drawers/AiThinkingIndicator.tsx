import Box from '@mui/material/Box'

/**
 * Animated "AI is analyzing" bubble shown in the chat thread while a report response is pending.
 */
export function AiThinkingIndicator() {
  return (
    <Box
      aria-live="polite"
      role="status"
      sx={{
        display: 'flex',
        justifyContent: 'flex-start',
        px: 1,
        pb: 1,
        pointerEvents: 'none',
      }}
    >
      <Box
        sx={{
          alignItems: 'center',
          bgcolor: 'action.hover',
          borderBottomLeftRadius: '6px',
          borderRadius: '18px',
          color: 'text.secondary',
          display: 'inline-flex',
          fontSize: '0.8125rem',
          gap: 0.75,
          lineHeight: 1.4,
          px: 1.35,
          py: 0.875,
        }}
      >
        AI is analyzing
        <Box
          aria-hidden="true"
          component="span"
          sx={{
            alignItems: 'center',
            display: 'inline-flex',
            gap: 0.35,
            '& span': {
              animation: 'chatThinkingDot 1.1s ease-in-out infinite',
              bgcolor: 'currentColor',
              borderRadius: '50%',
              height: 4,
              opacity: 0.42,
              width: 4,
            },
            '& span:nth-of-type(2)': {
              animationDelay: '0.16s',
            },
            '& span:nth-of-type(3)': {
              animationDelay: '0.32s',
            },
            '@keyframes chatThinkingDot': {
              '0%, 80%, 100%': {
                opacity: 0.35,
                transform: 'translateY(0)',
              },
              '40%': {
                opacity: 1,
                transform: 'translateY(-3px)',
              },
            },
          }}
        >
          <span />
          <span />
          <span />
        </Box>
      </Box>
    </Box>
  )
}
