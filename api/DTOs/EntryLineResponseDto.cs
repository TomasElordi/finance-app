namespace api.DTOs;

using api.Models;
using api.Models.Enums;

public class EntryLineResponseDto
{
    public required Guid Id { get; set; }
    public required Guid AccountId { get; set; }
    public required decimal Amount { get; set; }
    public required EntryLineType Type { get; set;}
}