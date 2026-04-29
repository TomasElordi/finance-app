namespace api.DTOs;

public class GetAccountsResponseDto
{
   public required ICollection<AccountResponseDto> Accounts { get; set; }
}