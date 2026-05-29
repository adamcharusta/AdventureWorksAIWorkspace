using FluentValidation;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports.CreateReport;

public sealed class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
{
    public CreateReportCommandValidator()
    {
        RuleFor(command => command.Message)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
