using System.ComponentModel.DataAnnotations;
using api.Models.Enums;

namespace api.DTOs;

public class EntryLineRequestDto
{
    [Required]
    public required Guid AccountId { get; set; }

    [Range(0.01, (double)decimal.MaxValue)]
    public required decimal Amount { get; set; }

    [Required]
    public required EntryLineType Type { get; set; }
}
