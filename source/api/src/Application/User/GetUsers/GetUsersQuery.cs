using AdventureWorksAIWorkspace.Application.Common.Dtos;
using AdventureWorksAIWorkspace.Application.Common.Dtos.User;
using AdventureWorksAIWorkspace.Application.Common.Services.User;

namespace AdventureWorksAIWorkspace.Application.User.GetUsers;

public sealed record GetUsersQuery;

public static class GetUsersQueryHandler
{
    public static async Task<GetUsersResponse> Handle(
        GetUsersQuery query,
        IUserManagementService userManagementService,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<UserDto> users = await userManagementService.GetUsersAsync(cancellationToken);
        return new GetUsersResponse(users);
    }
}
