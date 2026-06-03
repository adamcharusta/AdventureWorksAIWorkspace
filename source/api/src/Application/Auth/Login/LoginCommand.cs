using AdventureWorksAIWorkspace.Application.Common.Dtos;
using AdventureWorksAIWorkspace.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.Auth;

namespace AdventureWorksAIWorkspace.Application.Auth.Login;

public sealed record LoginCommand(string Identifier, string Password);

public static class LoginCommandHandler
{
    public static async Task<LoginResponse> Handle(
        LoginCommand command,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        LoginResult result = await authenticationService.LoginAsync(command.Identifier, command.Password, cancellationToken);

        return result.Outcome switch
        {
            LoginOutcome.Success => new LoginResponse(
                result.Tokens!.AccessToken,
                result.Tokens.AccessTokenExpiresAt,
                result.Tokens.RefreshToken,
                result.Tokens.RefreshTokenExpiresAt),

            LoginOutcome.InvalidCredentials =>
                throw new UnauthorizedException("Invalid user name, email, or password."),

            LoginOutcome.PasswordChangeRequired =>
                throw new ForbiddenException(
                    "Password change is required before the account can be used. Complete the first-login password change flow."),

            _ => throw new InvalidOperationException($"Unexpected login outcome: {result.Outcome}")
        };
    }
}
