using System.ComponentModel.DataAnnotations;

namespace api.DTOs;

public class PutEntryRequestDto
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
