using AdventureWorksAIWorkspace.Application;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorksAIWorkspace.Application.Tests;

public sealed class ApplicationServiceRegistrationTests
{
    [Fact]
    public void AddApplicationServices_Should_RegisterMapsterConfiguration()
    {
        var services = new ServiceCollection();

        services.AddApplicationServices();

        using var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<TypeAdapterConfig>();

        config.Should().NotBeNull();
    }
}
