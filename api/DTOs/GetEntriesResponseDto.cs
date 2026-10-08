namespace api.DTOs;

public class GetEntriesResponseDto
{
   public required ICollection<EntryResponseDto> Entries { get; set; }
   public required int Page { get; set; }
   public required int PageSize { get; set; }
   public required int TotalCount { get; set; }
   public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
}
