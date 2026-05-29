using System.Text.RegularExpressions;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using Ardalis.GuardClauses;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Services;

/// <summary>
/// Generates read-only AdventureWorks SQL from a natural-language question by prompting the AI
/// model through <see cref="IAiChatClient"/>.
/// </summary>
/// <remarks>
/// The prompt instructs the model to emit a single read-only SELECT, but the model is untrusted:
/// the generated SQL must still pass <see cref="ISqlSafetyValidator"/> before it is executed.
/// </remarks>
public sealed partial class AiSqlGenerator : IAiSqlGenerator
{
    private const string SystemPrompt =
        """
        You are a senior T-SQL analyst for the Microsoft AdventureWorks SQL Server database.
        Convert the user's business question into a single, read-only T-SQL query.

        Rules:
        - Return exactly one statement. A leading common table expression (WITH ...) is allowed, but it must feed a SELECT.
        - The statement must be read-only. Never use INSERT, UPDATE, DELETE, DROP, ALTER, TRUNCATE, EXEC, MERGE, CREATE, GRANT, REVOKE, or SELECT ... INTO.
        - Do not return multiple statements separated by semicolons.
        - Use schema-qualified object names, for example Sales.SalesOrderHeader.
        - Use TOP to keep result sets reasonable when the question does not specify a limit.
        - Output only the SQL statement. Do not add explanations, comments, or markdown code fences.
        """;

    private readonly IAiChatClient _chatClient;

    public AiSqlGenerator(IAiChatClient chatClient)
    {
        Guard.Against.Null(chatClient);
        _chatClient = chatClient;
    }

    public async Task<GeneratedSql> GenerateSqlAsync(string question, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(question);

        IReadOnlyList<AiChatMessage> messages =
        [
            AiChatMessage.System(SystemPrompt),
            AiChatMessage.User(question)
        ];

        AiChatResult result = await _chatClient.CompleteAsync(messages, cancellationToken);

        string sql = ExtractSql(result.Content);

        return new GeneratedSql(sql, result.InputTokens, result.OutputTokens);
    }

    private static string ExtractSql(string content)
    {
        string text = (content ?? string.Empty).Trim();

        Match fence = CodeFenceRegex().Match(text);
        return fence.Success ? fence.Groups["sql"].Value.Trim() : text;
    }

    [GeneratedRegex("```(?:sql)?\\s*(?<sql>.+?)```", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
    private static partial Regex CodeFenceRegex();
}
