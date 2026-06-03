using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AdventureWorksAIWorkspace.Functional.Tests;

public sealed class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task GetRoot_WhenNoEndpointIsRegistered_ShouldReturnNotFound()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSwaggerDocument_InDevelopment_ShouldReturnOpenApiDocument()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSwaggerDocument_ShouldDescribeHealthEndpoint()
    {
        using var client = factory.CreateClient();

        using var document = await JsonDocument.ParseAsync(
            await client.GetStreamAsync("/swagger/v1/swagger.json"));

        document.RootElement
            .GetProperty("paths")
            .TryGetProperty("/health", out _)
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnHealthy()
    {
        using var client = factory.CreateClient();

        var health = await client.GetFromJsonAsync<HealthStatus>("/health");

        health.Should().NotBeNull();
        health!.Status.Should().Be("Healthy");
    }

    private sealed record HealthStatus(string Status);
}
