using System.ComponentModel.DataAnnotations;

namespace api.DTOs;

public class PostBulkEntryRequestDto
{
    [Required]
    [MinLength(1)]
    public required ICollection<PostEntryRequestDto> Entries { get; set; }
}
