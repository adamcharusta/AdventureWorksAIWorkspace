using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Sql;
using AdventureWorksAIWorkspaceAPI.Application.Common.Exceptions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;

namespace AdventureWorksAIWorkspaceAPI.Application.Reports.GenerateReport;

/// <summary>
/// Generates a report from a natural-language business question by producing SQL with the AI
/// model, validating it, and executing it against AdventureWorks.
/// </summary>
/// <param name="Question">The user's natural-language business question.</param>
public sealed record GenerateReportCommand(string Question);

public static class GenerateReportCommandHandler
{
    public static async Task<GenerateReportResponse> Handle(
        GenerateReportCommand command,
        IAiSqlGenerator sqlGenerator,
        ISqlSafetyValidator sqlValidator,
        IAdventureWorksQueryExecutor queryExecutor,
        CancellationToken cancellationToken)
    {
        GeneratedSql generated = await sqlGenerator.GenerateSqlAsync(command.Question, cancellationToken);

        SqlValidationResult validation = sqlValidator.Validate(generated.Sql);
        if (!validation.IsValid)
        {
            return Failure(command, generated, ReportOutcome.Rejected, validation.Reason);
        }

        try
        {
            TabularResult result = await queryExecutor.ExecuteQueryAsync(generated.Sql, cancellationToken);

            return new GenerateReportResponse(
                command.Question,
                generated.Sql,
                ReportOutcome.Executed,
                Message: null,
                Result: result,
                generated.InputTokens,
                generated.OutputTokens);
        }
        catch (QueryExecutionException exception)
        {
            return Failure(command, generated, ReportOutcome.ExecutionFailed, exception.Message);
        }
    }

    private static GenerateReportResponse Failure(
        GenerateReportCommand command,
        GeneratedSql generated,
        ReportOutcome outcome,
        string? message)
    {
        return new GenerateReportResponse(
            command.Question,
            generated.Sql,
            outcome,
            message,
            Result: null,
            generated.InputTokens,
            generated.OutputTokens);
    }
}
