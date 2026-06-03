using FluentValidation;

namespace AdventureWorksAIWorkspace.Application.User.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.UserName).MaximumLength(256)
            .When(command => command.UserName is not null);
        RuleFor(command => command.Email).MaximumLength(256).EmailAddress()
            .When(command => command.Email is not null);
        RuleFor(command => command.Role).MaximumLength(64)
            .When(command => command.Role is not null);
    }
}
