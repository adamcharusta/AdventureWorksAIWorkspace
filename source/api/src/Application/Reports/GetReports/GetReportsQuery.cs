using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.Reports;

namespace AdventureWorksAIWorkspace.Application.Reports.GetReports;

public sealed record GetReportsQuery(string? CurrentUserId);

public sealed record GetReportsResponse(IReadOnlyList<ReportSummaryDto> Reports);

public static class GetReportsQueryHandler
{
    public static async Task<GetReportsResponse> Handle(
        GetReportsQuery query,
        IReportRepository reportRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.CurrentUserId))
        {
            throw new UnauthorizedException("Authenticated user identifier is missing.");
        }

        var reports = await reportRepository.GetReportsForUserAsync(query.CurrentUserId, cancellationToken);
        return new GetReportsResponse(reports.Select(ReportMapping.ToSummaryDto).ToList());
    }
}
