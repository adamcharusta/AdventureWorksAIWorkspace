using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.Reports;
using AdventureWorksAIWorkspace.Domain.Reports;

namespace AdventureWorksAIWorkspace.Application.Reports.DeleteReport;

public sealed record DeleteReportCommand(string ReportId, string? CurrentUserId = null);

public sealed record DeleteReportResponse(string ReportId);

public static class DeleteReportCommandHandler
{
    public static async Task<DeleteReportResponse> Handle(
        DeleteReportCommand command,
        IReportRepository reportRepository,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.CurrentUserId))
        {
            throw new UnauthorizedException("Authenticated user identifier is missing.");
        }

        // Load the full aggregate (conversation, messages, generated SQL) so the change tracker
        // can order the cascade deletes and break the message <-> SQL-query reference cycle.
        Report report = await reportRepository.GetOwnedReportAsync(
            command.ReportId,
            command.CurrentUserId,
            cancellationToken)
            ?? throw new NotFoundException($"Report with ID '{command.ReportId}' was not found.");

        reportRepository.Remove(report);
        await reportRepository.SaveChangesAsync(cancellationToken);

        return new DeleteReportResponse(report.Id);
    }
}
