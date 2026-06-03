import Box from '@mui/material/Box'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import type { ReactNode } from 'react'

export type HomeWorkspaceStateProps = {
  action?: ReactNode
  description?: string
  icon: ReactNode
  title: string
}

export function HomeWorkspaceState({
  action,
  description,
  icon,
  title,
}: HomeWorkspaceStateProps) {
  return (
    <Stack
      spacing={1.5}
      sx={{
        alignItems: 'center',
        justifyContent: 'center',
        minHeight: { xs: 360, md: 480 },
        textAlign: 'center',
      }}
    >
      <Box sx={{ alignItems: 'center', display: 'flex', minHeight: 40 }}>
        {icon}
      </Box>
      <Typography sx={{ fontWeight: 600 }}>{title}</Typography>
      {description ? (
        <Typography
          color="text.secondary"
          sx={{ maxWidth: 420 }}
          variant="body2"
        >
          {description}
        </Typography>
      ) : null}
      {action ? <Box sx={{ pt: 1 }}>{action}</Box> : null}
    </Stack>
  )
}
