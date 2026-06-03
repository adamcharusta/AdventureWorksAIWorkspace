using AdventureWorksAIWorkspace.Application.Common.Dtos;
using AdventureWorksAIWorkspace.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.Auth;

namespace AdventureWorksAIWorkspace.Application.Auth.Refresh;

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
