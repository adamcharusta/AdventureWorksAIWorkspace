namespace AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;

public enum CreateUserOutcome
{
    Success,
    UserAlreadyExists,
    InvalidRole,
    CreationFailed
}

public sealed record CreateUserResult(
    CreateUserOutcome Outcome,
    string? UserId = null,
    string? UserName = null,
    string? Email = null,
    string? Role = null,
    IReadOnlyList<string>? Errors = null);
