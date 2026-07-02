using api.DTOs;

namespace api.Services;

public interface IEntryService
{
    Task<List<EntryResponseDto>> GetEntriesAsync(Guid userId);
    Task<EntryResponseDto?> GetEntryAsync(Guid userId, Guid entryId);
    Task<EntryResponseDto> CreateEntryAsync(Guid userId, PostEntryRequestDto dto);
    Task<EntryResponseDto?> UpdateEntryAsync(Guid userId, Guid entryId, PutEntryRequestDto dto);
    Task<bool> DeleteEntryAsync(Guid userId, Guid entryId);
    Task<EntryResponseDto> CreateClosingEntryAsync(Guid userId);
}
