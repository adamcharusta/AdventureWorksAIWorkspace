using AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;
using AdventureWorksAIWorkspaceAPI.Infrastructure.WeatherForecasts;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Guard.Against.Null(services);
        Guard.Against.Null(configuration);

        services.AddSingleton<IWeatherForecastProvider, SampleWeatherForecastProvider>();

        return services;
    }
}
