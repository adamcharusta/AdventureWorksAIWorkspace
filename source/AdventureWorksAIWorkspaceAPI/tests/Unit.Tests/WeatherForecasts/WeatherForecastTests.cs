using AdventureWorksAIWorkspaceAPI.Domain.WeatherForecasts;

namespace AdventureWorksAIWorkspaceAPI.Unit.Tests.WeatherForecasts;

public sealed class WeatherForecastTests
{
    [Fact]
    public void TemperatureF_Should_ConvertCelsiusToFahrenheit()
    {
        var forecast = new WeatherForecast(new DateOnly(2026, 5, 26), 20, "Warm");

        forecast.TemperatureF.Should().Be(67);
    }
}
