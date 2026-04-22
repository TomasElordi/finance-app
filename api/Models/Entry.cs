namespace api.Models;
using System.ComponentModel.DataAnnotations;

public class Entry
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required string Title { get; set; }
    public string? Description { get; set;}
    public required DateTime Date { get; set; }
    public ICollection<EntryLine> EntryLines { get; set; } = [];
}