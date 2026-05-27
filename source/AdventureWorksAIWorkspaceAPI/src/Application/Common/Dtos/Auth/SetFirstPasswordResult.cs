namespace AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Auth;

public enum SetFirstPasswordOutcome
{
    Success,
    InvalidCredentials,
    PasswordChangeNotRequired,
    PasswordChangeFailed
}

public sealed record SetFirstPasswordResult(
    SetFirstPasswordOutcome Outcome,
    AuthTokens? Tokens,
    IReadOnlyList<string>? Errors = null);
