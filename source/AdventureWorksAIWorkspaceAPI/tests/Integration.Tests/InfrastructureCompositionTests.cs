using AdventureWorksAIWorkspaceAPI.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorksAIWorkspaceAPI.Integration.Tests;

public sealed class InfrastructureCompositionTests
{
    [Fact]
    public void AddInfrastructureServices_Should_BuildAValidServiceProvider()
    {
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();

        services.AddInfrastructureServices(configuration);

        var act = () =>
        {
            using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        };

        act.Should().NotThrow();
    }
}
