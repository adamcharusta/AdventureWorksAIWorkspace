using System.Security.Claims;

namespace AdventureWorksAIWorkspaceAPI.Api.Authentication;

/// <summary>
/// Resolves the authenticated user's identifier from the request, in one place, so endpoints do not
/// each repeat the claim lookup.
/// </summary>
public static class CurrentUserExtensions
{
    private const string SubjectClaimType = "sub";

    /// <summary>
    /// Returns the authenticated user's identifier, preferring the JWT <c>sub</c> claim and falling
    /// back to <see cref="ClaimTypes.NameIdentifier"/>. Returns <c>null</c> when neither is present.
    /// </summary>
    public static string? GetCurrentUserId(this HttpContext httpContext) =>
        httpContext.User.FindFirstValue(SubjectClaimType)
        ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
