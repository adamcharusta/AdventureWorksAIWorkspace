using FluentValidation;

namespace AdventureWorksAIWorkspaceAPI.Application.Auth.Refresh;

public sealed class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty();
    }
}
