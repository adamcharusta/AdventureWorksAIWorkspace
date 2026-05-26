using Mapster;

namespace AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;

public static class GetWeatherForecastsHandler
{
    public static GetWeatherForecastsResponse Handle(
        GetWeatherForecastsQuery query,
        IWeatherForecastProvider provider)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var forecasts = provider
            .GetForecasts(query.Days, startDate)
            .Select(forecast => forecast.Adapt<WeatherForecastDto>())
            .ToArray();

        return new GetWeatherForecastsResponse(forecasts);
    }
}
