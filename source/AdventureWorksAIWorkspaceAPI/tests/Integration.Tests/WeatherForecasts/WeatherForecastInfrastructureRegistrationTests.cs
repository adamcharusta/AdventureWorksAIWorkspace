using AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;
using AdventureWorksAIWorkspaceAPI.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorksAIWorkspaceAPI.Integration.Tests.WeatherForecasts;

public sealed class WeatherForecastInfrastructureRegistrationTests
{
    [Fact]
    public void AddInfrastructureServices_Should_RegisterWeatherForecastProvider()
    {
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();

        services.AddInfrastructureServices(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<IWeatherForecastProvider>();
        var forecasts = provider.GetForecasts(2, new DateOnly(2026, 5, 26));

        forecasts.Should().HaveCount(2);
    }
}
