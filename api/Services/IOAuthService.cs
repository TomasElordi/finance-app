using api.Models;

namespace api.Services;

public interface IOAuthService
{
    Task<OAuthRegisteredClient> RegisterClientAsync(string? clientName, List<string>? redirectUris, string? tokenEndpointAuthMethod);
    Task<OAuthClient> ValidateAuthorizeRequestAsync(string? clientId, string? redirectUri, string? responseType, string? codeChallenge, string? codeChallengeMethod);
    Task<string> CreateAuthorizationCodeAsync(OAuthClient client, Guid userId, string redirectUri, string codeChallenge);
    Task<OAuthTokenResult> ExchangeCodeAsync(string? clientId, string? clientSecret, string? code, string? redirectUri, string? codeVerifier);
    Task<OAuthTokenResult> RefreshAsync(string? clientId, string? clientSecret, string? refreshToken);
}
