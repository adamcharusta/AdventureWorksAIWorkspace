namespace AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;

public enum DeleteUserOutcome
{
    Success,
    UserNotFound,
    DeleteFailed
}

public sealed record DeleteUserResult(
    DeleteUserOutcome Outcome,
    IReadOnlyList<string>? Errors = null);
