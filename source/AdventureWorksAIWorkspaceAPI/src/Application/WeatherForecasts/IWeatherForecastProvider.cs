using AdventureWorksAIWorkspaceAPI.Domain.WeatherForecasts;

namespace AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;

public interface IWeatherForecastProvider
{
    IReadOnlyList<WeatherForecast> GetForecasts(int days, DateOnly startDate);
}
