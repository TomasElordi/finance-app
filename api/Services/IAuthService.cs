using api.DTOs;

namespace api.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
    Task<AuthResponseDto> RefreshAsync(string refreshToken);
    Task LogoutAsync(Guid userId);
}
