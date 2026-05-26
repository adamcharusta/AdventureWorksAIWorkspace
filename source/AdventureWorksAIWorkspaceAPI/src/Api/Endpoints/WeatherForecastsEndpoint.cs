using AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace AdventureWorksAIWorkspaceAPI.Api.Endpoints;

public static class WeatherForecastsEndpoint
{
    [WolverineGet(
        "/api/weather-forecasts",
        Name = "GetWeatherForecasts",
        OperationId = "GetWeatherForecasts",
        Summary = "Returns sample weather forecasts.",
        Description = "Returns deterministic sample weather forecast data through the CQRS application flow.")]
    [Tags("WeatherForecasts")]
    [ProducesResponseType<IReadOnlyList<WeatherForecastDto>>(StatusCodes.Status200OK)]
    public static async Task<Ok<IReadOnlyList<WeatherForecastDto>>> Get(
        IMessageBus messageBus,
        int days = 5,
        CancellationToken cancellationToken = default)
    {
        var response = await messageBus.InvokeAsync<GetWeatherForecastsResponse>(
            new GetWeatherForecastsQuery(days),
            cancellationToken);

        return TypedResults.Ok(response.Forecasts);
    }
}
