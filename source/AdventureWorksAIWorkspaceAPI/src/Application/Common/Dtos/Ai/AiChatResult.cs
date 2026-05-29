namespace AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;

/// <summary>
/// The result of a chat completion request to the AI model.
/// </summary>
/// <param name="Content">The text content produced by the model.</param>
/// <param name="InputTokens">Number of prompt tokens consumed, when reported by the provider.</param>
/// <param name="OutputTokens">Number of completion tokens produced, when reported by the provider.</param>
public sealed record AiChatResult(string Content, int? InputTokens, int? OutputTokens);
