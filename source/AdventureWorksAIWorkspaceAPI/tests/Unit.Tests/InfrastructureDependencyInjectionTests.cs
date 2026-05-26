using AdventureWorksAIWorkspaceAPI.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorksAIWorkspaceAPI.Unit.Tests;

public sealed class InfrastructureDependencyInjectionTests
{
    [Fact]
    public void AddInfrastructureServices_Should_ReturnTheSameServiceCollection()
    {
        var services = new ServiceCollection();
        var configuration = Substitute.For<IConfiguration>();

        var result = services.AddInfrastructureServices(configuration);

        result.Should().BeSameAs(services);
    }
}
