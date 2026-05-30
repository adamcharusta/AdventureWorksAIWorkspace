using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.Reports;
using AdventureWorksAIWorkspaceAPI.Application.Reports.AddReportMessage;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wolverine;

namespace AdventureWorksAIWorkspaceAPI.Functional.Tests;

/// <summary>
/// Guards the Wolverine code-generation configuration. Wolverine builds a handler's code on first
/// invocation; a dependency it can only obtain via service location (for example an AI service that
/// depends on the opaque <c>IAiChatClient</c> HttpClient factory) throws
/// <c>InvalidServiceLocationException</c> during that build unless the service is listed in
/// <c>CodeGeneration.AlwaysUseServiceLocationFor</c>. The handler unit tests call the static handler
/// directly, so they never exercise the generated code where this fails.
/// </summary>
public sealed class WolverineCodegenTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public WolverineCodegenTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task InvokingAddReportMessage_ShouldGenerateHandlerCodeWithoutServiceLocationError()
    {
        // The repository is stubbed to return no report so the handler short-circuits with a
        // NotFoundException before reaching the database or any AI call. This still forces Wolverine
        // to generate the full handler — including resolving every AI dependency — which is where a
        // missing AlwaysUseServiceLocationFor registration would throw InvalidServiceLocationException.
        using var stubbedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var repository = Substitute.For<IReportRepository>();
                repository
                    .GetOwnedReportAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                    .Returns((Report?)null);

                services.RemoveAll<IReportRepository>();
                services.AddScoped(_ => repository);
            });
        });

        using var scope = stubbedFactory.Services.CreateScope();
        var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

        var act = () => messageBus.InvokeAsync<ReportChatResponse>(
            new AddReportMessageCommand("missing-report", "any follow-up message", "user-1"));

        // Reaching the NotFoundException proves the handler code generated and ran. A service
        // location failure would instead surface as InvalidServiceLocationException.
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
