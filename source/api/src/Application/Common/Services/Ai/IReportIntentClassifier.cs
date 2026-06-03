using AdventureWorksAIWorkspace.Application.Common.Dtos.Ai;

namespace AdventureWorksAIWorkspace.Application.Common.Services.Ai;

/// <summary>
/// What a follow-up report message intends to do with the report's existing sections.
/// </summary>
public enum ReportChatIntent
{
    /// <summary>Produce a new, additional report section (a new table/chart).</summary>
    NewSection = 0,

    /// <summary>Refine or correct the most recent section in place instead of adding another.</summary>
    RefineLastSection = 1
}

/// <summary>
/// Classifies whether a follow-up chat message asks to refine the existing report section or to add
/// a new one, so the workflow can overwrite a section in place rather than always appending.
/// </summary>
/// <remarks>
/// Implementations may call an AI model. They must be conservative: when the intent is unclear or
/// the request fails, return <see cref="ReportChatIntent.NewSection"/> so the report keeps its
/// existing sections untouched.
/// </remarks>
public interface IReportIntentClassifier
{
    /// <summary>
    /// Classifies the intent of a follow-up report message.
    /// </summary>
    /// <param name="request">The message and conversation context to classify.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The classified intent.</returns>
    Task<ReportChatIntent> ClassifyAsync(
        ReportIntentRequest request,
        CancellationToken cancellationToken = default);
}
