using System.Security.Cryptography;
using System.Text;
using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

// Thrown for OAuth protocol errors; Error is the RFC 6749 error code.
public class OAuthException(string error, string description) : Exception(description)
{
    public string Error { get; } = error;
}

public record OAuthTokenResult(string AccessToken, string RefreshToken, int ExpiresIn);

public record OAuthRegisteredClient(OAuthClient Client, string? ClientSecret);

public class OAuthService(AppDbContext db, IJwtService jwtService, IConfiguration configuration) : IOAuthService
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RefreshLifetime = TimeSpan.FromDays(90);
    // Must match the lifetime JwtService gives access tokens.
    private const int AccessTokenSeconds = 24 * 60 * 60;
    private const int MaxRedirectUris = 10;

    // Only these hosts (plus loopback, for local tools like MCP Inspector) may receive
    // authorization codes. Anyone can register a client, so without this an attacker
    // could register their own redirect and phish a user into authorizing it.
    private HashSet<string> AllowedRedirectHosts =>
        (configuration["OAuth:AllowedRedirectHosts"] ?? "claude.ai,claude.com")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public async Task<OAuthRegisteredClient> RegisterClientAsync(
        string? clientName, List<string>? redirectUris, string? tokenEndpointAuthMethod)
    {
        if (redirectUris is null || redirectUris.Count == 0)
            throw new OAuthException("invalid_redirect_uri", "At least one redirect_uri is required.");
        if (redirectUris.Count > MaxRedirectUris)
            throw new OAuthException("invalid_redirect_uri", "Too many redirect_uris.");
        foreach (var uri in redirectUris)
        {
            if (!IsAllowedRedirectUri(uri))
                throw new OAuthException("invalid_redirect_uri", $"redirect_uri not allowed: {uri}");
        }

        var method = tokenEndpointAuthMethod ?? "none";
        if (method is not ("none" or "client_secret_post" or "client_secret_basic"))
            throw new OAuthException("invalid_client_metadata", $"Unsupported token_endpoint_auth_method: {method}");

        string? secret = method == "none" ? null : NewToken();
        var name = clientName?.Trim() ?? "";
        var client = new OAuthClient
        {
            Id = Guid.NewGuid(),
            ClientId = NewToken(24),
            ClientSecretHash = secret is null ? null : Hash(secret),
            Name = name.Length == 0 ? "MCP client" : name[..Math.Min(name.Length, 200)],
            RedirectUris = redirectUris,
            CreatedAt = DateTime.UtcNow
        };
        db.OAuthClients.Add(client);
        await db.SaveChangesAsync();
        return new OAuthRegisteredClient(client, secret);
    }

    // Validates an authorization request before showing the login page. Errors here are
    // shown to the user instead of redirected, since the redirect itself can't be trusted.
    public async Task<OAuthClient> ValidateAuthorizeRequestAsync(
        string? clientId, string? redirectUri, string? responseType, string? codeChallenge, string? codeChallengeMethod)
    {
        var client = await db.OAuthClients.FirstOrDefaultAsync(c => c.ClientId == clientId)
            ?? throw new OAuthException("invalid_client", "Unknown client.");
        if (redirectUri is null || !client.RedirectUris.Contains(redirectUri))
            throw new OAuthException("invalid_request", "redirect_uri doesn't match the registered ones.");
        if (responseType != "code")
            throw new OAuthException("unsupported_response_type", "Only response_type=code is supported.");
        if (string.IsNullOrEmpty(codeChallenge) || codeChallengeMethod != "S256")
            throw new OAuthException("invalid_request", "PKCE with code_challenge_method=S256 is required.");
        return client;
    }

    public async Task<string> CreateAuthorizationCodeAsync(OAuthClient client, Guid userId, string redirectUri, string codeChallenge)
    {
        // Housekeeping: codes are short-lived, drop the expired ones.
        await db.OAuthAuthorizationCodes.Where(c => c.ExpiresAt < DateTime.UtcNow).ExecuteDeleteAsync();

        var code = NewToken();
        db.OAuthAuthorizationCodes.Add(new OAuthAuthorizationCode
        {
            Id = Guid.NewGuid(),
            CodeHash = Hash(code),
            ClientId = client.Id,
            UserId = userId,
            RedirectUri = redirectUri,
            CodeChallenge = codeChallenge,
            ExpiresAt = DateTime.UtcNow.Add(CodeLifetime)
        });
        await db.SaveChangesAsync();
        return code;
    }

    public async Task<OAuthTokenResult> ExchangeCodeAsync(
        string? clientId, string? clientSecret, string? code, string? redirectUri, string? codeVerifier)
    {
        var client = await AuthenticateClientAsync(clientId, clientSecret);
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(codeVerifier))
            throw new OAuthException("invalid_request", "code and code_verifier are required.");

        var codeHash = Hash(code);
        var stored = await db.OAuthAuthorizationCodes
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CodeHash == codeHash);
        if (stored is null)
            throw new OAuthException("invalid_grant", "Invalid authorization code.");

        // Single use: delete before checking anything else so a code can't be retried.
        db.OAuthAuthorizationCodes.Remove(stored);
        await db.SaveChangesAsync();

        if (stored.ExpiresAt < DateTime.UtcNow)
            throw new OAuthException("invalid_grant", "Authorization code expired.");
        if (stored.ClientId != client.Id)
            throw new OAuthException("invalid_grant", "Authorization code was issued to another client.");
        if (stored.RedirectUri != redirectUri)
            throw new OAuthException("invalid_grant", "redirect_uri doesn't match the authorization request.");
        if (!FixedTimeEquals(Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier))), stored.CodeChallenge))
            throw new OAuthException("invalid_grant", "Invalid code_verifier.");

        return await IssueTokensAsync(client, stored.User);
    }

    // Refresh tokens rotate: each use returns a new one and invalidates the old.
    public async Task<OAuthTokenResult> RefreshAsync(string? clientId, string? clientSecret, string? refreshToken)
    {
        var client = await AuthenticateClientAsync(clientId, clientSecret);
        if (string.IsNullOrEmpty(refreshToken))
            throw new OAuthException("invalid_request", "refresh_token is required.");

        var tokenHash = Hash(refreshToken);
        var stored = await db.OAuthRefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        if (stored is null || stored.ClientId != client.Id || stored.ExpiresAt < DateTime.UtcNow)
            throw new OAuthException("invalid_grant", "Invalid or expired refresh token.");

        db.OAuthRefreshTokens.Remove(stored);
        return await IssueTokensAsync(client, stored.User);
    }

    private async Task<OAuthTokenResult> IssueTokensAsync(OAuthClient client, User user)
    {
        var accessToken = jwtService.GenerateTokens(user.Id, user.Name, user.Email).AccessToken;
        var refreshToken = NewToken();
        db.OAuthRefreshTokens.Add(new OAuthRefreshToken
        {
            Id = Guid.NewGuid(),
            TokenHash = Hash(refreshToken),
            ClientId = client.Id,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.Add(RefreshLifetime)
        });
        await db.SaveChangesAsync();
        return new OAuthTokenResult(accessToken, refreshToken, AccessTokenSeconds);
    }

    private async Task<OAuthClient> AuthenticateClientAsync(string? clientId, string? clientSecret)
    {
        var client = await db.OAuthClients.FirstOrDefaultAsync(c => c.ClientId == clientId)
            ?? throw new OAuthException("invalid_client", "Unknown client.");
        if (client.ClientSecretHash is not null &&
            (clientSecret is null || !FixedTimeEquals(Hash(clientSecret), client.ClientSecretHash)))
            throw new OAuthException("invalid_client", "Invalid client credentials.");
        return client;
    }

    private bool IsAllowedRedirectUri(string uri)
    {
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed) || !string.IsNullOrEmpty(parsed.Fragment))
            return false;
        if (parsed.IsLoopback)
            return parsed.Scheme is "http" or "https";
        return parsed.Scheme == "https" && AllowedRedirectHosts.Contains(parsed.Host);
    }

    private static string NewToken(int bytes = 32) => Base64Url(RandomNumberGenerator.GetBytes(bytes));

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}
