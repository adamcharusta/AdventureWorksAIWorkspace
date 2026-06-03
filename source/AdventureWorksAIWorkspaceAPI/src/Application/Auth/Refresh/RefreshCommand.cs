using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;

namespace AdventureWorksAIWorkspaceAPI.Application.Auth.Refresh;

public sealed record RefreshCommand(string RefreshToken);

public static class RefreshCommandHandler
{
    public static async Task<RefreshResponse> Handle(
        RefreshCommand command,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        AuthTokens? tokens = await authenticationService.RefreshAsync(command.RefreshToken, cancellationToken);

        if (tokens is null)
        {
            throw new UnauthorizedException("Invalid or expired refresh token.");
        }

        return new RefreshResponse(
            tokens.AccessToken,
            tokens.AccessTokenExpiresAt,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiresAt);
    }
}
