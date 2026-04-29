using api.DTOs;

namespace api.Services;
public interface IJwtService
{
   TokensDto GenerateTokens(Guid id, string Name, string Email);
}