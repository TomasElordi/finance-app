namespace api.Models;
using System.ComponentModel.DataAnnotations;

// OAuth 2.1 authorization server used by MCP clients (Claude) to act on behalf of a
// user. Clients register themselves (RFC 7591), users log in on /oauth/authorize, and
// the resulting access tokens are the same JWTs the REST API already accepts.
// Codes, secrets and refresh tokens are stored only as SHA-256 hashes.

public class OAuthClient
{
    [Key]
    public required Guid Id { get; set; }
    public required string ClientId { get; set; }
    // Null for public clients (PKCE only, token_endpoint_auth_method = none).
    public string? ClientSecretHash { get; set; }
    public required string Name { get; set; }
    public required List<string> RedirectUris { get; set; }
    public required DateTime CreatedAt { get; set; }
}

public class OAuthAuthorizationCode
{
    [Key]
    public required Guid Id { get; set; }
    public required string CodeHash { get; set; }
    public required Guid ClientId { get; set; }
    public OAuthClient Client { get; set; } = null!;
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required string RedirectUri { get; set; }
    // S256 PKCE challenge sent to /oauth/authorize.
    public required string CodeChallenge { get; set; }
    public required DateTime ExpiresAt { get; set; }
}

public class OAuthRefreshToken
{
    [Key]
    public required Guid Id { get; set; }
    public required string TokenHash { get; set; }
    public required Guid ClientId { get; set; }
    public OAuthClient Client { get; set; } = null!;
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required DateTime ExpiresAt { get; set; }
}
