using Ardalis.GuardClauses;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Wolverine.FluentValidation;

namespace AdventureWorksAIWorkspaceAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        Guard.Against.Null(services);

        services.AddSingleton(TypeAdapterConfig.GlobalSettings);

        return services;
    }

    public static WolverineOptions AddApplicationServices(this WolverineOptions options)
    {
        Guard.Against.Null(options);

        options.Discovery.IncludeAssembly(typeof(ApplicationAssembly).Assembly);
        options.UseFluentValidation();

        return options;
    }
}
