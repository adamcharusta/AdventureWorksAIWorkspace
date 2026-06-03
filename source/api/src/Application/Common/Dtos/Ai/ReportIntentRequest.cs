namespace AdventureWorksAIWorkspace.Application.Common.Dtos.Ai;

/// <summary>
/// Inputs for classifying what a follow-up report message intends to do with the report.
/// </summary>
/// <param name="Message">The new user message being classified.</param>
/// <param name="RecentMessages">Recent conversation turns before this message, oldest first.</param>
/// <param name="LastSectionTitle">
/// Title (or originating prompt) of the most recent existing report section, used as the candidate
/// target when the message turns out to be a refinement.
/// </param>
public sealed record ReportIntentRequest(
    string Message,
    IReadOnlyList<string> RecentMessages,
    string? LastSectionTitle);
