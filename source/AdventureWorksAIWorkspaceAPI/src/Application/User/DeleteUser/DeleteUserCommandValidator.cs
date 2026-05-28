using FluentValidation;

namespace AdventureWorksAIWorkspaceAPI.Application.User.DeleteUser;

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
    }
}
