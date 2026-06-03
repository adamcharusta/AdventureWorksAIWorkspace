using AdventureWorksAIWorkspace.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspace.Application.Common.Services.Auth;
using AdventureWorksAIWorkspace.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace AdventureWorksAIWorkspace.Infrastructure.Services.Auth;

/// <summary>
/// Default <see cref="IAuthenticationService"/>: validates credentials and delegates token issuance
/// to <see cref="ITokenService"/>.
/// </summary>
internal sealed class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService) : IAuthenticationService
{
    public async Task<LoginResult> LoginAsync(string identifier, string password,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByNameAsync(identifier)
                                ?? await userManager.FindByEmailAsync(identifier);

        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            return new LoginResult(LoginOutcome.InvalidCredentials, null);
        }

        if (user.IsFirstLogin)
        {
            return new LoginResult(LoginOutcome.PasswordChangeRequired, null);
        }

        AuthTokens tokens = await tokenService.IssueTokensAsync(user, cancellationToken);
        return new LoginResult(LoginOutcome.Success, tokens);
    }

    public Task<AuthTokens?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default) =>
        tokenService.RefreshAsync(refreshToken, cancellationToken);

    public async Task<SetFirstPasswordResult> SetFirstPasswordAsync(string identifier, string confirmNewPassword,
        string newPassword, CancellationToken cancellationToken = default)
    {
        if (confirmNewPassword != newPassword)
        {
            return new SetFirstPasswordResult(SetFirstPasswordOutcome.PasswordChangeFailed, null,
                ["New password and confirmation do not match."]);
        }

        ApplicationUser? user = await userManager.FindByNameAsync(identifier)
                                ?? await userManager.FindByEmailAsync(identifier);

        if (user is null)
        {
            return new SetFirstPasswordResult(SetFirstPasswordOutcome.InvalidCredentials, null);
        }

        if (!user.IsFirstLogin)
        {
            return new SetFirstPasswordResult(SetFirstPasswordOutcome.PasswordChangeNotRequired, null);
        }

        string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        IdentityResult resetResult = await userManager.ResetPasswordAsync(user, resetToken, newPassword);

        if (!resetResult.Succeeded)
        {
            var errors = resetResult.Errors.Select(e => e.Description).ToList();
            return new SetFirstPasswordResult(SetFirstPasswordOutcome.PasswordChangeFailed, null, errors);
        }

        user.IsFirstLogin = false;
        await userManager.UpdateAsync(user);

        await tokenService.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);

        AuthTokens tokens = await tokenService.IssueTokensAsync(user, cancellationToken);
        return new SetFirstPasswordResult(SetFirstPasswordOutcome.Success, tokens);
    }
}
