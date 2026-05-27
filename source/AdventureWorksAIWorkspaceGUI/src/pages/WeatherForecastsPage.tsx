import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import Container from '@mui/material/Container'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'

import { useGetWeatherForecasts } from '../api/generated/weather-forecasts/weather-forecasts'
import { ToastPlayground } from '../components/ToastPlayground'
import { WeatherTrendCard } from '../components/WeatherTrendCard'

export function WeatherForecastsPage() {
  const { data, isLoading, error } = useGetWeatherForecasts(
    { days: 7 },
    { query: { select: (response) => response.data } },
  )

  return (
    <Container maxWidth="md" sx={{ py: 6 }}>
      <Stack spacing={3}>
        <Box>
          <Typography variant="h4" component="h1" gutterBottom>
            Weather forecasts
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Demo of GUI ↔ API connectivity via TanStack Query + Orval.
          </Typography>
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
