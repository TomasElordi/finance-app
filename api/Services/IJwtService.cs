using api.DTOs;

namespace api.Services;
public interface IJwtService
{
   // clientId is set for tokens issued to OAuth (MCP) clients, so they can be revoked
   // separately from web sessions.
   TokensDto GenerateTokens(Guid id, string Name, string Email, string? clientId = null);
}
