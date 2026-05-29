using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Database;

public sealed class ReportRepository(AppDbContext dbContext) : IReportRepository
{
    public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        await dbContext.Reports.AddAsync(report, cancellationToken);
    }

    public async Task<Report?> GetOwnedReportAsync(
        string reportId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Reports
            .Include(report => report.Conversation)
            .ThenInclude(conversation => conversation!.Messages)
            .Include(report => report.GeneratedSqlQueries)
            .SingleOrDefaultAsync(
                report => report.Id == reportId && report.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Report>> GetReportsForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Reports
            .Where(report => report.UserId == userId)
            .OrderByDescending(report => report.UpdatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
