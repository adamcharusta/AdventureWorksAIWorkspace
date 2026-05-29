using System.Security.Claims;
using AdventureWorksAIWorkspaceAPI.Application.Reports;
using AdventureWorksAIWorkspaceAPI.Application.Reports.AddReportMessage;
using AdventureWorksAIWorkspaceAPI.Application.Reports.CreateReport;
using AdventureWorksAIWorkspaceAPI.Application.Reports.DeleteReport;
using AdventureWorksAIWorkspaceAPI.Application.Reports.GenerateReport;
using AdventureWorksAIWorkspaceAPI.Application.Reports.GetReportDetails;
using AdventureWorksAIWorkspaceAPI.Application.Reports.GetReports;
using AdventureWorksAIWorkspaceAPI.Application.Reports.RenameReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace AdventureWorksAIWorkspaceAPI.Api.Endpoints;

public static class ReportsEndpoint
{
    private const string SubjectClaimType = "sub";

    [WolverineGet(
        "/api/reports",
        Name = "GetReports",
        OperationId = "GetReports",
        Summary = "Returns the authenticated user's saved reports.",
        Description = "Returns lightweight report metadata for the authenticated user's report sidebar.")]
    [Tags("Reports")]
    [Authorize]
    [ProducesResponseType<GetReportsResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<Ok<GetReportsResponse>> GetReports(
        HttpContext httpContext,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        GetReportsResponse response = await messageBus.InvokeAsync<GetReportsResponse>(
            new GetReportsQuery(GetCurrentUserId(httpContext)),
            cancellationToken);

        return TypedResults.Ok(response);
    }

    [WolverineGet(
        "/api/reports/{reportId}",
        Name = "GetReportDetails",
        OperationId = "GetReportDetails",
        Summary = "Returns a saved report with conversation and SQL history.",
        Description =
            "Returns report metadata, chat messages, and generated SQL metadata for a report owned by the authenticated user.")]
    [Tags("Reports")]
    [Authorize]
    [ProducesResponseType<ReportDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<Ok<ReportDetailsDto>> GetReportDetails(
        string reportId,
        HttpContext httpContext,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        ReportDetailsDto response = await messageBus.InvokeAsync<ReportDetailsDto>(
            new GetReportDetailsQuery(reportId, GetCurrentUserId(httpContext)),
            cancellationToken);

        return TypedResults.Ok(response);
    }

    [WolverinePost(
        "/api/reports",
        Name = "CreateReport",
        OperationId = "CreateReport",
        Summary = "Creates a report from a chat message.",
        Description =
            "Creates a saved report for the authenticated user, persists the first user message, generates SQL, validates it, executes it when safe, and stores the assistant response.")]
    [Tags("Reports")]
    [Authorize]
    [ProducesResponseType<ReportChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<Ok<ReportChatResponse>> CreateReport(
        [FromBody] CreateReportRequest request,
        HttpContext httpContext,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        ReportChatResponse response = await messageBus.InvokeAsync<ReportChatResponse>(
            new CreateReportCommand(request.Message, GetCurrentUserId(httpContext)),
            cancellationToken);

        return TypedResults.Ok(response);
    }

    [WolverinePost(
        "/api/reports/{reportId}/messages",
        Name = "AddReportMessage",
        OperationId = "AddReportMessage",
        Summary = "Adds a follow-up message to a saved report.",
        Description =
            "Appends a user message to an existing report conversation, then generates, validates, executes, and persists the assistant response for a report owned by the authenticated user.")]
    [Tags("Reports")]
    [Authorize]
    [ProducesResponseType<ReportChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<Ok<ReportChatResponse>> AddReportMessage(
        string reportId,
        [FromBody] AddReportMessageRequest request,
        HttpContext httpContext,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        ReportChatResponse response = await messageBus.InvokeAsync<ReportChatResponse>(
            new AddReportMessageCommand(reportId, request.Message, GetCurrentUserId(httpContext)),
            cancellationToken);

        return TypedResults.Ok(response);
    }

    [WolverinePost(
        "/api/reports/generate",
        Name = "GenerateReport",
        OperationId = "GenerateReport",
        Summary = "Generates a report from a natural-language question.",
        Description =
            "Produces read-only SQL with the AI model, validates it for safety, and executes it against AdventureWorks. " +
            "When the generated SQL is rejected, the response returns the SQL and the rejection reason without executing it.")]
    [Tags("Reports")]
    [Authorize]
    [ProducesResponseType<GenerateReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public static async Task<Ok<GenerateReportResponse>> Generate(
        [FromBody] GenerateReportCommand command,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        GenerateReportResponse response =
            await messageBus.InvokeAsync<GenerateReportResponse>(command, cancellationToken);

        return TypedResults.Ok(response);
    }

    [WolverinePut(
        "/api/reports/{reportId}/title",
        Name = "RenameReport",
        OperationId = "RenameReport",
        Summary = "Renames a saved report.",
        Description =
            "Updates the title of a report owned by the authenticated user. The initial title is " +
            "suggested by the AI; this lets the user override it.")]
    [Tags("Reports")]
    [Authorize]
    [ProducesResponseType<ReportSummaryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<Ok<ReportSummaryDto>> RenameReport(
        string reportId,
        [FromBody] RenameReportRequest request,
        HttpContext httpContext,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        ReportSummaryDto response = await messageBus.InvokeAsync<ReportSummaryDto>(
            new RenameReportCommand(reportId, request.Title, GetCurrentUserId(httpContext)),
            cancellationToken);

        return TypedResults.Ok(response);
    }

    [WolverineDelete(
        "/api/reports/{reportId}",
        Name = "DeleteReport",
        OperationId = "DeleteReport",
        Summary = "Deletes a saved report.",
        Description =
            "Permanently deletes a report owned by the authenticated user, including its conversation, " +
            "chat messages, and generated SQL history.")]
    [Tags("Reports")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<NoContent> DeleteReport(
        string reportId,
        HttpContext httpContext,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        await messageBus.InvokeAsync<DeleteReportResponse>(
            new DeleteReportCommand(reportId, GetCurrentUserId(httpContext)),
            cancellationToken);

        return TypedResults.NoContent();
    }

    private static string? GetCurrentUserId(HttpContext httpContext) =>
        httpContext.User.FindFirstValue(SubjectClaimType)
        ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
