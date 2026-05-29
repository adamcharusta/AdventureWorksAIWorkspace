using FluentValidation;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports.GenerateReport;

public sealed class GenerateReportCommandValidator : AbstractValidator<GenerateReportCommand>
{
    public GenerateReportCommandValidator()
    {
        RuleFor(command => command.Question)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
