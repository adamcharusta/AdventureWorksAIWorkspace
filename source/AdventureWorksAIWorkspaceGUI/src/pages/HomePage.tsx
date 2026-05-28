import AddRoundedIcon from '@mui/icons-material/AddRounded'
import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings'
import AutoAwesomeRoundedIcon from '@mui/icons-material/AutoAwesomeRounded'
import DarkModeRoundedIcon from '@mui/icons-material/DarkModeRounded'
import DashboardRoundedIcon from '@mui/icons-material/DashboardRounded'
import FolderRoundedIcon from '@mui/icons-material/FolderRounded'
import HistoryRoundedIcon from '@mui/icons-material/HistoryRounded'
import LightModeRoundedIcon from '@mui/icons-material/LightModeRounded'
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import StarRoundedIcon from '@mui/icons-material/StarRounded'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { ChatDrawer } from '../components/workspace/ChatDrawer'
import {
  MenuDrawer,
  type MenuDrawerItem,
} from '../components/workspace/MenuDrawer'
import { useAuth } from '../hooks/use-auth'
import { useThemeMode } from '../hooks/use-theme-mode'
import { isAdminRole } from '../lib/auth-roles'
import { toast } from '../lib/toast'

const reportItems: MenuDrawerItem[] = [
  { label: 'Workspace', icon: <DashboardRoundedIcon />, selected: true },
  { label: 'Recent reports', icon: <HistoryRoundedIcon /> },
  { label: 'Saved reports', icon: <FolderRoundedIcon /> },
  { label: 'Favorite reports', icon: <StarRoundedIcon /> },
  { label: 'Search reports', icon: <SearchRoundedIcon /> },
]

const HomePage = () => {
  const navigate = useNavigate()
  const { username, role, logout } = useAuth()
  const { mode, toggleMode } = useThemeMode()
  const [isChatDrawerOpen, setIsChatDrawerOpen] = useState(true)
  const [isMenuOpen, setIsMenuOpen] = useState(true)

  const isAdmin = isAdminRole(role)

  const handleLogout = () => {
    logout()
    toast.info('Your session has been closed.', 'Signed out')
    navigate('/login', { replace: true })
  }

  const bottomItems: MenuDrawerItem[] = [
    ...(isAdmin
      ? [
          {
            label: 'Admin Panel',
            icon: <AdminPanelSettingsIcon />,
            onClick: () => navigate('/admin'),
          },
        ]
      : []),
    {
      label: 'Toggle theme',
      icon:
        mode === 'dark' ? <LightModeRoundedIcon /> : <DarkModeRoundedIcon />,
      onClick: toggleMode,
    },
    {
      label: 'Log out',
      icon: <LogoutRoundedIcon />,
      onClick: handleLogout,
    },
  ]

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
        bottomItems={bottomItems}
        items={reportItems}
        open={isMenuOpen}
        onToggle={() => setIsMenuOpen((current) => !current)}
        title={`Hello ${username ?? 'there'}!`}
        subtitle="Generate insights and reports"
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
