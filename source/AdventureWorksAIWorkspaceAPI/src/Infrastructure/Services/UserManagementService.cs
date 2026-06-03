using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.User.UpdateUser;
using AdventureWorksAIWorkspaceAPI.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Services;

/// <summary>
/// Default <see cref="IUserManagementService"/>: admin operations over Identity users and roles.
/// Refresh-token cleanup on password reset and delete is delegated to <see cref="ITokenService"/>.
/// </summary>
internal sealed class UserManagementService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ITokenService tokenService,
    IOptions<InitialAdminOptions> initialAdminOptions) : IUserManagementService
{
    private readonly InitialAdminOptions _initialAdminOptions = initialAdminOptions.Value;

    public async Task<CreateUserResult> CreateUserAsync(string userName, string email, string? role,
        CancellationToken cancellationToken = default)
    {
        string assignedRole = role ?? RoleNames.User;

        if (!await roleManager.RoleExistsAsync(assignedRole))
        {
            return new CreateUserResult(CreateUserOutcome.InvalidRole);
        }

        ApplicationUser? existingByName = await userManager.FindByNameAsync(userName);
        ApplicationUser? existingByEmail = await userManager.FindByEmailAsync(email);

        if (existingByName is not null || existingByEmail is not null)
        {
            return new CreateUserResult(CreateUserOutcome.UserAlreadyExists);
        }

        ApplicationUser newUser = new() { UserName = userName, Email = email, IsFirstLogin = true };

        IdentityResult createResult = await userManager.CreateAsync(newUser, _initialAdminOptions.TemplatePassword);

        if (!createResult.Succeeded)
        {
            var errors = createResult.Errors.Select(e => e.Description).ToList();
            return new CreateUserResult(CreateUserOutcome.CreationFailed, Errors: errors);
        }

        await userManager.AddToRoleAsync(newUser, assignedRole);

        return new CreateUserResult(CreateUserOutcome.Success, newUser.Id, newUser.UserName, newUser.Email,
            assignedRole);
    }

    public async Task<UpdateUserResult> UpdateUserAsync(UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(command.UserId);

        if (user is null)
        {
            return new UpdateUserResult(UpdateUserOutcome.UserNotFound);
        }

        if (command.UserName is not null && command.UserName != user.UserName)
        {
            ApplicationUser? existingByName = await userManager.FindByNameAsync(command.UserName);
            if (existingByName is not null)
            {
                return new UpdateUserResult(UpdateUserOutcome.UserNameAlreadyTaken);
            }

            user.UserName = command.UserName;
        }

        if (command.Email is not null && command.Email != user.Email)
        {
            ApplicationUser? existingByEmail = await userManager.FindByEmailAsync(command.Email);
            if (existingByEmail is not null)
            {
                return new UpdateUserResult(UpdateUserOutcome.EmailAlreadyTaken);
            }

            user.Email = command.Email;
        }

        if (command.Role is not null)
        {
            if (!await roleManager.RoleExistsAsync(command.Role))
            {
                return new UpdateUserResult(UpdateUserOutcome.InvalidRole);
            }

            IList<string> currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRoleAsync(user, command.Role);
        }

        if (command.ResetPassword)
        {
            string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult resetResult =
                await userManager.ResetPasswordAsync(user, resetToken, _initialAdminOptions.TemplatePassword);

            if (!resetResult.Succeeded)
            {
                var errors = resetResult.Errors.Select(e => e.Description).ToList();
                return new UpdateUserResult(UpdateUserOutcome.UpdateFailed, Errors: errors);
            }

            user.IsFirstLogin = true;
            await tokenService.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
        }

        IdentityResult updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => e.Description).ToList();
            return new UpdateUserResult(UpdateUserOutcome.UpdateFailed, Errors: errors);
        }

        IList<string> roles = await userManager.GetRolesAsync(user);
        string currentRole = roles.FirstOrDefault() ?? RoleNames.User;

        return new UpdateUserResult(UpdateUserOutcome.Success, user.Id, user.UserName, user.Email, currentRole);
    }

    public async Task<DeleteUserResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return new DeleteUserResult(DeleteUserOutcome.UserNotFound);
        }

        await tokenService.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);

        IdentityResult deleteResult = await userManager.DeleteAsync(user);

        if (!deleteResult.Succeeded)
        {
            var errors = deleteResult.Errors.Select(e => e.Description).ToList();
            return new DeleteUserResult(DeleteUserOutcome.DeleteFailed, errors);
        }

        return new DeleteUserResult(DeleteUserOutcome.Success);
    }

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        List<ApplicationUser> users = await userManager.Users
            .OrderBy(u => u.UserName)
            .ToListAsync(cancellationToken);

        List<UserDto> result = [];

        foreach (ApplicationUser user in users)
        {
            IList<string> roles = await userManager.GetRolesAsync(user);
            string role = roles.FirstOrDefault() ?? RoleNames.User;
            result.Add(new UserDto(user.Id, user.UserName!, user.Email!, role));
        }

        return result;
    }

    public async Task<IReadOnlyList<string>> GetAssignableRolesAsync(CancellationToken cancellationToken = default)
    {
        return await roleManager.Roles
            .Select(role => role.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .OrderBy(name => name)
            .Select(name => name!)
            .ToListAsync(cancellationToken);
    }
}
