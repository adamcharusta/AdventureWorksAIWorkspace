using AdventureWorksAIWorkspace.Application.Common.Dtos.AdventureWorks;
using AdventureWorksAIWorkspace.Application.Common.Dtos.Charts;

namespace AdventureWorksAIWorkspace.Application.Common.Services.Ai;

/// <summary>
/// Decides how a report result should be presented: which charts to render and what business
/// insights to surface.
/// </summary>
/// <remarks>
/// Implementations prompt the AI model with the question and result shape and parse its response.
/// They must never fail the report: when the model output cannot be used, they fall back to a
/// deterministic presentation derived from the result's column types.
/// </remarks>
public interface IReportVisualizer
{
    /// <summary>
    /// Produces insights and chart suggestions for the supplied result.
    /// </summary>
    /// <param name="question">The user's original natural-language question.</param>
    /// <param name="result">The executed query result.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>The insights and chart specifications to present.</returns>
    Task<ReportPresentation> CreatePresentationAsync(
        string question,
        TabularResult result,
        CancellationToken cancellationToken = default);
}
