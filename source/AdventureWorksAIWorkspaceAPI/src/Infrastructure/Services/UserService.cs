using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspaceAPI.Application.Common.Dtos.User;
using AdventureWorksAIWorkspaceAPI.Application.Common.Services;
using AdventureWorksAIWorkspaceAPI.Application.User.UpdateUser;
using AdventureWorksAIWorkspaceAPI.Infrastructure.Database;
using AdventureWorksAIWorkspaceAPI.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AdventureWorksAIWorkspaceAPI.Infrastructure.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    AppDbContext dbContext,
    IOptions<JwtOptions> jwtOptions,
    IOptions<InitialAdminOptions> initialAdminOptions) : IUserService
{
    private readonly InitialAdminOptions _initialAdminOptions = initialAdminOptions.Value;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<LoginResult> LoginAsync(string identifier, string password,
        CancellationToken cancellationToken = default)
    {
        ApplicationUser? user = await userManager.FindByNameAsync(identifier)
                                ?? await userManager.FindByEmailAsync(identifier);

        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            return new LoginResult(LoginOutcome.InvalidCredentials, null);
        }

        if (user.IsFirstLogin)
        {
            return new LoginResult(LoginOutcome.PasswordChangeRequired, null);
        }

        AuthTokens tokens = await IssueTokensAsync(user, cancellationToken);
        return new LoginResult(LoginOutcome.Success, tokens);
    }

    public async Task<SetFirstPasswordResult> SetFirstPasswordAsync(string identifier, string confirmNewPassword,
        string newPassword, CancellationToken cancellationToken = default)
    {
        if (confirmNewPassword != newPassword)
        {
            return new SetFirstPasswordResult(SetFirstPasswordOutcome.PasswordChangeFailed, null,
                ["New password and confirmation do not match."]);
        }

        ApplicationUser? user = await userManager.FindByNameAsync(identifier)
                                ?? await userManager.FindByEmailAsync(identifier);

        if (user is null)
        {
            return new SetFirstPasswordResult(SetFirstPasswordOutcome.InvalidCredentials, null);
        }

        if (!user.IsFirstLogin)
        {
            return new SetFirstPasswordResult(SetFirstPasswordOutcome.PasswordChangeNotRequired, null);
        }

        string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        IdentityResult resetResult = await userManager.ResetPasswordAsync(user, resetToken, newPassword);

        if (!resetResult.Succeeded)
        {
            var errors = resetResult.Errors.Select(e => e.Description).ToList();
            return new SetFirstPasswordResult(SetFirstPasswordOutcome.PasswordChangeFailed, null, errors);
        }

        user.IsFirstLogin = false;
        await userManager.UpdateAsync(user);

        await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);

        AuthTokens tokens = await IssueTokensAsync(user, cancellationToken);
        return new SetFirstPasswordResult(SetFirstPasswordOutcome.Success, tokens);
    }

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
            await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
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

        await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);

        IdentityResult deleteResult = await userManager.DeleteAsync(user);

        if (!deleteResult.Succeeded)
        {
            var errors = deleteResult.Errors.Select(e => e.Description).ToList();
            return new DeleteUserResult(DeleteUserOutcome.DeleteFailed, errors);
        }

        return new DeleteUserResult(DeleteUserOutcome.Success);
    }

    public async Task<AuthTokens?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        string hash = HashToken(refreshToken);

        RefreshToken? existing = await dbContext.RefreshTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (existing is null || !existing.IsActive || existing.User is null)
        {
            return null;
        }

        existing.RevokedAt = DateTime.UtcNow;

        return await IssueTokensAsync(existing.User, cancellationToken);
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

    private async Task<AuthTokens> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        IList<string> roles = await userManager.GetRolesAsync(user);

        DateTime now = DateTime.UtcNow;
        DateTime accessExpires = now.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes);
        DateTime refreshExpires = now.AddDays(_jwtOptions.RefreshTokenLifetimeDays);

        string accessToken = CreateAccessToken(user, roles, now, accessExpires);
        string refreshTokenValue = GenerateRefreshTokenValue();

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id, TokenHash = HashToken(refreshTokenValue), CreatedAt = now, ExpiresAt = refreshExpires
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthTokens(accessToken, accessExpires, refreshTokenValue, refreshExpires);
    }

    private string CreateAccessToken(ApplicationUser user, IList<string> roles, DateTime issuedAt, DateTime expiresAt)
    {
        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        foreach (string role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor descriptor = new()
        {
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            Subject = new ClaimsIdentity(claims),
            IssuedAt = issuedAt,
            NotBefore = issuedAt,
            Expires = expiresAt,
            SigningCredentials = credentials
        };

        JsonWebTokenHandler handler = new();
        return handler.CreateToken(descriptor);
    }

    private static string GenerateRefreshTokenValue()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    private async Task RevokeAllRefreshTokensAsync(string userId, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;

        List<RefreshToken> activeTokens = await dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (RefreshToken token in activeTokens)
        {
            token.RevokedAt = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
