using FluentValidation;

namespace AdventureWorksAIWorkspaceAPI.Application.User.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.UserName).NotEmpty().MaximumLength(256);
        RuleFor(command => command.Email).NotEmpty().MaximumLength(256).EmailAddress();
        RuleFor(command => command.Role).MaximumLength(64)
            .When(command => command.Role is not null);
    }
}
