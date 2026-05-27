using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace AdventureWorksAIWorkspaceAPI.Api.Endpoints;

public static class HealthEndpoint
{
    [WolverineGet(
        "/health",
        Name = "GetHealth",
        OperationId = "GetHealth",
        Summary = "Checks the health status of the API.",
        Description = "Returns the health status of the API and its dependencies.")]
    [Tags("Health")]
    [ProducesResponseType<HealthStatus>(StatusCodes.Status200OK)]
    public static Ok<HealthStatus> Get()
    {
        return TypedResults.Ok(new HealthStatus { Status = "Healthy" });
    }
}

public class HealthStatus
{
    public string Status { get; set; } = string.Empty;
}
