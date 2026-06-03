using FluentValidation;

namespace AdventureWorksAIWorkspace.Application.Auth.SetFirstPassword;

public sealed class SetFirstPasswordCommandValidator : AbstractValidator<SetFirstPasswordCommand>
{
    public SetFirstPasswordCommandValidator()
    {
        RuleFor(command => command.Identifier).NotEmpty().MaximumLength(256);
        RuleFor(command => command.ConfirmNewPassword).NotEmpty()
            .Equal(command => command.NewPassword).WithMessage("Password confirmation must match the new password.");
        RuleFor(command => command.NewPassword).NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
    }
}
