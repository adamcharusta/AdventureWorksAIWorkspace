using FluentValidation;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports.AddReportMessage;

public sealed class AddReportMessageCommandValidator : AbstractValidator<AddReportMessageCommand>
{
    public AddReportMessageCommandValidator()
    {
        RuleFor(command => command.ReportId).NotEmpty();
        RuleFor(command => command.Message)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
