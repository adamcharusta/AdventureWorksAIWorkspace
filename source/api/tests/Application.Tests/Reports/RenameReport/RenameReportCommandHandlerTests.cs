using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.Reports;
using AdventureWorksAIWorkspace.Application.Reports.RenameReport;
using AdventureWorksAIWorkspace.Domain.Reports;

namespace AdventureWorksAIWorkspace.Application.Tests.Reports.RenameReport;

public sealed class RenameReportCommandHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();

    [Fact]
    public async Task Handle_WhenOwned_ShouldUpdateTitleAndReturnSummary()
    {
        var report = new Report
        {
            Id = "report-1",
            UserId = "user-1",
            Title = "Old title",
            OriginalPrompt = "prompt",
            Status = ReportStatus.Ready,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns(report);
        _reportRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var response = await RenameReportCommandHandler.Handle(
            new RenameReportCommand("report-1", "  New title  ", "user-1"),
            _reportRepository,
            CancellationToken.None);

        report.Title.Should().Be("New title");
        response.Id.Should().Be("report-1");
        response.Title.Should().Be("New title");
        await _reportRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReportIsNotOwned_ShouldThrowNotFoundException()
    {
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns((Report?)null);

        var act = () => RenameReportCommandHandler.Handle(
            new RenameReportCommand("report-1", "New title", "user-1"),
            _reportRepository,
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUserIsMissing_ShouldThrowUnauthorizedException()
    {
        var act = () => RenameReportCommandHandler.Handle(
            new RenameReportCommand("report-1", "New title", null),
            _reportRepository,
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
