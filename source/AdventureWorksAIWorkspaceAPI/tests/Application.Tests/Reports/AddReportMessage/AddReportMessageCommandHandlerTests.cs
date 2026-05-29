using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Sql;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.Reports.AddReportMessage;
using AdventureWorksAIWorkspaceAPI.Application.Reports.GenerateReport;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.Reports.AddReportMessage;

public sealed class AddReportMessageCommandHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();
    private readonly IAiSqlGenerator _sqlGenerator = Substitute.For<IAiSqlGenerator>();
    private readonly ISqlSafetyValidator _sqlValidator = Substitute.For<ISqlSafetyValidator>();
    private readonly IAdventureWorksQueryExecutor _queryExecutor = Substitute.For<IAdventureWorksQueryExecutor>();

    [Fact]
    public async Task Handle_WhenReportExists_ShouldAppendMessagesAndPersistSql()
    {
        Report report = CreateReport();
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns(report);
        _reportRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _sqlGenerator
            .GenerateSqlAsync("show revenue by month", Arg.Any<CancellationToken>())
            .Returns(new GeneratedSql("SELECT 1", 3, 4));
        _sqlValidator.Validate("SELECT 1").Returns(SqlValidationResult.Valid());
        _queryExecutor
            .ExecuteQueryAsync("SELECT 1", Arg.Any<CancellationToken>())
            .Returns(new TabularResult(
                [new TabularColumn("Value", "int")],
                [[(object?)1]],
                1,
                false,
                2));

        var response = await AddReportMessageCommandHandler.Handle(
            new AddReportMessageCommand("report-1", "show revenue by month", "user-1"),
            _reportRepository,
            _sqlGenerator,
            _sqlValidator,
            _queryExecutor,
            CancellationToken.None);

        report.Conversation!.Messages.Should().HaveCount(3);
        report.Conversation.Messages.Select(message => message.SortOrder).Should().Equal(1, 2, 3);
        report.GeneratedSqlQueries.Should().ContainSingle();
        report.GeneratedSqlQueries.Single().SourceMessageId.Should().Be(response.UserMessage.Id);
        response.Outcome.Should().Be(ReportOutcome.Executed);
        response.AssistantMessage.RelatedSqlQueryId.Should().Be(response.SqlQuery!.Id);

        await _reportRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReportIsNotOwned_ShouldThrowNotFoundException()
    {
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns((Report?)null);

        var act = () => AddReportMessageCommandHandler.Handle(
            new AddReportMessageCommand("report-1", "follow up", "user-1"),
            _reportRepository,
            _sqlGenerator,
            _sqlValidator,
            _queryExecutor,
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    private static Report CreateReport()
    {
        var report = new Report
        {
            Id = "report-1",
            UserId = "user-1",
            Title = "Sales",
            OriginalPrompt = "sales",
            Status = ReportStatus.Ready,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var conversation = new ReportConversation
        {
            Id = "conversation-1",
            ReportId = report.Id,
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt
        };
        report.Conversation = conversation;
        conversation.Messages.Add(new ReportMessage
        {
            Id = "message-1",
            ConversationId = conversation.Id,
            Role = ReportMessageRole.User,
            Content = "sales",
            SortOrder = 1,
            CreatedAt = report.CreatedAt
        });

        return report;
    }
}
