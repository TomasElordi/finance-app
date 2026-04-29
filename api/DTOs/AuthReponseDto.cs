namespace api.DTOs;
public class AuthResponseDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required UserResponseDto User { get; set; } 
}