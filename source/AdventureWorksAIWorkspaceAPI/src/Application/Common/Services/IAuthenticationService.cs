using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Auth;

namespace AdventureWorksAIWorkspaceAPI.Application.Common.Services;

/// <summary>
/// Authenticates users and issues session tokens: sign-in, refresh, and the forced first-login
/// password change.
/// </summary>
public interface IAuthenticationService
{
    Task<LoginResult> LoginAsync(string identifier, string password, CancellationToken cancellationToken = default);

    Task<AuthTokens?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<SetFirstPasswordResult> SetFirstPasswordAsync(string identifier, string confirmNewPassword, string newPassword,
        CancellationToken cancellationToken = default);
}
