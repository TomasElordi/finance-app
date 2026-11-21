using System.IdentityModel.Tokens.Jwt;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace api.Services;

// The parts of a validated access token that revocation needs.
public record AccessTokenInfo(string Jti, DateTime IssuedAt, DateTime ExpiresAt, string? ClientId)
{
    public const string HttpContextKey = "AccessTokenInfo";

    public static AccessTokenInfo? From(SecurityToken token) => token switch
    {
        JsonWebToken t when !string.IsNullOrEmpty(t.Id) => new(
            t.Id, t.IssuedAt, t.ValidTo,
            t.TryGetPayloadValue<string>(JwtService.ClientIdClaim, out var clientId) ? clientId : null),
        JwtSecurityToken t when !string.IsNullOrEmpty(t.Id) => new(
            t.Id, t.IssuedAt, t.ValidTo,
            t.Claims.FirstOrDefault(c => c.Type == JwtService.ClientIdClaim)?.Value),
        _ => null
    };
}

public class TokenRevocationService(AppDbContext db) : ITokenRevocationService
{
    public async Task<bool> IsRevokedAsync(Guid userId, AccessTokenInfo token)
    {
        if (await db.RevokedTokens.AnyAsync(r => r.Jti == token.Jti))
            return true;
        if (token.ClientId is null)
            return false;

        var revokedAt = await db.Users
            .Where(u => u.Id == userId)
            .Select(u => u.McpTokensRevokedAt)
            .FirstOrDefaultAsync();
        // iat has second precision, so compare at that precision.
        return revokedAt is { } at && token.IssuedAt <= at;
    }

    public async Task RevokeAsync(AccessTokenInfo token)
    {
        // Housekeeping: a revoked token past its expiry is rejected anyway.
        await db.RevokedTokens.Where(r => r.ExpiresAt < DateTime.UtcNow).ExecuteDeleteAsync();
        if (await db.RevokedTokens.AnyAsync(r => r.Jti == token.Jti))
            return;
        db.RevokedTokens.Add(new RevokedToken { Id = Guid.NewGuid(), Jti = token.Jti, ExpiresAt = token.ExpiresAt });
        await db.SaveChangesAsync();
    }

    public async Task<int> RevokeMcpClientsAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        await db.Users.Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.McpTokensRevokedAt, now));
        var clients = await db.OAuthRefreshTokens
            .Where(t => t.UserId == userId)
            .Select(t => t.ClientId)
            .Distinct()
            .CountAsync();
        await db.OAuthRefreshTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync();
        await db.OAuthAuthorizationCodes.Where(c => c.UserId == userId).ExecuteDeleteAsync();
        return clients;
    }
}

public interface ITokenRevocationService
{
    Task<bool> IsRevokedAsync(Guid userId, AccessTokenInfo token);
    Task RevokeAsync(AccessTokenInfo token);
    // Returns how many clients had an active connection.
    Task<int> RevokeMcpClientsAsync(Guid userId);
}
