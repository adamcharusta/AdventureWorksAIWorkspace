using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Charts;
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
    private readonly IReportVisualizer _reportVisualizer = Substitute.For<IReportVisualizer>();
    private readonly IReportIntentClassifier _intentClassifier = Substitute.For<IReportIntentClassifier>();

    [Fact]
    public async Task Handle_WhenReportExists_ShouldAppendMessagesAndPersistSql()
    {
        Report report = CreateReport();
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns(report);
        _reportRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _sqlGenerator
            .GenerateSqlAsync(
                "show revenue by month",
                Arg.Is<AiSqlGenerationContext>(context =>
                    context.OriginalPrompt == "sales" &&
                    context.RecentMessages.Count == 1 &&
                    context.RecentMessages[0].Contains("User: sales")),
                Arg.Any<CancellationToken>())
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
        _reportVisualizer
            .CreatePresentationAsync("show revenue by month", Arg.Any<TabularResult>(), Arg.Any<CancellationToken>())
            .Returns(new ReportPresentation("Revenue trend looks stable.", []));

        var response = await AddReportMessageCommandHandler.Handle(
            new AddReportMessageCommand("report-1", "show revenue by month", "user-1"),
            _reportRepository,
            _sqlGenerator,
            _sqlValidator,
            _queryExecutor,
            _reportVisualizer,
            _intentClassifier,
            CancellationToken.None);

        report.Conversation!.Messages.Should().HaveCount(3);
        report.Conversation.Messages.Select(message => message.SortOrder).Should().Equal(1, 2, 3);
        report.GeneratedSqlQueries.Should().HaveCount(2);
        report.GeneratedSqlQueries.Last().SourceMessageId.Should().Be(response.UserMessage.Id);
        report.GeneratedSqlQueries.Last().Summary.Should().Be("Revenue trend looks stable.");
        report.GeneratedSqlQueries.Last().ResultJson.Should().NotBeNullOrWhiteSpace();
        response.Outcome.Should().Be(ReportOutcome.Executed);
        response.AssistantMessage.RelatedSqlQueryId.Should().Be(response.SqlQuery!.Id);
        response.Report.Result.Should().NotBeNull();
        response.Report.Sections.Should().HaveCount(2);
        response.Report.Sections.Select(section => section.Title)
            .Should()
            .Equal("Existing category section", "show revenue by month");
        report.ResultJson.Should().NotBeNullOrWhiteSpace();
        report.ChartsJson.Should().NotBeNullOrWhiteSpace();
        report.Title.Should().Be("Sales");

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
            _reportVisualizer,
            _intentClassifier,
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenExecutionFailsThenSucceeds_ShouldRetryWithFeedback()
    {
        Report report = CreateReport();
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns(report);
        _reportRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _sqlGenerator
            .GenerateSqlAsync("retry please", Arg.Any<AiSqlGenerationContext>(), Arg.Any<CancellationToken>())
            .Returns(new GeneratedSql("SELECT broken", 1, 1), new GeneratedSql("SELECT fixed", 2, 2));
        _sqlValidator.Validate(Arg.Any<string>()).Returns(SqlValidationResult.Valid());
        _queryExecutor
            .ExecuteQueryAsync("SELECT broken", Arg.Any<CancellationToken>())
            .Returns<TabularResult>(_ => throw new QueryExecutionException("ORDER BY is invalid in subqueries."));
        _queryExecutor
            .ExecuteQueryAsync("SELECT fixed", Arg.Any<CancellationToken>())
            .Returns(new TabularResult([new TabularColumn("Value", "int")], [[(object?)1]], 1, false, 2));
        _reportVisualizer
            .CreatePresentationAsync("retry please", Arg.Any<TabularResult>(), Arg.Any<CancellationToken>())
            .Returns(new ReportPresentation("Recovered after retry.", []));

        var response = await AddReportMessageCommandHandler.Handle(
            new AddReportMessageCommand("report-1", "retry please", "user-1"),
            _reportRepository,
            _sqlGenerator,
            _sqlValidator,
            _queryExecutor,
            _reportVisualizer,
            _intentClassifier,
            CancellationToken.None);

        response.Outcome.Should().Be(ReportOutcome.Executed);
        await _sqlGenerator.Received(2)
            .GenerateSqlAsync("retry please", Arg.Any<AiSqlGenerationContext>(), Arg.Any<CancellationToken>());
        await _sqlGenerator.Received(1).GenerateSqlAsync(
            "retry please",
            Arg.Is<AiSqlGenerationContext>(context => context.PreviousFailure == null),
            Arg.Any<CancellationToken>());
        await _sqlGenerator.Received(1).GenerateSqlAsync(
            "retry please",
            Arg.Is<AiSqlGenerationContext>(context =>
                context.PreviousFailure != null &&
                context.PreviousFailure.FailedSql == "SELECT broken" &&
                context.PreviousFailure.Error.Contains("ORDER BY")),
            Arg.Any<CancellationToken>());
        report.GeneratedSqlQueries.Should().HaveCount(3);
        report.GeneratedSqlQueries.Last().ExecutionStatus.Should().Be(SqlExecutionStatus.Executed);
        report.Status.Should().Be(ReportStatus.Ready);
    }

    [Fact]
    public async Task Handle_WhenValidationRejectsThenSucceeds_ShouldRetryWithFeedback()
    {
        Report report = CreateReport();
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns(report);
        _reportRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _sqlGenerator
            .GenerateSqlAsync("retry please", Arg.Any<AiSqlGenerationContext>(), Arg.Any<CancellationToken>())
            .Returns(new GeneratedSql("SELECT 1; SELECT 2", 1, 1), new GeneratedSql("SELECT 1", 2, 2));
        _sqlValidator.Validate("SELECT 1; SELECT 2")
            .Returns(SqlValidationResult.Invalid("Multiple SQL statements are not allowed."));
        _sqlValidator.Validate("SELECT 1").Returns(SqlValidationResult.Valid());
        _queryExecutor
            .ExecuteQueryAsync("SELECT 1", Arg.Any<CancellationToken>())
            .Returns(new TabularResult([new TabularColumn("Value", "int")], [[(object?)1]], 1, false, 2));
        _reportVisualizer
            .CreatePresentationAsync("retry please", Arg.Any<TabularResult>(), Arg.Any<CancellationToken>())
            .Returns(new ReportPresentation("Recovered after retry.", []));

        var response = await AddReportMessageCommandHandler.Handle(
            new AddReportMessageCommand("report-1", "retry please", "user-1"),
            _reportRepository,
            _sqlGenerator,
            _sqlValidator,
            _queryExecutor,
            _reportVisualizer,
            _intentClassifier,
            CancellationToken.None);

        response.Outcome.Should().Be(ReportOutcome.Executed);
        await _sqlGenerator.Received(1).GenerateSqlAsync(
            "retry please",
            Arg.Is<AiSqlGenerationContext>(context =>
                context.PreviousFailure != null &&
                context.PreviousFailure.Error.Contains("Multiple SQL statements")),
            Arg.Any<CancellationToken>());
        report.GeneratedSqlQueries.Should().HaveCount(3);
        report.GeneratedSqlQueries.Last().ExecutionStatus.Should().Be(SqlExecutionStatus.Executed);
    }

    [Fact]
    public async Task Handle_WhenAllAttemptsFailExecution_ShouldStopAfterMaxAttempts()
    {
        Report report = CreateReport();
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns(report);
        _reportRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _sqlGenerator
            .GenerateSqlAsync("retry please", Arg.Any<AiSqlGenerationContext>(), Arg.Any<CancellationToken>())
            .Returns(new GeneratedSql("SELECT broken", 1, 1));
        _sqlValidator.Validate(Arg.Any<string>()).Returns(SqlValidationResult.Valid());
        _queryExecutor
            .ExecuteQueryAsync("SELECT broken", Arg.Any<CancellationToken>())
            .Returns<TabularResult>(_ => throw new QueryExecutionException("ORDER BY is invalid in subqueries."));

        var response = await AddReportMessageCommandHandler.Handle(
            new AddReportMessageCommand("report-1", "retry please", "user-1"),
            _reportRepository,
            _sqlGenerator,
            _sqlValidator,
            _queryExecutor,
            _reportVisualizer,
            _intentClassifier,
            CancellationToken.None);

        response.Outcome.Should().Be(ReportOutcome.ExecutionFailed);
        await _sqlGenerator.Received(3)
            .GenerateSqlAsync("retry please", Arg.Any<AiSqlGenerationContext>(), Arg.Any<CancellationToken>());
        report.GeneratedSqlQueries.Should().HaveCount(4);
        report.GeneratedSqlQueries.Last().ExecutionStatus.Should().Be(SqlExecutionStatus.Failed);
        report.Status.Should().Be(ReportStatus.Failed);
    }

    [Fact]
    public async Task Handle_WhenIntentIsRefine_ShouldOverwriteLastSectionInPlace()
    {
        Report report = CreateReport();
        _reportRepository
            .GetOwnedReportAsync("report-1", "user-1", Arg.Any<CancellationToken>())
            .Returns(report);
        _reportRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _intentClassifier
            .ClassifyAsync(Arg.Any<ReportIntentRequest>(), Arg.Any<CancellationToken>())
            .Returns(ReportChatIntent.RefineLastSection);
        _sqlGenerator
            .GenerateSqlAsync("hide the account number", Arg.Any<AiSqlGenerationContext>(), Arg.Any<CancellationToken>())
            .Returns(new GeneratedSql("SELECT 2", 5, 6));
        _sqlValidator.Validate("SELECT 2").Returns(SqlValidationResult.Valid());
        _queryExecutor
            .ExecuteQueryAsync("SELECT 2", Arg.Any<CancellationToken>())
            .Returns(new TabularResult(
                [new TabularColumn("Value", "int")],
                [[(object?)1]],
                1,
                false,
                3));
        _reportVisualizer
            .CreatePresentationAsync("hide the account number", Arg.Any<TabularResult>(), Arg.Any<CancellationToken>())
            .Returns(new ReportPresentation("Refined the existing table.", []));

        var response = await AddReportMessageCommandHandler.Handle(
            new AddReportMessageCommand("report-1", "hide the account number", "user-1"),
            _reportRepository,
            _sqlGenerator,
            _sqlValidator,
            _queryExecutor,
            _reportVisualizer,
            _intentClassifier,
            CancellationToken.None);

        response.Outcome.Should().Be(ReportOutcome.Executed);

        // The existing section is overwritten in place rather than a new one being appended.
        response.Report.Sections.Should().ContainSingle();
        response.SqlQuery!.Id.Should().Be("sql-existing");

        GeneratedSqlQuery refined = report.GeneratedSqlQueries.Single(query => query.Id == "sql-existing");
        refined.SqlText.Should().Be("SELECT 2");
        refined.Summary.Should().Be("Refined the existing table.");
        refined.ResultRowCount.Should().Be(1);

        // The working audit row for this turn carries no section content, so it is not a duplicate.
        GeneratedSqlQuery working = report.GeneratedSqlQueries.Single(query => query.Id != "sql-existing");
        working.Summary.Should().BeNullOrWhiteSpace();
        working.ResultJson.Should().BeNullOrWhiteSpace();

        await _intentClassifier.Received(1)
            .ClassifyAsync(Arg.Any<ReportIntentRequest>(), Arg.Any<CancellationToken>());
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

        report.GeneratedSqlQueries.Add(new GeneratedSqlQuery
        {
            Id = "sql-existing",
            ReportId = report.Id,
            UserPrompt = "sales",
            SqlText = "SELECT 1",
            PresentationTitle = "Existing category section",
            Summary = "Existing section stays in the report.",
            ResultJson =
                "{\"columns\":[{\"name\":\"Value\",\"dataType\":\"int\"}],\"rows\":[[1]],\"rowCount\":1,\"truncated\":false,\"elapsedMilliseconds\":1}",
            ChartsJson = "[]",
            ValidationStatus = SqlValidationStatus.Valid,
            ExecutionStatus = SqlExecutionStatus.Executed,
            CreatedAt = report.CreatedAt
        });

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
