namespace api.Models;
using System.ComponentModel.DataAnnotations;
using api.Models.Enums;

public class EntryLine
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid EntryId { get; set; }
    public Entry Entry { get; set; } = null!;
    public required Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public required decimal Amount { get; set; }
    public required EntryLineType Type { get; set;}
}

