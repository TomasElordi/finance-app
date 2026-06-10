using api.Data;
using api.DTOs;
using api.Exceptions;
using api.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class AccountService(AppDbContext db) : IAccountService
{
    public async Task<List<AccountResponseDto>> GetAccountsAsync(Guid userId)
    {
        var accounts = await db.Accounts.Where(a => a.UserId == userId).ToListAsync();
        return accounts.Adapt<List<AccountResponseDto>>();
    }

    public async Task<AccountResponseDto> CreateAccountAsync(Guid userId, PostAccountRequestDto dto)
    {
        var account = new Account { Id = Guid.NewGuid(), Name = dto.Name, Nature = dto.Nature, UserId = userId };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return new AccountResponseDto { Id = account.Id, Name = account.Name, Nature = account.Nature, Balance = account.Balance };
    }

    public async Task<bool> DeleteAccountAsync(Guid userId, Guid accountId)
    {
        var account = await db.Accounts.Where(a => a.Id == accountId).FirstOrDefaultAsync();

        if (account == null)
            return false;

        if (account.UserId != userId)
            throw new ForbiddenException("Unauthorized.");

        var hasEntryLines = await db.EntryLines.AnyAsync(el => el.AccountId == accountId);
        if (hasEntryLines)
            throw new ConflictException("Cannot delete an account that has associated journal entries.");

        db.Accounts.Remove(account);
        await db.SaveChangesAsync();
        return true;
    }
}
