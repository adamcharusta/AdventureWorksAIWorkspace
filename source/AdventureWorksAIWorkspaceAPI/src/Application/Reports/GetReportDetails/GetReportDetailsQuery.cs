using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports.GetReportDetails;

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
