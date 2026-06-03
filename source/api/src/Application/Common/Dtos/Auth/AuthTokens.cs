namespace AdventureWorksAIWorkspace.Application.Common.Dtos.Auth;

public sealed record AuthTokens(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);
