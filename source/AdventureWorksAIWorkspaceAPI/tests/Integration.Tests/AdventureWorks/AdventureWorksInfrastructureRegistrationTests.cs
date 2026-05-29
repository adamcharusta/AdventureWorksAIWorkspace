using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdventureWorksAIWorkspaceAPI.Integration.Tests.AdventureWorks;

public sealed class AdventureWorksInfrastructureRegistrationTests
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
    public void AddInfrastructureServices_Should_RegisterAdventureWorksQueryExecutor()
    {
        var services = new ServiceCollection();

        services.AddInfrastructureServices(BuildConfiguration());

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();

        var executor = scope.ServiceProvider.GetRequiredService<IAdventureWorksQueryExecutor>();

        executor.Should().NotBeNull();
        executor.Should().BeOfType<Infrastructure.Services.DapperAdventureWorksQueryExecutor>();
    }
}
