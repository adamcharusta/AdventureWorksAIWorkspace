import Alert from '@mui/material/Alert'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CircularProgress from '@mui/material/CircularProgress'
import Container from '@mui/material/Container'
import Stack from '@mui/material/Stack'
import Typography from '@mui/material/Typography'
import { LineChart } from '@mui/x-charts/LineChart'

import { useGetWeatherForecasts } from './api/generated/weather-forecasts/weather-forecasts'
import { toast } from './lib/toast'

function App() {
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

        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Toast playground
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
              Each button fires a sonner toast that renders an MUI Alert through
              the shared `toast` wrapper.
            </Typography>
            <Stack
              direction="row"
              spacing={1}
              useFlexGap
              sx={{ flexWrap: 'wrap' }}
            >
              <Button
                variant="contained"
                color="success"
                onClick={() =>
                  toast.success('Report saved to your workspace.', 'Saved')
                }
              >
                Success
              </Button>
              <Button
                variant="contained"
                color="error"
                onClick={() =>
                  toast.error(
                    'SQL validation rejected destructive commands.',
                    'Blocked',
                  )
                }
              >
                Error
              </Button>
              <Button
                variant="contained"
                color="warning"
                onClick={() =>
                  toast.warning(
                    'Query returned more than 1000 rows.',
                    'Heads up',
                  )
                }
              >
                Warning
              </Button>
              <Button
                variant="contained"
                color="info"
                onClick={() =>
                  toast.info('AI is generating SQL…', 'Working on it')
                }
              >
                Info
              </Button>
            </Stack>
          </CardContent>
        </Card>
      </Stack>
    </Container>
  )
}

export default App
