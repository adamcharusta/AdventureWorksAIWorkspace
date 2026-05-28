using AdventureWorksAIWorkspaceAPI.Application.Common.Services;

namespace AdventureWorksAIWorkspaceAPI.Application.User.GetAssignableRoles;

public sealed record GetAssignableRolesQuery;

public static class GetAssignableRolesQueryHandler
{
    public static async Task<GetAssignableRolesResponse> Handle(
        GetAssignableRolesQuery query,
        IUserService userService,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<string> roles = await userService.GetAssignableRolesAsync(cancellationToken);
        return new GetAssignableRolesResponse(roles);
    }
}
