namespace api.DTOs;

public class PutEntryRequestDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTime Date { get; set; }
    public required ICollection<EntryLineRequestDto> EntryLines { get; set; }
}