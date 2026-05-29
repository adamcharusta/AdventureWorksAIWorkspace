using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Common.Services;

public interface IReportRepository
{
    Task AddAsync(Report report, CancellationToken cancellationToken = default);

    Task<Report?> GetOwnedReportAsync(string reportId, string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Report>> GetReportsForUserAsync(string userId, CancellationToken cancellationToken = default);

    void Remove(Report report);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
