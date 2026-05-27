namespace api.DTOs;

public class GetEntriesResponseDto
{
   public required ICollection<EntryResponseDto> Entries { get; set; }
}