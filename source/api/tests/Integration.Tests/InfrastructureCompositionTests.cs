using AdventureWorksAIWorkspace.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorksAIWorkspace.Integration.Tests;

public sealed class InfrastructureCompositionTests
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
    public void AddInfrastructureServices_Should_BuildAValidServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddInfrastructureServices(BuildConfiguration());

        var act = () =>
        {
            using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        };

        act.Should().NotThrow();
    }
}
