using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;

namespace AdventureWorksAIWorkspaceAPI.Application.User.CreateUser;

public sealed record CreateUserCommand(string UserName, string Email, string? Role = null);

public static class CreateUserCommandHandler
{
    public static async Task<CreateUserResponse> Handle(
        CreateUserCommand command,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        CreateUserResult result = await userService.CreateUserAsync(
            command.UserName, command.Email, command.Role, cancellationToken);

        return result.Outcome switch
        {
            CreateUserOutcome.Success => new CreateUserResponse(
                result.UserId!, result.UserName!, result.Email!, result.Role!),

            CreateUserOutcome.UserAlreadyExists =>
                throw new ForbiddenException("A user with this user name or email already exists."),

            CreateUserOutcome.CreationFailed =>
                throw new ForbiddenException(string.Join(" ", result.Errors ?? [])),

            CreateUserOutcome.InvalidRole =>
                throw new ForbiddenException($"The role '{command.Role}' is not valid."),

            _ => throw new InvalidOperationException($"Unexpected outcome: {result.Outcome}")
        };
    }
}
