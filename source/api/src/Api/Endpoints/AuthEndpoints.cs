using AdventureWorksAIWorkspace.Application.Auth.Login;
using AdventureWorksAIWorkspace.Application.Auth.Refresh;
using AdventureWorksAIWorkspace.Application.Auth.SetFirstPassword;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace AdventureWorksAIWorkspace.Api.Endpoints;

public static class AuthEndpoints
{
    [WolverinePost(
        "/api/auth/login",
        Name = "Login",
        OperationId = "Login",
        Summary = "Authenticates a user and returns access and refresh tokens.",
        Description =
            "Validates the supplied credentials (user name or email + password) and issues a JWT access token and a refresh token. Returns 401 for invalid credentials and 403 when the account requires a first-login password change.")]
    [Tags("Authentication")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<Ok<LoginResponse>> Login(
        [FromBody] LoginCommand command,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        LoginResponse response = await messageBus.InvokeAsync<LoginResponse>(command, cancellationToken);
        return TypedResults.Ok(response);
    }

    [WolverinePost(
        "/api/auth/set-first-password",
        Name = "SetFirstPassword",
        OperationId = "SetFirstPassword",
        Summary = "Sets a new password for an account that requires a first-login password change.",
        Description =
            "Accepts the user identifier, new password, and password confirmation. Only works for accounts marked as requiring a first-login password change. On success, clears the first-login flag and returns access and refresh tokens.")]
    [Tags("Authentication")]
    [ProducesResponseType<SetFirstPasswordResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public static async Task<Ok<SetFirstPasswordResponse>> SetFirstPassword(
        [FromBody] SetFirstPasswordCommand command,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        SetFirstPasswordResponse response =
            await messageBus.InvokeAsync<SetFirstPasswordResponse>(command, cancellationToken);
        return TypedResults.Ok(response);
    }

    [WolverinePost(
        "/api/auth/refresh",
        Name = "Refresh",
        OperationId = "Refresh",
        Summary = "Rotates the refresh token and returns new access and refresh tokens.",
        Description =
            "Validates the supplied refresh token, revokes it, and issues a new access token together with a new refresh token. Returns 401 if the refresh token is missing, unknown, expired, or revoked.")]
    [Tags("Authentication")]
    [ProducesResponseType<RefreshResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<Ok<RefreshResponse>> Refresh(
        [FromBody] RefreshCommand command,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        RefreshResponse response = await messageBus.InvokeAsync<RefreshResponse>(command, cancellationToken);
        return TypedResults.Ok(response);
    }
}
