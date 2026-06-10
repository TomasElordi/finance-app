using System.ComponentModel.DataAnnotations;

namespace api.DTOs;

public class RefreshRequestDto
{
    [Required]
    public required string RefreshToken { get; set; }
}
