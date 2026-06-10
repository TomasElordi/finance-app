using api.DTOs;

namespace api.Services;

public interface IAccountService
{
    Task<List<AccountResponseDto>> GetAccountsAsync(Guid userId);
    Task<AccountResponseDto> CreateAccountAsync(Guid userId, PostAccountRequestDto dto);
    Task<bool> DeleteAccountAsync(Guid userId, Guid accountId);
}
