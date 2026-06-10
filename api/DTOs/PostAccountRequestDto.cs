using System.ComponentModel.DataAnnotations;
using api.Models.Enums;

namespace api.DTOs;

public class PostAccountRequestDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    [Required]
    public required NatureType Nature { get; set; }
}
