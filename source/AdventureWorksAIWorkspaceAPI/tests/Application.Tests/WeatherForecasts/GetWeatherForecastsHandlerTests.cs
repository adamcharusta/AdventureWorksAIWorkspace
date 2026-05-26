using AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;
using AdventureWorksAIWorkspaceAPI.Domain.WeatherForecasts;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.WeatherForecasts;

public sealed class GetWeatherForecastsHandlerTests
{
    [Fact]
    public void Handle_Should_ReturnMappedForecastsFromProvider()
    {
        var provider = Substitute.For<IWeatherForecastProvider>();
        provider
            .GetForecasts(2, Arg.Any<DateOnly>())
            .Returns(
            [
                new WeatherForecast(new DateOnly(2026, 5, 27), 10, "Cool"),
                new WeatherForecast(new DateOnly(2026, 5, 28), 20, "Warm")
            ]);

        var response = GetWeatherForecastsHandler.Handle(
            new GetWeatherForecastsQuery(2),
            provider);

        response.Forecasts.Should().HaveCount(2);
        response.Forecasts[0].Should().BeEquivalentTo(
            new WeatherForecastDto(new DateOnly(2026, 5, 27), 10, 49, "Cool"));
    }
}
