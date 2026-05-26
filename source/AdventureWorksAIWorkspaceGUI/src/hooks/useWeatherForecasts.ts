import { useQuery } from '@tanstack/react-query'

import { apiClient } from '../api/client'

// Local DTO mirror. Once the backend restart regenerates schema.d.ts with the
// concrete response type (Ok<IReadOnlyList<WeatherForecastDto>>), the cast in
// queryFn can be removed and this type swapped for one inferred from `paths`.
export type WeatherForecast = {
  date: string
  temperatureC: number
  temperatureF: number
  summary: string
}

export const useWeatherForecasts = (days = 5) =>
  useQuery({
    queryKey: ['weather-forecasts', days],
    queryFn: async () => {
      const { data, error } = await apiClient.GET('/api/weather-forecasts', {
        params: { query: { days } },
      })
      if (error) throw error
      return data as unknown as WeatherForecast[]
    },
  })
