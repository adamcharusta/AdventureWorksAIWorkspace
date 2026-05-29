using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports.RenameReport;

public sealed record RenameReportCommand(string ReportId, string Title, string? CurrentUserId = null);

public sealed record RenameReportRequest(string Title);

public static class RenameReportCommandHandler
{
    public static async Task<ReportSummaryDto> Handle(
        RenameReportCommand command,
        IReportRepository reportRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.CurrentUserId))
        {
            throw new UnauthorizedException("Authenticated user identifier is missing.");
        }

        Report report = await reportRepository.GetOwnedReportAsync(
            command.ReportId,
            command.CurrentUserId,
            cancellationToken)
            ?? throw new NotFoundException($"Report with ID '{command.ReportId}' was not found.");

        report.Title = command.Title.Trim();
        report.UpdatedAt = DateTime.UtcNow;

        await reportRepository.SaveChangesAsync(cancellationToken);

        return ReportMapping.ToSummaryDto(report);
    }
}
