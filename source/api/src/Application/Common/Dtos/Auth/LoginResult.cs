namespace AdventureWorksAIWorkspace.Application.Common.Dtos.Auth;

public enum LoginOutcome
{
    Success,
    InvalidCredentials,
    PasswordChangeRequired
}

public sealed record LoginResult(LoginOutcome Outcome, AuthTokens? Tokens);
