import HomeRoundedIcon from '@mui/icons-material/HomeRounded'
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded'
import Box from '@mui/material/Box'
import IconButton from '@mui/material/IconButton'
import Stack from '@mui/material/Stack'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'

import { ThemeModeSwitch } from '@/shared/components/theme/ThemeModeSwitch'

type AdminPanelHeaderProps = {
  onGoHome: () => void
  onLogout: () => void
}

export function AdminPanelHeader({
  onGoHome,
  onLogout,
}: AdminPanelHeaderProps) {
  return (
    <Box
      sx={{
        alignItems: 'center',
        display: 'flex',
        gap: 2,
        flexWrap: 'wrap',
        justifyContent: 'space-between',
      }}
    >
      <Box>
        <Typography component="h1" sx={{ fontWeight: 700 }} variant="h4">
          Admin Panel
        </Typography>
        <Typography color="text.secondary" variant="body2">
          Manage application users, roles, and account resets.
        </Typography>
      </Box>
      <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
        <Tooltip title="Back to home">
          <IconButton
            aria-label="Back to home"
            color="inherit"
            onClick={onGoHome}
          >
            <HomeRoundedIcon />
          </IconButton>
        </Tooltip>
        <ThemeModeSwitch />
        <Tooltip title="Log out">
          <IconButton aria-label="Log out" color="inherit" onClick={onLogout}>
            <LogoutRoundedIcon />
          </IconButton>
        </Tooltip>
      </Stack>
    </Box>
  )
}
