using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Sql;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.Reports.GenerateReport;
using NSubstitute.ExceptionExtensions;

namespace AdventureWorksAIWorkspaceAPI.Application.Tests.Reports.GenerateReport;

public sealed class GenerateReportCommandHandlerTests
{
    private readonly IAiSqlGenerator _sqlGenerator = Substitute.For<IAiSqlGenerator>();
    private readonly ISqlSafetyValidator _sqlValidator = Substitute.For<ISqlSafetyValidator>();
    private readonly IAdventureWorksQueryExecutor _queryExecutor = Substitute.For<IAdventureWorksQueryExecutor>();

    [Fact]
    public async Task Handle_WhenSqlIsValid_ShouldExecuteAndReturnResult()
    {
        _sqlGenerator
            .GenerateSqlAsync("top products", Arg.Any<CancellationToken>())
            .Returns(new GeneratedSql("SELECT 1", 10, 20));
        _sqlValidator.Validate("SELECT 1").Returns(SqlValidationResult.Valid());

        var tabular = new TabularResult(
            [new TabularColumn("Value", "int")],
            [[(object?)1]],
            1,
            false,
            5);
        _queryExecutor
            .ExecuteQueryAsync("SELECT 1", Arg.Any<CancellationToken>())
            .Returns(tabular);

        GenerateReportResponse response = await GenerateReportCommandHandler.Handle(
            new GenerateReportCommand("top products"),
            _sqlGenerator,
            _sqlValidator,
            _queryExecutor,
            CancellationToken.None);

        response.Outcome.Should().Be(ReportOutcome.Executed);
        response.Sql.Should().Be("SELECT 1");
        response.Result.Should().BeSameAs(tabular);
        response.Message.Should().BeNull();
        response.InputTokens.Should().Be(10);
        response.OutputTokens.Should().Be(20);
    }

    [Fact]
    public async Task Handle_WhenSqlIsRejected_ShouldNotExecuteAndReturnReason()
    {
        _sqlGenerator
            .GenerateSqlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GeneratedSql("DROP TABLE Production.Product", 5, 5));
        _sqlValidator
            .Validate("DROP TABLE Production.Product")
            .Returns(SqlValidationResult.Invalid("SQL contains a forbidden keyword: DROP."));

        GenerateReportResponse response = await GenerateReportCommandHandler.Handle(
            new GenerateReportCommand("delete everything"),
            _sqlGenerator,
            _sqlValidator,
            _queryExecutor,
            CancellationToken.None);

        response.Outcome.Should().Be(ReportOutcome.Rejected);
        response.Message.Should().Contain("DROP");
        response.Result.Should().BeNull();

        await _queryExecutor
            .DidNotReceive()
            .ExecuteQueryAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExecutionFails_ShouldReturnExecutionFailedOutcome()
    {
        _sqlGenerator
            .GenerateSqlAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new GeneratedSql("SELECT * FROM Missing.Table", 7, 3));
        _sqlValidator
            .Validate("SELECT * FROM Missing.Table")
            .Returns(SqlValidationResult.Valid());
        _queryExecutor
            .ExecuteQueryAsync("SELECT * FROM Missing.Table", Arg.Any<CancellationToken>())
            .ThrowsAsync(new QueryExecutionException("Query execution failed: Invalid object name 'Missing.Table'."));

        GenerateReportResponse response = await GenerateReportCommandHandler.Handle(
            new GenerateReportCommand("show missing"),
            _sqlGenerator,
            _sqlValidator,
            _queryExecutor,
            CancellationToken.None);

        response.Outcome.Should().Be(ReportOutcome.ExecutionFailed);
        response.Message.Should().Contain("Invalid object name");
        response.Result.Should().BeNull();
        response.Sql.Should().Be("SELECT * FROM Missing.Table");
    }
}
