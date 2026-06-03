namespace AdventureWorksAIWorkspace.Application.Auth.SetFirstPassword;

public sealed record SetFirstPasswordResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);
