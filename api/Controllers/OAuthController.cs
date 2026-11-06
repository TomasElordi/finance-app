using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace api.Controllers;

// OAuth 2.1 authorization server for MCP clients, following the MCP authorization spec:
// protected resource metadata (RFC 9728), authorization server metadata (RFC 8414),
// dynamic client registration (RFC 7591) and authorization code + PKCE.
// Connecting from Claude only needs the /mcp URL; everything else is discovered.
[ApiExplorerSettings(IgnoreApi = true)]
public class OAuthController(ILogger<OAuthController> logger, IOAuthService oauth, IAuthService auth, IConfiguration configuration)
    : ControllerBase
{
    private const string McpPath = "/mcp";

    // Public origin of the API, e.g. https://xxx.execute-api.us-east-1.amazonaws.com.
    // Behind API Gateway the scheme comes from X-Forwarded-Proto.
    private string Issuer
    {
        get
        {
            var configured = configuration["OAuth:Issuer"];
            if (!string.IsNullOrEmpty(configured)) return configured.TrimEnd('/');
            var proto = Request.Headers["X-Forwarded-Proto"].FirstOrDefault()?.Split(',')[0].Trim() ?? Request.Scheme;
            return $"{proto}://{Request.Host}";
        }
    }

    // ---------- Discovery ----------

    [HttpGet("/.well-known/oauth-protected-resource")]
    [HttpGet("/.well-known/oauth-protected-resource/mcp")]
    public IActionResult ProtectedResourceMetadata() => Ok(new
    {
        resource = Issuer + McpPath,
        authorization_servers = new[] { Issuer },
        bearer_methods_supported = new[] { "header" },
        resource_name = "Finance App"
    });

    [HttpGet("/.well-known/oauth-authorization-server")]
    public IActionResult AuthorizationServerMetadata() => Ok(new
    {
        issuer = Issuer,
        authorization_endpoint = $"{Issuer}/oauth/authorize",
        token_endpoint = $"{Issuer}/oauth/token",
        registration_endpoint = $"{Issuer}/oauth/register",
        response_types_supported = new[] { "code" },
        grant_types_supported = new[] { "authorization_code", "refresh_token" },
        code_challenge_methods_supported = new[] { "S256" },
        token_endpoint_auth_methods_supported = new[] { "none", "client_secret_post", "client_secret_basic" },
        authorization_response_iss_parameter_supported = true
    });

    // ---------- Dynamic client registration ----------

    public class RegisterRequest
    {
        [JsonPropertyName("client_name")] public string? ClientName { get; set; }
        [JsonPropertyName("redirect_uris")] public List<string>? RedirectUris { get; set; }
        [JsonPropertyName("token_endpoint_auth_method")] public string? TokenEndpointAuthMethod { get; set; }
    }

    [HttpPost("/oauth/register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var (client, secret) = await oauth.RegisterClientAsync(request.ClientName, request.RedirectUris, request.TokenEndpointAuthMethod);
            logger.LogInformation("OAuth client registered: {ClientName} ({ClientId})", client.Name, client.ClientId);
            NoStore();
            return StatusCode(201, new
            {
                client_id = client.ClientId,
                client_secret = secret,
                client_id_issued_at = new DateTimeOffset(client.CreatedAt).ToUnixTimeSeconds(),
                client_secret_expires_at = secret is null ? (long?)null : 0,
                client_name = client.Name,
                redirect_uris = client.RedirectUris,
                token_endpoint_auth_method = secret is null ? "none" : request.TokenEndpointAuthMethod,
                grant_types = new[] { "authorization_code", "refresh_token" },
                response_types = new[] { "code" }
            });
        }
        catch (OAuthException ex)
        {
            logger.LogWarning("OAuth client registration rejected: {Error} {Description}", ex.Error, ex.Message);
            return BadRequest(new { error = ex.Error, error_description = ex.Message });
        }
    }

    // ---------- Authorization (login page) ----------

    // The same parameters arrive in the query (GET) and are echoed back in the login form (POST).
    public record AuthorizeRequest(
        string? ResponseType, string? ClientId, string? RedirectUri, string? State, string? Scope,
        string? Resource, string? CodeChallenge, string? CodeChallengeMethod)
    {
        public static AuthorizeRequest From(Func<string, string?> get) => new(
            get("response_type"), get("client_id"), get("redirect_uri"), get("state"), get("scope"),
            get("resource"), get("code_challenge"), get("code_challenge_method"));
    }

    [HttpGet("/oauth/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var request = AuthorizeRequest.From(k => Request.Query.TryGetValue(k, out var v) ? v.ToString() : null);
        try
        {
            var client = await ValidateAsync(request);
            return LoginPage(request, client.Name, email: null, error: null);
        }
        catch (OAuthException ex)
        {
            return ErrorPage(ex.Message);
        }
    }

    [HttpPost("/oauth/authorize")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> AuthorizeSubmit([FromForm] IFormCollection form)
    {
        var request = AuthorizeRequest.From(k => form.TryGetValue(k, out var v) ? v.ToString() : null);
        string? email = form["email"], password = form["password"], action = form["action"];
        Models.OAuthClient client;
        try
        {
            client = await ValidateAsync(request);
        }
        catch (OAuthException ex)
        {
            return ErrorPage(ex.Message);
        }

        if (action == "cancel")
            return RedirectToClient(request, ("error", "access_denied"));

        var user = string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password)
            ? null
            : await auth.ValidateCredentialsAsync(email, password);
        if (user is null)
        {
            logger.LogInformation("OAuth login failed for client {ClientId}", client.ClientId);
            return LoginPage(request, client.Name, email, "Email o contraseña incorrectos.");
        }

        var code = await oauth.CreateAuthorizationCodeAsync(client, user.Id, request.RedirectUri!, request.CodeChallenge!);
        logger.LogInformation("OAuth authorization granted to {ClientId} by user {UserId}", client.ClientId, user.Id);
        return RedirectToClient(request, ("code", code));
    }

    private Task<Models.OAuthClient> ValidateAsync(AuthorizeRequest r) =>
        oauth.ValidateAuthorizeRequestAsync(r.ClientId, r.RedirectUri, r.ResponseType, r.CodeChallenge, r.CodeChallengeMethod);

    private IActionResult RedirectToClient(AuthorizeRequest request, (string Key, string Value) result)
    {
        var query = new Dictionary<string, string?> { [result.Key] = result.Value, ["iss"] = Issuer };
        if (request.State is not null) query["state"] = request.State;
        return Redirect(QueryHelpers.AddQueryString(request.RedirectUri!, query));
    }

    // ---------- Token ----------

    [HttpPost("/oauth/token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Token([FromForm] IFormCollection form)
    {
        NoStore();
        var (clientId, clientSecret) = ClientCredentials(form);
        try
        {
            var result = form["grant_type"].ToString() switch
            {
                "authorization_code" => await oauth.ExchangeCodeAsync(
                    clientId, clientSecret, form["code"], form["redirect_uri"], form["code_verifier"]),
                "refresh_token" => await oauth.RefreshAsync(clientId, clientSecret, form["refresh_token"]),
                _ => throw new OAuthException("unsupported_grant_type", "Unsupported grant_type.")
            };
            return Ok(new
            {
                access_token = result.AccessToken,
                token_type = "Bearer",
                expires_in = result.ExpiresIn,
                refresh_token = result.RefreshToken
            });
        }
        catch (OAuthException ex)
        {
            logger.LogWarning("OAuth token request rejected: {Error} {Description}", ex.Error, ex.Message);
            var body = new { error = ex.Error, error_description = ex.Message };
            return ex.Error == "invalid_client" ? Unauthorized(body) : BadRequest(body);
        }
    }

    // client_secret_basic (Authorization header) or client_secret_post / public (form).
    private (string? Id, string? Secret) ClientCredentials(IFormCollection form)
    {
        var header = Request.Headers.Authorization.ToString();
        if (header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header[6..].Trim()));
                var separator = decoded.IndexOf(':');
                if (separator > 0)
                    return (Uri.UnescapeDataString(decoded[..separator]), Uri.UnescapeDataString(decoded[(separator + 1)..]));
            }
            catch (FormatException)
            {
                // Fall back to form credentials.
            }
        }
        var secret = form["client_secret"].ToString();
        return (form["client_id"], string.IsNullOrEmpty(secret) ? null : secret);
    }

    private void NoStore()
    {
        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";
    }

    // ---------- HTML ----------

    private ContentResult LoginPage(AuthorizeRequest r, string clientName, string? email, string? error)
    {
        static string E(string? v) => WebUtility.HtmlEncode(v ?? "");
        var redirectHost = Uri.TryCreate(r.RedirectUri, UriKind.Absolute, out var u) ? u.Host : "";

        var hidden = new StringBuilder();
        foreach (var (name, value) in new[]
                 {
                     ("response_type", r.ResponseType), ("client_id", r.ClientId), ("redirect_uri", r.RedirectUri),
                     ("state", r.State), ("scope", r.Scope), ("resource", r.Resource),
                     ("code_challenge", r.CodeChallenge), ("code_challenge_method", r.CodeChallengeMethod)
                 })
        {
            if (value is not null)
                hidden.Append($"<input type=\"hidden\" name=\"{name}\" value=\"{E(value)}\">");
        }

        var body = $"""
            <h1>Conectar con Finance App</h1>
            <p><strong>{E(clientName)}</strong> quiere acceder a tu cuenta para ver y cargar datos en tu nombre.</p>
            <p class="muted">Después de ingresar vas a volver a <strong>{E(redirectHost)}</strong>.</p>
            {(error is null ? "" : $"<p class=\"error\">{E(error)}</p>")}
            <form method="post" action="/oauth/authorize">
              {hidden}
              <label>Email<input type="email" name="email" value="{E(email)}" autocomplete="username" required autofocus></label>
              <label>Contraseña<input type="password" name="password" autocomplete="current-password" required></label>
              <button type="submit" name="action" value="login">Ingresar y autorizar</button>
              <button type="submit" name="action" value="cancel" class="secondary" formnovalidate>Cancelar</button>
            </form>
            """;
        return Html(body, 200);
    }

    private ContentResult ErrorPage(string message) =>
        Html($"""
            <h1>No se pudo conectar</h1>
            <p>La solicitud de autorización no es válida.</p>
            <p class="muted">{WebUtility.HtmlEncode(message)}</p>
            """, 400);

    private ContentResult Html(string body, int status)
    {
        NoStore();
        // The login page must never be framed (clickjacking) or leak the URL.
        Response.Headers.XFrameOptions = "DENY";
        Response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; form-action 'self' https: http://localhost:* http://127.0.0.1:*; frame-ancestors 'none'";
        Response.Headers["Referrer-Policy"] = "no-referrer";
        return new ContentResult
        {
            StatusCode = status,
            ContentType = "text/html; charset=utf-8",
            Content = $$"""
                <!doctype html>
                <html lang="es">
                <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>Finance App</title>
                <style>
                  :root { color-scheme: light dark; --fg:#111; --bg:#f6f6f7; --card:#fff; --muted:#666; --border:#ddd; --accent:#111; --accent-fg:#fff; --error:#c0262d; }
                  @media (prefers-color-scheme: dark) { :root { --fg:#eee; --bg:#111; --card:#1b1b1d; --muted:#999; --border:#333; --accent:#eee; --accent-fg:#111; --error:#ff6b6b; } }
                  * { box-sizing: border-box; }
                  body { margin:0; min-height:100vh; display:flex; align-items:center; justify-content:center; padding:16px; background:var(--bg); color:var(--fg); font:15px/1.5 system-ui, sans-serif; }
                  main { width:100%; max-width:380px; background:var(--card); border:1px solid var(--border); border-radius:12px; padding:24px; }
                  h1 { font-size:20px; margin:0 0 12px; }
                  p { margin:0 0 12px; }
                  .muted { color:var(--muted); font-size:13px; }
                  .error { color:var(--error); }
                  form { display:flex; flex-direction:column; gap:12px; margin-top:16px; }
                  label { display:flex; flex-direction:column; gap:4px; font-size:13px; font-weight:500; }
                  input { font:inherit; padding:8px 10px; border:1px solid var(--border); border-radius:8px; background:transparent; color:inherit; }
                  button { font:inherit; font-weight:500; padding:9px; border-radius:8px; border:1px solid var(--accent); background:var(--accent); color:var(--accent-fg); cursor:pointer; }
                  button.secondary { background:transparent; color:var(--fg); border-color:var(--border); }
                </style>
                </head>
                <body><main>{{body}}</main></body>
                </html>
                """
        };
    }
}
