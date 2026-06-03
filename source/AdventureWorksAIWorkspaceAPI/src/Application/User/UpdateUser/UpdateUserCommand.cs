using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;

namespace AdventureWorksAIWorkspaceAPI.Application.User.UpdateUser;

public sealed record UpdateUserCommand(
    string UserId,
    string? UserName = null,
    string? Email = null,
    string? Role = null,
    bool ResetPassword = false);

public static class UpdateUserCommandHandler
{
    public static async Task<UpdateUserResponse> Handle(
        UpdateUserCommand command,
        IUserManagementService userManagementService,
        CancellationToken cancellationToken)
    {
        UpdateUserResult result = await userManagementService.UpdateUserAsync(command, cancellationToken);

        return result.Outcome switch
        {
            UpdateUserOutcome.Success => new UpdateUserResponse(
                result.UserId!, result.UserName!, result.Email!, result.Role!),

            UpdateUserOutcome.UserNotFound =>
                throw new NotFoundException($"User with ID '{command.UserId}' was not found."),

            UpdateUserOutcome.InvalidRole =>
                throw new ForbiddenException($"The role '{command.Role}' is not valid."),

            UpdateUserOutcome.UserNameAlreadyTaken =>
                throw new ForbiddenException($"The user name '{command.UserName}' is already taken."),

            UpdateUserOutcome.EmailAlreadyTaken =>
                throw new ForbiddenException($"The email '{command.Email}' is already taken."),

            UpdateUserOutcome.UpdateFailed =>
                throw new ForbiddenException(string.Join(" ", result.Errors ?? [])),

            _ => throw new InvalidOperationException($"Unexpected outcome: {result.Outcome}")
        };
    }
}
