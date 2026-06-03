using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.Reports;
using AdventureWorksAIWorkspace.Domain.Reports;

namespace AdventureWorksAIWorkspace.Application.Reports.GetReportDetails;

public sealed record GetReportDetailsQuery(string ReportId, string? CurrentUserId);

public static class GetReportDetailsQueryHandler
{
    public static async Task<ReportDetailsDto> Handle(
        GetReportDetailsQuery query,
        IReportRepository reportRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.CurrentUserId))
        {
            throw new UnauthorizedException("Authenticated user identifier is missing.");
        }

        Report report = await reportRepository.GetOwnedReportAsync(
            query.ReportId,
            query.CurrentUserId,
            cancellationToken)
            ?? throw new NotFoundException($"Report with ID '{query.ReportId}' was not found.");

        return ReportMapping.ToDetailsDto(report);
    }
}
