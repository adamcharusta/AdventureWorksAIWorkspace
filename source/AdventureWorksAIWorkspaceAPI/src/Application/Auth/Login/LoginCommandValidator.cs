using FluentValidation;

namespace AdventureWorksAIWorkspaceAPI.Application.Auth.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Identifier).NotEmpty().MaximumLength(256);
        RuleFor(command => command.Password).NotEmpty();
    }
}
