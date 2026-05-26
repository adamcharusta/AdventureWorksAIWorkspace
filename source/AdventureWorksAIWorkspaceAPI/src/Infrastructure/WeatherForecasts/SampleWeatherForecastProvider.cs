using AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;
using AdventureWorksAIWorkspaceAPI.Domain.WeatherForecasts;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.WeatherForecasts;

public sealed class SampleWeatherForecastProvider : IWeatherForecastProvider
{
    private static readonly string[] Summaries =
    [
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    ];

    public IReadOnlyList<WeatherForecast> GetForecasts(int days, DateOnly startDate)
    {
        return Enumerable
            .Range(1, days)
            .Select(CreateForecast)
            .ToArray();

        WeatherForecast CreateForecast(int dayOffset)
        {
            var summary = Summaries[(dayOffset - 1) % Summaries.Length];
            var temperatureC = -10 + dayOffset * 4;

            return new WeatherForecast(startDate.AddDays(dayOffset), temperatureC, summary);
        }
    }
}
