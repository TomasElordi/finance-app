using api.DTOs;

namespace api.Services;

public interface ICardService
{
    Task<CardStatementDto> GetStatementAsync(Guid userId, int year, int month);
    Task<CardUpcomingDto> GetUpcomingAsync(Guid userId, int year, int month);
    Task<CardItemResponseDto> CreateItemAsync(Guid userId, CardItemRequestDto request);
    Task<CardItemResponseDto> UpdateItemAsync(Guid userId, Guid itemId, CardItemRequestDto request);
    Task DeleteItemAsync(Guid userId, Guid itemId);
    Task<List<PersonDto>> GetPeopleAsync(Guid userId);
    Task<PersonDto> CreatePersonAsync(Guid userId, CreatePersonRequestDto request);
    Task DeletePersonAsync(Guid userId, Guid personId);
}
