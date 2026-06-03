using AdventureWorksAIWorkspace.Application.Common.Dtos;
using AdventureWorksAIWorkspace.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.Auth;

namespace AdventureWorksAIWorkspace.Application.Auth.SetFirstPassword;

public sealed record SetFirstPasswordCommand(string Identifier, string ConfirmNewPassword, string NewPassword);

public static class SetFirstPasswordCommandHandler
{
    public static async Task<SetFirstPasswordResponse> Handle(
        SetFirstPasswordCommand command,
        IAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        SetFirstPasswordResult result = await authenticationService.SetFirstPasswordAsync(
            command.Identifier, command.ConfirmNewPassword, command.NewPassword, cancellationToken);

        return result.Outcome switch
        {
            SetFirstPasswordOutcome.Success => new SetFirstPasswordResponse(
                result.Tokens!.AccessToken,
                result.Tokens.AccessTokenExpiresAt,
                result.Tokens.RefreshToken,
                result.Tokens.RefreshTokenExpiresAt),

            SetFirstPasswordOutcome.InvalidCredentials =>
                throw new UnauthorizedException("Invalid user name, email, or password."),

            SetFirstPasswordOutcome.PasswordChangeNotRequired =>
                throw new ForbiddenException("This account does not require a first-login password change."),

            SetFirstPasswordOutcome.PasswordChangeFailed =>
                throw new ForbiddenException(
                    string.Join(" ", result.Errors ?? [])),

            _ => throw new InvalidOperationException($"Unexpected outcome: {result.Outcome}")
        };
    }
}
