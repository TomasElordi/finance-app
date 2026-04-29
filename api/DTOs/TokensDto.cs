namespace api.DTOs;
public class TokensDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}