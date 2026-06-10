using api.Data;
using api.DTOs;
using api.Exceptions;
using api.Models;
using api.Models.Enums;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class EntryService(AppDbContext db) : IEntryService
{
    private static readonly HashSet<NatureType> DebitNormalNatures = [NatureType.Asset, NatureType.Expense];

    private async Task ApplyBalanceDeltasAsync(IEnumerable<EntryLine> lines, bool reverse = false)
    {
        var accountIds = lines.Select(l => l.AccountId).Distinct().ToList();
        var accounts = await db.Accounts
            .Where(a => accountIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id);

        foreach (var line in lines)
        {
            var account = accounts[line.AccountId];
            bool isDebitNormal = DebitNormalNatures.Contains(account.Nature);
            decimal delta = (isDebitNormal == (line.Type == EntryLineType.Debit)) ? line.Amount : -line.Amount;
            account.Balance += reverse ? -delta : delta;
        }
    }

    public async Task<List<EntryResponseDto>> GetEntriesAsync(Guid userId)
    {
        var entries = await db.Entries
            .Where(e => e.UserId == userId)
            .Include(e => e.EntryLines)
            .ToListAsync();
        return entries.Adapt<List<EntryResponseDto>>();
    }

    public async Task<EntryResponseDto?> GetEntryAsync(Guid userId, Guid entryId)
    {
        var entry = await db.Entries
            .Include(e => e.EntryLines)
            .Where(e => e.Id == entryId && e.UserId == userId)
            .FirstOrDefaultAsync();
        return entry?.Adapt<EntryResponseDto>();
    }

    public async Task<EntryResponseDto> CreateEntryAsync(Guid userId, PostEntryRequestDto dto)
    {
        if (!dto.EntryLines.Any())
            throw new ValidationException("Entry must have at least one line.");

        var accountIds = dto.EntryLines.Select(l => l.AccountId).Distinct();
        var validAccounts = await db.Accounts
            .Where(a => a.UserId == userId && accountIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync();
        if (validAccounts.Count != accountIds.Count())
            throw new ValidationException("One or more accounts are invalid.");

        var entry = new Entry
        {
            Id = Guid.NewGuid(),
            Date = DateTime.SpecifyKind(dto.Date.UtcDateTime, DateTimeKind.Utc),
            Title = dto.Title,
            UserId = userId,
            Description = dto.Description
        };

        decimal sum = 0;
        foreach (var lineDto in dto.EntryLines)
        {
            if (lineDto.Amount <= 0)
                throw new ValidationException("Amount must be greater than 0.");

            var line = new EntryLine { Id = Guid.NewGuid(), AccountId = lineDto.AccountId, Amount = lineDto.Amount, Type = lineDto.Type, EntryId = entry.Id };
            sum += lineDto.Type == EntryLineType.Credit ? lineDto.Amount : -lineDto.Amount;
            entry.EntryLines.Add(line);
        }

        if (sum != 0)
            throw new ValidationException("This entry is out of balance. Check your debit and credit amounts.");

        await ApplyBalanceDeltasAsync(entry.EntryLines);
        db.Entries.Add(entry);
        await db.SaveChangesAsync();

        return new EntryResponseDto { Id = entry.Id, Date = entry.Date, Title = entry.Title, Description = entry.Description, EntryLines = entry.EntryLines.Adapt<List<EntryLineResponseDto>>() };
    }

    public async Task<EntryResponseDto?> UpdateEntryAsync(Guid userId, Guid entryId, PutEntryRequestDto dto)
    {
        var entry = await db.Entries
            .Include(e => e.EntryLines)
            .Where(e => e.Id == entryId && e.UserId == userId)
            .FirstOrDefaultAsync();

        if (entry == null)
            return null;

        if (!dto.EntryLines.Any())
            throw new ValidationException("Entry must have at least one line.");

        var accountIds = dto.EntryLines.Select(l => l.AccountId).Distinct();
        var validAccounts = await db.Accounts
            .Where(a => a.UserId == userId && accountIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync();
        if (validAccounts.Count != accountIds.Count())
            throw new ValidationException("One or more accounts are invalid.");

        decimal sum = 0;
        var newLines = new List<EntryLine>();
        foreach (var lineDto in dto.EntryLines)
        {
            if (lineDto.Amount <= 0)
                throw new ValidationException("Amount must be greater than 0.");

            sum += lineDto.Type == EntryLineType.Credit ? lineDto.Amount : -lineDto.Amount;
            newLines.Add(new EntryLine { Id = Guid.NewGuid(), AccountId = lineDto.AccountId, Amount = lineDto.Amount, Type = lineDto.Type, EntryId = entry.Id });
        }

        if (sum != 0)
            throw new ValidationException("This entry is out of balance. Check your debit and credit amounts.");

        await ApplyBalanceDeltasAsync(entry.EntryLines, reverse: true);
        await ApplyBalanceDeltasAsync(newLines);

        entry.Title = dto.Title;
        entry.Description = dto.Description;
        entry.Date = DateTime.SpecifyKind(dto.Date.UtcDateTime, DateTimeKind.Utc);

        db.EntryLines.RemoveRange(entry.EntryLines);
        db.EntryLines.AddRange(newLines);
        await db.SaveChangesAsync();

        return new EntryResponseDto { Id = entry.Id, Date = entry.Date, Title = entry.Title, Description = entry.Description, EntryLines = newLines.Adapt<List<EntryLineResponseDto>>() };
    }

    public async Task<bool> DeleteEntryAsync(Guid userId, Guid entryId)
    {
        var entry = await db.Entries
            .Include(e => e.EntryLines)
            .Where(e => e.Id == entryId && e.UserId == userId)
            .FirstOrDefaultAsync();

        if (entry == null)
            return false;

        await ApplyBalanceDeltasAsync(entry.EntryLines, reverse: true);
        db.Entries.Remove(entry);
        await db.SaveChangesAsync();
        return true;
    }
}
