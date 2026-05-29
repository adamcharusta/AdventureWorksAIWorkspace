using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Sql;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.Reports.CreateReport;
using AdventureWorksAIWorkspaceAPI.Application.Reports.GenerateReport;
using AdventureWorksAIWorkspaceAPI.Domain.Reports;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.Reports.CreateReport;

public sealed class CreateReportCommandHandlerTests
{
    private readonly IReportRepository _reportRepository = Substitute.For<IReportRepository>();
    private readonly IAiSqlGenerator _sqlGenerator = Substitute.For<IAiSqlGenerator>();
    private readonly ISqlSafetyValidator _sqlValidator = Substitute.For<ISqlSafetyValidator>();
    private readonly IAdventureWorksQueryExecutor _queryExecutor = Substitute.For<IAdventureWorksQueryExecutor>();

    [Fact]
    public async Task Handle_WhenSqlExecutes_ShouldPersistReportConversationAndSql()
    {
        Report? savedReport = null;
        _reportRepository
            .AddAsync(Arg.Do<Report>(report => savedReport = report), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _reportRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _sqlGenerator
            .GenerateSqlAsync("top products", Arg.Any<CancellationToken>())
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

        var response = await CreateReportCommandHandler.Handle(
            new CreateReportCommand("top products", "user-1"),
            _reportRepository,
            _sqlGenerator,
            _sqlValidator,
            _queryExecutor,
            CancellationToken.None);

        savedReport.Should().NotBeNull();
        savedReport!.UserId.Should().Be("user-1");
        savedReport.Title.Should().Be("top products");
        savedReport.Status.Should().Be(ReportStatus.Ready);
        savedReport.Conversation!.Messages.Should().HaveCount(2);
        savedReport.GeneratedSqlQueries.Should().ContainSingle();
        savedReport.GeneratedSqlQueries.Single().ExecutionStatus.Should().Be(SqlExecutionStatus.Executed);
        response.Outcome.Should().Be(ReportOutcome.Executed);
        response.Report.Messages.Should().HaveCount(2);
        response.SqlQuery!.ResultRowCount.Should().Be(1);

        await _reportRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
