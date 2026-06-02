namespace api.DTOs;

public class PutEntryRequestDto
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required DateTimeOffset Date { get; set; }
    public required ICollection<EntryLineRequestDto> EntryLines { get; set; }
}