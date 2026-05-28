using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;

namespace AdventureWorksAIWorkspaceAPI.Application.User.DeleteUser;

public sealed record DeleteUserCommand(string UserId, string? CurrentUserId);

public static class DeleteUserCommandHandler
{
    public static async Task Handle(
        DeleteUserCommand command,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.CurrentUserId))
        {
            throw new UnauthorizedException("Authenticated user identifier is missing.");
        }

        if (string.Equals(command.UserId, command.CurrentUserId, StringComparison.Ordinal))
        {
            throw new ForbiddenException("Administrators cannot delete their own user account.");
        }

        DeleteUserResult result = await userService.DeleteUserAsync(command.UserId, cancellationToken);

        switch (result.Outcome)
        {
            case DeleteUserOutcome.Success:
                return;

            case DeleteUserOutcome.UserNotFound:
                throw new NotFoundException($"User with ID '{command.UserId}' was not found.");

            case DeleteUserOutcome.DeleteFailed:
                throw new ForbiddenException(GetFailureMessage(result));

            default:
                throw new InvalidOperationException($"Unexpected outcome: {result.Outcome}");
        }
    }

    private static string GetFailureMessage(DeleteUserResult result) =>
        result.Errors is { Count: > 0 }
            ? string.Join(" ", result.Errors)
            : "The user account could not be deleted.";
}
