using api.DTOs;

namespace api.Services;

public interface IEntryService
{
    Task<GetEntriesResponseDto> GetEntriesAsync(Guid userId, int page, int pageSize);
    Task<EntryResponseDto?> GetEntryAsync(Guid userId, Guid entryId);
    Task<EntryResponseDto> CreateEntryAsync(Guid userId, PostEntryRequestDto dto);
    Task<List<EntryResponseDto>> CreateEntriesAsync(Guid userId, ICollection<PostEntryRequestDto> dtos);
    Task<EntryResponseDto?> UpdateEntryAsync(Guid userId, Guid entryId, PutEntryRequestDto dto);
    Task<bool> DeleteEntryAsync(Guid userId, Guid entryId);
    Task<EntryResponseDto> CreateClosingEntryAsync(Guid userId);
}
