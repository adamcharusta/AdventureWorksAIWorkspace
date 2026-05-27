import AddRoundedIcon from '@mui/icons-material/AddRounded'
import AutoAwesomeRoundedIcon from '@mui/icons-material/AutoAwesomeRounded'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { ChatDrawer } from '../components/ChatDrawer'
import { MenuDrawer } from '../components/MenuDrawer'
import { ThemeModeSwitch } from '../components/ThemeModeSwitch'
import { useAuth } from '../hooks/use-auth'
import { toast } from '../lib/toast'

const HomePage = () => {
  const navigate = useNavigate()
  const { logout } = useAuth()
  const [isChatDrawerOpen, setIsChatDrawerOpen] = useState(true)
  const [isMenuOpen, setIsMenuOpen] = useState(true)

  const handleLogout = () => {
    logout()
    toast.info('Your session has been closed.', 'Signed out')
    navigate('/login', { replace: true })
  }

  return (
    <Box
      sx={{
        bgcolor: 'background.default',
        color: 'text.primary',
        display: 'flex',
        minHeight: '100vh',
      }}
    >
      <MenuDrawer
        open={isMenuOpen}
        onToggle={() => setIsMenuOpen((current) => !current)}
        onLogout={handleLogout}
      />

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          minWidth: 0,
        }}
      >
        <Stack
          spacing={3}
          sx={{
            mx: 'auto',
            maxWidth: 1240,
            px: { xs: 2, md: 4 },
            py: { xs: 2, md: 3 },
          }}
        >
          <Stack
            direction={{ xs: 'column', sm: 'row' }}
            spacing={2}
            sx={{
              alignItems: { xs: 'stretch', sm: 'center' },
              justifyContent: 'space-between',
            }}
          >
            <Box sx={{ minWidth: 0 }}>
              <Typography component="h1" sx={{ fontWeight: 700 }} variant="h4">
                Adventure Works
              </Typography>
              <Typography color="text.secondary" variant="body2">
                Business intelligence workspace
              </Typography>
            </Box>

            <Stack
              direction="row"
              spacing={1}
              sx={{
                alignItems: 'center',
                justifyContent: { xs: 'space-between', sm: 'flex-end' },
              }}
            >
              <ThemeModeSwitch />
              <Button startIcon={<AddRoundedIcon />} variant="contained">
                New report
              </Button>
            </Stack>
          </Stack>

          <Box
            sx={{
              borderColor: 'divider',
              borderRadius: 2,
              borderStyle: 'solid',
              borderWidth: 1,
              minHeight: { xs: 420, md: 560 },
              overflow: 'hidden',
            }}
          >
            <Stack
              spacing={3}
              sx={{
                justifyContent: 'space-between',
                minHeight: 'inherit',
                minWidth: 0,
                p: { xs: 2.5, md: 4 },
              }}
            >
              <Box>
                <Typography sx={{ fontWeight: 700 }} variant="h6">
                  Report workspace
                </Typography>
                <Typography color="text.secondary" sx={{ mt: 0.75 }}>
                  No report selected
                </Typography>
              </Box>

              <Stack
                spacing={1.5}
                sx={{
                  alignItems: 'center',
                  flexGrow: 1,
                  justifyContent: 'center',
                  py: 8,
                  textAlign: 'center',
                }}
              >
                <AutoAwesomeRoundedIcon color="primary" fontSize="large" />
                <Typography sx={{ fontWeight: 600 }}>
                  Start a new AI report
                </Typography>
                <Typography
                  color="text.secondary"
                  sx={{ maxWidth: 420 }}
                  variant="body2"
                >
                  Choose a saved report from the sidebar or create a new
                  analysis.
                </Typography>
              </Stack>
            </Stack>
          </Box>
        </Stack>
      </Box>

      <ChatDrawer
        open={isChatDrawerOpen}
        onToggle={() => setIsChatDrawerOpen((current) => !current)}
      />
    </Box>
  )
}

export default HomePage
