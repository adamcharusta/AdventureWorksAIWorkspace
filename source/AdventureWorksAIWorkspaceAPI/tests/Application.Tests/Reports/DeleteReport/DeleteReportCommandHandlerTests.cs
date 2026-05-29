using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.Reports.DeleteReport;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.Reports.DeleteReport;

public sealed class DeleteReportCommandHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();

    [Fact]
    public async Task Handle_WhenOwned_ShouldRemoveReportAndSaveChanges()
    {
        var report = new Report
        {
            Id = "report-1",
            UserId = "user-1",
            Title = "Report",
            OriginalPrompt = "prompt",
            Status = ReportStatus.Ready,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns(report);
        _reportRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var response = await DeleteReportCommandHandler.Handle(
            new DeleteReportCommand("report-1", "user-1"),
            _reportRepository,
            CancellationToken.None);

        response.ReportId.Should().Be("report-1");
        _reportRepository.Received(1).Remove(report);
        await _reportRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReportIsNotOwned_ShouldThrowNotFoundException()
    {
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns((Report?)null);

        var act = () => DeleteReportCommandHandler.Handle(
            new DeleteReportCommand("report-1", "user-1"),
            _reportRepository,
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
        _reportRepository.DidNotReceive().Remove(Arg.Any<Report>());
    }

    [Fact]
    public async Task Handle_WhenUserIsMissing_ShouldThrowUnauthorizedException()
    {
        var act = () => DeleteReportCommandHandler.Handle(
            new DeleteReportCommand("report-1", null),
            _reportRepository,
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
