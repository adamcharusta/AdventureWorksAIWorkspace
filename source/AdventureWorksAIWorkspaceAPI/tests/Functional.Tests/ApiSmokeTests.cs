using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;
using AdventureWorksAIWorkspaceAPI.Domain.WeatherForecasts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AdventureWorksAIWorkspaceAPI.Functional.Tests;

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
    public async Task GetWeatherForecasts_WhenDaysIsValid_ShouldReturnForecasts()
    {
        using var client = factory.CreateClient();

        var forecasts = await client.GetFromJsonAsync<WeatherForecastContract[]>("/api/weather-forecasts?days=3");

        forecasts.Should().NotBeNull();
        forecasts.Should().HaveCount(3);
        forecasts![0].Summary.Should().Be("Freezing");
    }

    [Fact]
    public async Task GetWeatherForecasts_WhenDaysIsInvalid_ShouldReturnValidationProblem()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/weather-forecasts?days=0");
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        root.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status400BadRequest);
        root.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.1");
        root.GetProperty("title").GetString().Should().Be("One or more validation errors occurred.");
        root.GetProperty("errors")
            .TryGetProperty(nameof(GetWeatherForecastsQuery.Days), out _)
            .Should()
            .BeTrue();
    }

    [Fact]
    public async Task GetWeatherForecasts_WhenApplicationThrowsNotFound_ShouldReturnProblemDetails()
    {
        var notFoundFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                var provider = Substitute.For<IWeatherForecastProvider>();
                provider
                    .GetForecasts(Arg.Any<int>(), Arg.Any<DateOnly>())
                    .Returns(_ => throw new NotFoundException("Weather forecast was not found."));

                services.RemoveAll<IWeatherForecastProvider>();
                services.AddSingleton(provider);
            });
        });

        using var client = notFoundFactory.CreateClient();

        var response = await client.GetAsync("/api/weather-forecasts?days=3");
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        root.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status404NotFound);
        root.GetProperty("type").GetString().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.5");
        root.GetProperty("title").GetString().Should().Be("The specified resource was not found.");
        root.GetProperty("detail").GetString().Should().Be("Weather forecast was not found.");
    }

    [Fact]
    public async Task GetSwaggerDocument_ShouldDescribeWeatherForecastEndpoint()
    {
        using var client = factory.CreateClient();

        using var document = await JsonDocument.ParseAsync(
            await client.GetStreamAsync("/swagger/v1/swagger.json"));

        document.RootElement
            .GetProperty("paths")
            .TryGetProperty("/api/weather-forecasts", out _)
            .Should()
            .BeTrue();
    }

    private sealed record WeatherForecastContract(
        DateOnly Date,
        int TemperatureC,
        int TemperatureF,
        string Summary);
}
