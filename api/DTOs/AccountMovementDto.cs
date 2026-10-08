using api.Models.Enums;

namespace api.DTOs;

public class AccountMovementDto
{
    public required Guid EntryId { get; set; }
    public required string EntryTitle { get; set; }
    public required DateTime Date { get; set; }
    public required EntryLineType Type { get; set; }
    public required decimal Amount { get; set; }
}
