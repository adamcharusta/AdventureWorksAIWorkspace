using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AdventureWorksAIWorkspace.Application.Common.Dtos.Auth;
using AdventureWorksAIWorkspace.Infrastructure.Database;
using AdventureWorksAIWorkspace.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AdventureWorksAIWorkspace.Infrastructure.Services.Auth;

/// <summary>
/// Default <see cref="ITokenService"/>: signs JWT access tokens and persists hashed refresh tokens.
/// </summary>
internal sealed class TokenService(
    UserManager<ApplicationUser> userManager,
    AppDbContext dbContext,
    IOptions<JwtOptions> jwtOptions) : ITokenService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthTokens> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
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

    public async Task<AuthTokens?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
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

    public async Task RevokeAllRefreshTokensAsync(string userId, CancellationToken cancellationToken)
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
}
