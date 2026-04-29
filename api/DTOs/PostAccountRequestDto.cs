namespace api.DTOs;
using api.Models.Enums;

public class PostAccountRequestDto
{
    public required string Name { get; set; }
    public required NatureType Nature { get; set; }
}