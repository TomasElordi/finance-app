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
    private const string RetainedEarningsAccountName = "Resultados Anteriores";

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

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            await ApplyBalanceDeltasAsync(entry.EntryLines);
            db.Entries.Add(entry);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();
            throw new ConflictException("Account balance was modified concurrently. Please retry.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        return new EntryResponseDto { Id = entry.Id, Date = entry.Date, Title = entry.Title, Description = entry.Description, EntryLines = entry.EntryLines.Adapt<List<EntryLineResponseDto>>() };
    }

    public async Task<List<EntryResponseDto>> CreateEntriesAsync(Guid userId, ICollection<PostEntryRequestDto> dtos)
    {
        if (!dtos.Any())
            throw new ValidationException("Debe incluir al menos un asiento.");

        var accountIds = dtos.SelectMany(d => d.EntryLines.Select(l => l.AccountId)).Distinct().ToList();
        var validAccountIds = (await db.Accounts
            .Where(a => a.UserId == userId && accountIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync())
            .ToHashSet();

        var entries = new List<Entry>();
        var position = 0;
        foreach (var dto in dtos)
        {
            position++;

            if (!dto.EntryLines.Any())
                throw new ValidationException($"El asiento en la posición {position} debe tener al menos una línea.");

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
                if (!validAccountIds.Contains(lineDto.AccountId))
                    throw new ValidationException($"El asiento en la posición {position} hace referencia a una cuenta inválida.");

                if (lineDto.Amount <= 0)
                    throw new ValidationException($"El asiento en la posición {position} tiene un monto inválido.");

                var line = new EntryLine { Id = Guid.NewGuid(), AccountId = lineDto.AccountId, Amount = lineDto.Amount, Type = lineDto.Type, EntryId = entry.Id };
                sum += lineDto.Type == EntryLineType.Credit ? lineDto.Amount : -lineDto.Amount;
                entry.EntryLines.Add(line);
            }

            if (sum != 0)
                throw new ValidationException($"El asiento en la posición {position} está desbalanceado. Verificá los montos de débito y crédito.");

            entries.Add(entry);
        }

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            await ApplyBalanceDeltasAsync(entries.SelectMany(e => e.EntryLines));
            db.Entries.AddRange(entries);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();
            throw new ConflictException("Account balance was modified concurrently. Please retry.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        return entries.Select(entry => new EntryResponseDto
        {
            Id = entry.Id,
            Date = entry.Date,
            Title = entry.Title,
            Description = entry.Description,
            EntryLines = entry.EntryLines.Adapt<List<EntryLineResponseDto>>()
        }).ToList();
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

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            await ApplyBalanceDeltasAsync(entry.EntryLines, reverse: true);
            await ApplyBalanceDeltasAsync(newLines);

            entry.Title = dto.Title;
            entry.Description = dto.Description;
            entry.Date = DateTime.SpecifyKind(dto.Date.UtcDateTime, DateTimeKind.Utc);

            db.EntryLines.RemoveRange(entry.EntryLines);
            db.EntryLines.AddRange(newLines);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();
            throw new ConflictException("Account balance was modified concurrently. Please retry.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

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

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            await ApplyBalanceDeltasAsync(entry.EntryLines, reverse: true);
            db.Entries.Remove(entry);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();
            throw new ConflictException("Account balance was modified concurrently. Please retry.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        return true;
    }

    public async Task<EntryResponseDto> CreateClosingEntryAsync(Guid userId)
    {
        var resultAccounts = await db.Accounts
            .Where(a => a.UserId == userId && (a.Nature == NatureType.Income || a.Nature == NatureType.Expense) && a.Balance != 0)
            .ToListAsync();

        if (resultAccounts.Count == 0)
            throw new ValidationException("No hay resultados positivos o negativos para cerrar.");

        var retainedEarnings = await db.Accounts
            .Where(a => a.UserId == userId && a.Nature == NatureType.Equity && a.Name == RetainedEarningsAccountName)
            .FirstOrDefaultAsync();

        if (retainedEarnings == null)
        {
            retainedEarnings = new Account { Id = Guid.NewGuid(), UserId = userId, Name = RetainedEarningsAccountName, Nature = NatureType.Equity, Balance = 0 };
            db.Accounts.Add(retainedEarnings);
            await db.SaveChangesAsync();
        }

        var previousMonthEnd = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(-1);

        var entry = new Entry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = $"Cierre de resultados {previousMonthEnd:MM/yyyy}",
            Description = "Cierre automático de cuentas de Resultado Positivo y Negativo a Resultados Anteriores.",
            Date = previousMonthEnd,
        };

        decimal netResult = 0;
        foreach (var account in resultAccounts)
        {
            bool isDebitNormal = DebitNormalNatures.Contains(account.Nature);
            var closingType = isDebitNormal == account.Balance > 0 ? EntryLineType.Credit : EntryLineType.Debit;

            entry.EntryLines.Add(new EntryLine { Id = Guid.NewGuid(), AccountId = account.Id, Amount = Math.Abs(account.Balance), Type = closingType, EntryId = entry.Id });

            netResult += account.Nature == NatureType.Income ? account.Balance : -account.Balance;
        }

        if (netResult != 0)
        {
            var retainedEarningsType = netResult > 0 ? EntryLineType.Credit : EntryLineType.Debit;
            entry.EntryLines.Add(new EntryLine { Id = Guid.NewGuid(), AccountId = retainedEarnings.Id, Amount = Math.Abs(netResult), Type = retainedEarningsType, EntryId = entry.Id });
        }

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            await ApplyBalanceDeltasAsync(entry.EntryLines);
            db.Entries.Add(entry);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();
            throw new ConflictException("Account balance was modified concurrently. Please retry.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        return new EntryResponseDto { Id = entry.Id, Date = entry.Date, Title = entry.Title, Description = entry.Description, EntryLines = entry.EntryLines.Adapt<List<EntryLineResponseDto>>() };
    }
}
