namespace api.DTOs;
using api.Models.Enums;

public class AccountResponseDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required NatureType Nature { get; set; }
}