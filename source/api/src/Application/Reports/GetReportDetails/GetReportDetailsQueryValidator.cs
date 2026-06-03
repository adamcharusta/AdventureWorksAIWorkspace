using FluentValidation;

namespace AdventureWorksAIWorkspace.Application.Reports.GetReportDetails;

public sealed class GetReportDetailsQueryValidator : AbstractValidator<GetReportDetailsQuery>
{
    public GetReportDetailsQueryValidator()
    {
        RuleFor(query => query.ReportId).NotEmpty();
    }
}
