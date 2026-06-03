using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Charts;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Sql;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.Reports;
using AdventureWorksAIWorkspaceAPI.Application.Reports.CreateReport;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.Reports.CreateReport;

public sealed class CreateReportCommandHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();
    private readonly IAiSqlGenerator _sqlGenerator = Substitute.For<IAiSqlGenerator>();
    private readonly ISqlSafetyValidator _sqlValidator = Substitute.For<ISqlSafetyValidator>();
    private readonly IAdventureWorksQueryExecutor _queryExecutor = Substitute.For<IAdventureWorksQueryExecutor>();
    private readonly IReportVisualizer _reportVisualizer = Substitute.For<IReportVisualizer>();
    private readonly IReportIntentClassifier _intentClassifier = Substitute.For<IReportIntentClassifier>();

    private IReportChatPipeline Pipeline => new ReportChatPipeline(
        _sqlGenerator, _sqlValidator, _queryExecutor, _reportVisualizer, _intentClassifier);

    [Fact]
    public async Task Handle_WhenSqlExecutes_ShouldPersistReportConversationAndSql()
    {
        Report? savedReport = null;
        _reportRepository
            .AddAsync(Arg.Do<Report>(report => savedReport = report), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _reportRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _sqlGenerator
            .GenerateSqlAsync(
                "top products",
                Arg.Is<AiSqlGenerationContext>(context =>
                    context.OriginalPrompt == "top products" &&
                    context.RecentMessages.Count == 0),
                Arg.Any<CancellationToken>())
            .Returns(new GeneratedSql("SELECT TOP 10 * FROM Production.Product", 12, 8));
        _sqlValidator
            .Validate("SELECT TOP 10 * FROM Production.Product")
            .Returns(SqlValidationResult.Valid());
        _queryExecutor
            .ExecuteQueryAsync("SELECT TOP 10 * FROM Production.Product", Arg.Any<CancellationToken>())
            .Returns(new TabularResult(
                [new TabularColumn("Name", "nvarchar")],
                [[(object?)"Road Bike"]],
                1,
                false,
                15));
        _reportVisualizer
            .CreatePresentationAsync("top products", Arg.Any<TabularResult>(), Arg.Any<CancellationToken>())
            .Returns(new ReportPresentation(
                "Road Bike is the top product.",
                [new ChartSpec(ChartKind.Bar, "Top products", "Name", [new ChartSeries("Name", null)], null)],
                "Top products by sales"));

        var response = await CreateReportCommandHandler.Handle(
            new CreateReportCommand("top products", "user-1"),
            _reportRepository,
            Pipeline,
            CancellationToken.None);

        savedReport.Should().NotBeNull();
        savedReport!.UserId.Should().Be("user-1");
        savedReport.Title.Should().Be("Top products by sales");
        savedReport.Status.Should().Be(ReportStatus.Ready);
        savedReport.Conversation!.Messages.Should().HaveCount(2);
        savedReport.GeneratedSqlQueries.Should().ContainSingle();
        savedReport.GeneratedSqlQueries.Single().ExecutionStatus.Should().Be(SqlExecutionStatus.Executed);
        savedReport.GeneratedSqlQueries.Single().PresentationTitle.Should().Be("Top products by sales");
        savedReport.GeneratedSqlQueries.Single().Summary.Should().Be("Road Bike is the top product.");
        savedReport.GeneratedSqlQueries.Single().ResultJson.Should().NotBeNullOrWhiteSpace();
        savedReport.GeneratedSqlQueries.Single().ChartsJson.Should().NotBeNullOrWhiteSpace();
        savedReport.ResultJson.Should().NotBeNullOrWhiteSpace();
        savedReport.ChartsJson.Should().NotBeNullOrWhiteSpace();
        response.Outcome.Should().Be(ReportOutcome.Executed);
        response.Report.Messages.Should().HaveCount(2);
        response.Report.Result.Should().NotBeNull();
        response.Report.Charts.Should().ContainSingle();
        response.Report.Sections.Should().ContainSingle();
        response.Report.Sections[0].Title.Should().Be("Top products by sales");
        response.Report.Sections[0].Result.Should().NotBeNull();
        response.Report.Sections[0].Charts.Should().ContainSingle();
        response.SqlQuery!.ResultRowCount.Should().Be(1);
        response.Charts.Should().ContainSingle();
        response.Charts[0].Kind.Should().Be(ChartKind.Bar);
        savedReport.Summary.Should().Be("Road Bike is the top product.");
        response.AssistantMessage.Content.Should().Be("Road Bike is the top product.");

        await _reportRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
