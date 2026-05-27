import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Typography from '@mui/material/Typography'
import { LineChart } from '@mui/x-charts/LineChart'

import type { WeatherForecastDto } from '../api/generated/model'

type WeatherTrendCardProps = {
  forecasts: WeatherForecastDto[]
}

export function WeatherTrendCard({ forecasts }: WeatherTrendCardProps) {
  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom>
          Temperature trend
        </Typography>
        <LineChart
          xAxis={[
            {
              data: forecasts.map((forecast) => forecast.date),
              scaleType: 'point',
              label: 'Date',
            },
          ]}
          series={[
            {
              data: forecasts.map((forecast) => forecast.temperatureC),
              label: '°C',
            },
            {
              data: forecasts.map((forecast) => forecast.temperatureF),
              label: '°F',
            },
          ]}
          height={300}
        />
      </CardContent>
    </Card>
  )
}
