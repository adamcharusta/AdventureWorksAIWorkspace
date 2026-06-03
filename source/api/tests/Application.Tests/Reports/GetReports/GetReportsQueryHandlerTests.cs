using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.Reports;
using AdventureWorksAIWorkspace.Application.Reports.GetReports;
using AdventureWorksAIWorkspace.Domain.Reports;

namespace AdventureWorksAIWorkspace.Application.Tests.Reports.GetReports;

public sealed class GetReportsQueryHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldReturnUserReports()
    {
        _reportRepository
            .GetReportsForUserAsync("user-1", Arg.Any<CancellationToken>())
            .Returns([
                new Report
                {
                    Id = "report-1",
                    UserId = "user-1",
                    Title = "Sales",
                    Status = ReportStatus.Ready,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow
                }
            ]);

        GetReportsResponse response = await GetReportsQueryHandler.Handle(
            new GetReportsQuery("user-1"),
            _reportRepository,
            CancellationToken.None);

        response.Reports.Should().ContainSingle();
        response.Reports[0].Id.Should().Be("report-1");
        response.Reports[0].Title.Should().Be("Sales");
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsMissing_ShouldThrowUnauthorizedException()
    {
        var act = () => GetReportsQueryHandler.Handle(
            new GetReportsQuery(null),
            _reportRepository,
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
