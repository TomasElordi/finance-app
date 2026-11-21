namespace api.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using api.DTOs;
using System.Security.Cryptography;

public class JwtService : IJwtService
{
    // Claim on access tokens issued to OAuth (MCP) clients.
    public const string ClientIdClaim = "client_id";
    public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(24);

    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public TokensDto GenerateTokens(Guid Id, string Name, string Email, string? clientId = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Id.ToString()),
            new(ClaimTypes.Email, Email),
            new(ClaimTypes.Name, Name),
            // jti lets a single token be revoked (logout); iat lets all MCP tokens
            // issued before a point in time be revoked.
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
        if (clientId is not null)
            claims.Add(new Claim(ClientIdClaim, clientId));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            notBefore: now,
            expires: now.Add(AccessTokenLifetime),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return new TokensDto
        {
            AccessToken = tokenString,
            RefreshToken = refreshToken
        };
    }
}
