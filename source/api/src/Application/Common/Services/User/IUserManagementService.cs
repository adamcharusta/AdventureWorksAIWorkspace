using AdventureWorksAIWorkspace.Application.Common.Dtos.User;
using AdventureWorksAIWorkspace.Application.User.UpdateUser;

namespace AdventureWorksAIWorkspace.Application.Common.Services.User;

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
