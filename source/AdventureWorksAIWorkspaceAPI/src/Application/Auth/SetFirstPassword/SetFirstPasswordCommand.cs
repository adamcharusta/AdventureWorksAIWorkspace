using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;

namespace AdventureWorksAIWorkspaceAPI.Application.Auth.SetFirstPassword;

public sealed record SetFirstPasswordCommand(string Identifier, string ConfirmNewPassword, string NewPassword);

public static class SetFirstPasswordCommandHandler
{
    public static async Task<SetFirstPasswordResponse> Handle(
        SetFirstPasswordCommand command,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        SetFirstPasswordResult result = await userService.SetFirstPasswordAsync(
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
