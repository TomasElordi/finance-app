namespace api.DTOs;

public class EntryResponseDto
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set;}
    public required DateTime Date { get; set; }
    public required ICollection<EntryLineResponseDto> EntryLines { get; set; }
}