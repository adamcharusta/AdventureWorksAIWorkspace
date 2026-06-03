using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;
using AdventureWorksAIWorkspaceAPI.Application.User.UpdateUser;

namespace AdventureWorksAIWorkspaceAPI.Application.Common.Services;

/// <summary>
/// Admin-facing user administration: create, update, delete, and list users and the roles that can
/// be assigned to them.
/// </summary>
public interface IUserManagementService
{
    Task<CreateUserResult> CreateUserAsync(string userName, string email, string? role,
        CancellationToken cancellationToken = default);

    Task<UpdateUserResult> UpdateUserAsync(UpdateUserCommand command, CancellationToken cancellationToken = default);

    Task<DeleteUserResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetAssignableRolesAsync(CancellationToken cancellationToken = default);
}
