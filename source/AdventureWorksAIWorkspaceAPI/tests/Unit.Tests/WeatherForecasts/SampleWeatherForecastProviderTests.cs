using AdventureWorksAIWorkspaceAPI.Infrastructure.WeatherForecasts;

namespace AdventureWorksAIWorkspaceAPI.Unit.Tests.WeatherForecasts;

public sealed class SampleWeatherForecastProviderTests
{
    [Fact]
    public void GetForecasts_Should_ReturnDeterministicForecastsForRequestedDays()
    {
        var provider = new SampleWeatherForecastProvider();
        var startDate = new DateOnly(2026, 5, 26);

        var forecasts = provider.GetForecasts(3, startDate);

        forecasts.Should().HaveCount(3);
        forecasts.Select(forecast => forecast.Date).Should().Equal(
            new DateOnly(2026, 5, 27),
            new DateOnly(2026, 5, 28),
            new DateOnly(2026, 5, 29));
        forecasts.Select(forecast => forecast.Summary).Should().Equal("Freezing", "Bracing", "Chilly");
    }
}
