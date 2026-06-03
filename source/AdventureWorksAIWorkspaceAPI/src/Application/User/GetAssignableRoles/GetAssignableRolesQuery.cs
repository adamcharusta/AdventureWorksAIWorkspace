using AdventureWorksAIWorkspaceAPI.Application.Common.Services;

namespace AdventureWorksAIWorkspaceAPI.Application.User.GetAssignableRoles;

public sealed record GetAssignableRolesQuery;

public static class GetAssignableRolesQueryHandler
{
    public static async Task<GetAssignableRolesResponse> Handle(
        GetAssignableRolesQuery query,
        IUserManagementService userManagementService,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string> roles = await userManagementService.GetAssignableRolesAsync(cancellationToken);
        return new GetAssignableRolesResponse(roles);
    }
}
