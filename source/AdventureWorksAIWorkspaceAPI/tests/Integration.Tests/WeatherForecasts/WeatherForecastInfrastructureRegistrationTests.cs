using AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;
using AdventureWorksAIWorkspaceAPI.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorksAIWorkspaceAPI.Integration.Tests.WeatherForecasts;

public sealed class WeatherForecastInfrastructureRegistrationTests
{
    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AdventureWorksAIWorkspaceDatabase"] =
                    "Server=localhost;Database=AppDb;User Id=sa;Password=Test_Password_123!;TrustServerCertificate=True;",
                ["ConnectionStrings:AdventureWorksDatabase"] =
                    "Server=localhost;Database=AdventureWorks2025;User Id=readonly;Password=Test_Password_123!;TrustServerCertificate=True;ApplicationIntent=ReadOnly;",
                ["Identity:Jwt:Issuer"] = "test-issuer",
                ["Identity:Jwt:Audience"] = "test-audience",
                ["Identity:Jwt:SigningKey"] = "test-signing-key-with-at-least-32-characters",
            })
            .Build();
    }

    [Fact]
    public void AddInfrastructureServices_Should_RegisterWeatherForecastProvider()
    {
        var services = new ServiceCollection();

        services.AddInfrastructureServices(BuildConfiguration());

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<IWeatherForecastProvider>();
        var forecasts = provider.GetForecasts(2, new DateOnly(2026, 5, 26));

        forecasts.Should().HaveCount(2);
    }
}
