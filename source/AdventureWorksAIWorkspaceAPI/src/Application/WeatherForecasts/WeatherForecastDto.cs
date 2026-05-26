namespace AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;

public sealed record WeatherForecastDto(
    DateOnly Date,
    int TemperatureC,
    int TemperatureF,
    string Summary);
