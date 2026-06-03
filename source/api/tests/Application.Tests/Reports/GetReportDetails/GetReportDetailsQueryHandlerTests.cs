using AdventureWorksAIWorkspace.Application.Common.Exceptions;
using AdventureWorksAIWorkspace.Application.Common.Services.Reports;
using AdventureWorksAIWorkspace.Application.Reports.GetReportDetails;
using AdventureWorksAIWorkspace.Domain.Reports;

namespace AdventureWorksAIWorkspace.Application.Tests.Reports.GetReportDetails;

public sealed class GetReportDetailsQueryHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();

    [Fact]
    public async Task Handle_WhenReportIsOwned_ShouldReturnReportDetails()
    {
        var report = new Report
        {
            Id = "report-1",
            UserId = "user-1",
            Title = "Sales report",
            OriginalPrompt = "Show sales",
            Summary = "Sales increased.",
            Status = ReportStatus.Ready,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };
        var conversation = new ReportConversation
        {
            Id = "conversation-1",
            ReportId = report.Id,
            Report = report,
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt
        };
        conversation.Messages.Add(new ReportMessage
        {
            Id = "message-1",
            ConversationId = conversation.Id,
            Role = ReportMessageRole.User,
            Content = "Show sales",
            SortOrder = 1,
            CreatedAt = report.CreatedAt
        });
        report.Conversation = conversation;
        report.GeneratedSqlQueries.Add(new GeneratedSqlQuery
        {
            Id = "query-1",
            ReportId = report.Id,
            UserPrompt = "Show sales",
            SqlText = "SELECT 1",
            ValidationStatus = SqlValidationStatus.Valid,
            ExecutionStatus = SqlExecutionStatus.Executed,
            CreatedAt = report.CreatedAt
        });
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns(report);

        var response = await GetReportDetailsQueryHandler.Handle(
            new GetReportDetailsQuery("report-1", "user-1"),
            _reportRepository,
            CancellationToken.None);

        response.Id.Should().Be("report-1");
        response.Title.Should().Be("Sales report");
        response.Messages.Should().ContainSingle(message => message.Id == "message-1");
        response.GeneratedSqlQueries.Should().ContainSingle(query => query.Id == "query-1");
    }

    [Fact]
    public async Task Handle_WhenReportIsNotOwned_ShouldThrowNotFoundException()
    {
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns((Report?)null);

        var act = () => GetReportDetailsQueryHandler.Handle(
            new GetReportDetailsQuery("report-1", "user-1"),
            _reportRepository,
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCurrentUserIsMissing_ShouldThrowUnauthorizedException()
    {
        var act = () => GetReportDetailsQueryHandler.Handle(
            new GetReportDetailsQuery("report-1", null),
            _reportRepository,
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
