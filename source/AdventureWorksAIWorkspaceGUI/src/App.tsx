import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CircularProgress from '@mui/material/CircularProgress'
import Container from '@mui/material/Container'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { LineChart } from '@mui/x-charts/LineChart'

import { useWeatherForecasts } from './hooks/useWeatherForecasts'

function App() {
  const { data, isLoading, error } = useWeatherForecasts(7)

  return (
    <Container maxWidth="md" sx={{ py: 6 }}>
      <Stack spacing={3}>
        <Box>
          <Typography variant="h4" component="h1" gutterBottom>
            Weather forecasts
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Demo of GUI ↔ API connectivity via TanStack Query + openapi-fetch.
          </Typography>
        </Box>

        {isLoading && (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
            <CircularProgress />
          </Box>
        )}

        {error && (
          <Alert severity="error">
            Failed to load forecasts: {(error as Error).message}
          </Alert>
        )}

        {data && data.length > 0 && (
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Temperature trend
              </Typography>
              <LineChart
                xAxis={[
                  {
                    data: data.map((f) => f.date),
                    scaleType: 'point',
                    label: 'Date',
                  },
                ]}
                series={[
                  { data: data.map((f) => f.temperatureC), label: '°C' },
                  { data: data.map((f) => f.temperatureF), label: '°F' },
                ]}
                height={300}
              />
            </CardContent>
          </Card>
        )}
      </Stack>
    </Container>
  )
}

export default App
