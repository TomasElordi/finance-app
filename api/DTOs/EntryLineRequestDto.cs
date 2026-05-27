namespace api.DTOs;

using api.Models.Enums;

public class EntryLineRequestDto
{
    public required Guid AccountId { get; set; }
    public required decimal Amount { get; set; }
    public required EntryLineType Type { get; set;}
}