using AdventureWorksAIWorkspace.Application.Common.Dtos.Ai;

namespace AdventureWorksAIWorkspace.Application.Common.Services.Ai;

/// <summary>
/// Sends chat messages to an AI model and returns the model's completion.
/// </summary>
/// <remarks>
/// This is a vendor-neutral transport abstraction. Prompt construction, schema context shaping,
/// and AI workflow orchestration belong to higher-level application services that depend on this
/// contract, not to its implementation.
/// </remarks>
public interface IAiChatClient
{
    /// <summary>
    /// Sends the supplied messages to the model and returns its completion.
    /// </summary>
    /// <param name="messages">The ordered conversation messages to send.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The model completion text together with token usage when available.</returns>
    Task<AiChatResult> CompleteAsync(
        IReadOnlyList<AiChatMessage> messages,
        CancellationToken cancellationToken = default);
}
