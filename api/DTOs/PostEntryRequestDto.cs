using System.ComponentModel.DataAnnotations;
using api.Models.Enums;

namespace api.DTOs;

public class PostEntryRequestDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string Title { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public required DateTimeOffset Date { get; set; }

    [Required]
    [MinLength(1)]
    public required ICollection<EntryLineRequestDto> EntryLines { get; set; }
}
