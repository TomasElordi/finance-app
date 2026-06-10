using System.ComponentModel.DataAnnotations;

namespace api.DTOs;

public class RegisterRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public required string Password { get; set; }
}
