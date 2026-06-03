using FluentValidation;

namespace AdventureWorksAIWorkspace.Application.Reports.RenameReport;

public sealed class RenameReportCommandValidator : AbstractValidator<RenameReportCommand>
{
    public RenameReportCommandValidator()
    {
        RuleFor(command => command.ReportId)
            .NotEmpty();

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(256);
    }
}
