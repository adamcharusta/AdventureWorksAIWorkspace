using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;
using AdventureWorksAIWorkspaceAPI.Application.User.UpdateUser;

namespace AdventureWorksAIWorkspaceAPI.Application.Common.Services;

public interface IUserService
{
    Task<LoginResult> LoginAsync(string identifier, string password, CancellationToken cancellationToken = default);

    Task<AuthTokens?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<SetFirstPasswordResult> SetFirstPasswordAsync(string identifier, string confirmNewPassword, string newPassword,
        CancellationToken cancellationToken = default);

    Task<CreateUserResult> CreateUserAsync(string userName, string email, string? role,
        CancellationToken cancellationToken = default);

    Task<UpdateUserResult> UpdateUserAsync(UpdateUserCommand command,
        CancellationToken cancellationToken = default);

    Task<DeleteUserResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetAssignableRolesAsync(CancellationToken cancellationToken = default);
}
