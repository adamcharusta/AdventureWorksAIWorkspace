using System.Text;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Ai;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Services;

/// <summary>
/// Classifies follow-up report messages as either a refinement of the most recent section or a
/// request for a new section, by prompting the AI model through <see cref="IAiChatClient"/>.
/// </summary>
/// <remarks>
/// Conservative by design: any unparseable output or failure falls back to
/// <see cref="ReportChatIntent.NewSection"/>, which preserves the report's existing sections.
/// </remarks>
public sealed class AiReportIntentClassifier : IReportIntentClassifier
{
    private const string SystemPrompt =
        """
        You classify a user's latest message in an ongoing data-report conversation.
        The report already shows a most-recent section (a table/chart). Decide what the latest
        message wants to do with that existing section.

        Answer with exactly one lowercase word, nothing else:
        - "refine" if the message asks to correct, adjust, fix, reshape, filter, sort, rename,
          add/remove/hide a column, or otherwise change the EXISTING most-recent section.
        - "new" if the message asks for a different, additional analysis or an extra table/chart
          that should be added alongside the existing ones.

        When unsure, answer "new". Do not explain. Output only "refine" or "new".
        """;

    private readonly IAiChatClient _chatClient;
    private readonly ILogger<AiReportIntentClassifier> _logger;

    public AiReportIntentClassifier(IAiChatClient chatClient, ILogger<AiReportIntentClassifier> logger)
    {
        Guard.Against.Null(chatClient);
        Guard.Against.Null(logger);

        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<ReportChatIntent> ClassifyAsync(
        ReportIntentRequest request,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(request);

        try
        {
            IReadOnlyList<AiChatMessage> messages =
            [
                AiChatMessage.System(SystemPrompt),
                AiChatMessage.User(BuildUserPrompt(request))
            ];

            AiChatResult completion = await _chatClient.CompleteAsync(messages, cancellationToken);

            return Parse(completion.Content);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(exception, "Report intent classification failed; defaulting to a new section.");
            return ReportChatIntent.NewSection;
        }
    }

    private static string BuildUserPrompt(ReportIntentRequest request)
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(request.LastSectionTitle))
        {
            builder.Append("Most-recent section: ").AppendLine(request.LastSectionTitle);
        }

        if (request.RecentMessages.Count > 0)
        {
            builder.AppendLine("Recent conversation:");
            foreach (string message in request.RecentMessages)
            {
                builder.Append("- ").AppendLine(message);
            }
        }

        builder.Append("Latest message: ").Append(request.Message);

        return builder.ToString();
    }

    private static ReportChatIntent Parse(string? content)
    {
        string text = (content ?? string.Empty).Trim().ToLowerInvariant();

        // Match the model's intended single word without being fooled by it echoing "new" inside a
        // sentence; "refine" is only chosen when it is the clearly leading signal.
        return text.StartsWith("refine", StringComparison.Ordinal)
            ? ReportChatIntent.RefineLastSection
            : ReportChatIntent.NewSection;
    }
}
