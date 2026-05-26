using AdventureWorksAIWorkspaceAPI.Application.WeatherForecasts;
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
    public static async Task<IResult> Get(
        IMessageBus messageBus,
        int days = 5,
        CancellationToken cancellationToken = default)
    {
        var response = await messageBus.InvokeAsync<GetWeatherForecastsResponse>(
            new GetWeatherForecastsQuery(days),
            cancellationToken);

        return Results.Ok(response.Forecasts);
    }
}
