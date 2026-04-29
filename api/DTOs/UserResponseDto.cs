namespace api.DTOs;
public class UserResponseDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; } 
}