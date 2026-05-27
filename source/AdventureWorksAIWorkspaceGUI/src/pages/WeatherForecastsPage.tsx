import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import CircularProgress from '@mui/material/CircularProgress'
import Container from '@mui/material/Container'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { useNavigate } from 'react-router-dom'

import { useGetWeatherForecasts } from '../api/generated/weather-forecasts/weather-forecasts'
import { ThemeModeSwitch } from '../components/ThemeModeSwitch'
import { ToastPlayground } from '../components/ToastPlayground'
import { WeatherTrendCard } from '../components/WeatherTrendCard'
import { toast } from '../lib/toast'
import { useAuth } from '../lib/use-auth'

export function WeatherForecastsPage() {
  const navigate = useNavigate()
  const { logout } = useAuth()
  const { data, isLoading, error } = useGetWeatherForecasts(
    { days: 7 },
    { query: { select: (response) => response.data } },
  )

  const handleLogout = () => {
    logout()
    toast.info('Your session has been closed.', 'Signed out')
    navigate('/login', { replace: true })
  }

  return (
    <Container maxWidth="md" sx={{ py: 6 }}>
      <Stack spacing={3}>
        <Box
          sx={{
            display: 'flex',
            alignItems: { xs: 'flex-start', sm: 'center' },
            justifyContent: 'space-between',
            gap: 2,
            flexDirection: { xs: 'column', sm: 'row' },
          }}
        >
          <Box>
            <Typography variant="h4" component="h1" gutterBottom>
              Weather forecasts
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Demo of GUI ↔ API connectivity via TanStack Query + Orval.
            </Typography>
          </Box>

          <Stack direction="row" spacing={1}>
            <ThemeModeSwitch />
            <Button variant="outlined" color="inherit" onClick={handleLogout}>
              Log out
            </Button>
          </Stack>
        </Box>

        {isLoading && (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
            <CircularProgress />
          </Box>
        )}

        {error ? (
          <Alert severity="error">
            Failed to load forecasts: {(error as Error).message}
          </Alert>
        ) : null}

        {data && data.length > 0 ? <WeatherTrendCard forecasts={data} /> : null}

        <ToastPlayground />
      </Stack>
    </Container>
  )
}
