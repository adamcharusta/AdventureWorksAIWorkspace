namespace AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;

public enum UpdateUserOutcome
{
    Success,
    UserNotFound,
    InvalidRole,
    UserNameAlreadyTaken,
    EmailAlreadyTaken,
    UpdateFailed
}

public sealed record UpdateUserResult(
    UpdateUserOutcome Outcome,
    string? UserId = null,
    string? UserName = null,
    string? Email = null,
    string? Role = null,
    IReadOnlyList<string>? Errors = null);
