namespace AdventureWorksAIWorkspace.Application.Common.Dtos.Ai;

/// <summary>
/// SQL produced by the AI model from a natural-language question.
/// </summary>
/// <remarks>
/// The SQL is untrusted until it has passed <c>ISqlSafetyValidator</c>; it must never be executed
/// before validation.
/// </remarks>
/// <param name="Sql">The generated SQL statement.</param>
/// <param name="InputTokens">Prompt tokens consumed, when reported by the provider.</param>
/// <param name="OutputTokens">Completion tokens produced, when reported by the provider.</param>
public sealed record GeneratedSql(string Sql, int? InputTokens, int? OutputTokens);
