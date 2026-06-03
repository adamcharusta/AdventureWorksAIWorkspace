using AdventureWorksAIWorkspace.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspace.Infrastructure.Identity;

namespace AdventureWorksAIWorkspace.Infrastructure.Services.Auth;

/// <summary>
/// Issues and revokes access/refresh tokens. Infrastructure-internal: it works with the Identity
/// <see cref="ApplicationUser"/> and the refresh-token store, so it is not exposed to the
/// Application layer. Authentication and user-management services depend on it.
/// </summary>
internal interface ITokenService
{
    /// <summary>Issues a new access token and a persisted refresh token for the user.</summary>
    Task<AuthTokens> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken);

    /// <summary>Revokes every active refresh token for the user (used on password reset and delete).</summary>
    Task RevokeAllRefreshTokensAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Validates and rotates a refresh token, returning a fresh token pair, or <c>null</c> when the
    /// supplied token is missing, unknown, or inactive.
    /// </summary>
    Task<AuthTokens?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
}
