using System.Security.Cryptography;
using api.Data;
using api.DTOs;
using api.Exceptions;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class SharedGroupService(AppDbContext db) : ISharedGroupService
{
    private const int MaxMembers = 20;
    private const int InviteCodeLength = 8;
    // No 0/O/1/I/L so codes are easy to dictate.
    private const string InviteAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
    private static readonly string[] DefaultCategories =
        ["Supermercado", "Alquiler", "Servicios", "Salidas", "Transporte", "Otros"];

    // ---------- Groups ----------

    public async Task<List<SharedGroupSummaryDto>> GetGroupsAsync(Guid userId)
    {
        // Include can't follow a Select through a navigation, so filter the groups directly.
        var groups = await db.SharedGroups
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .Include(g => g.Members).ThenInclude(m => m.User)
            .OrderBy(g => g.Name)
            .ToListAsync();

        var result = new List<SharedGroupSummaryDto>();
        foreach (var group in groups)
        {
            var balances = await ComputeBalancesAsync(group.Id);
            result.Add(new SharedGroupSummaryDto
            {
                Id = group.Id,
                Name = group.Name,
                MemberNames = group.Members.OrderBy(m => m.JoinedAt).Select(m => m.User.Name).ToList(),
                MyBalance = balances.GetValueOrDefault(userId).Balance
            });
        }
        return result;
    }

    public async Task<SharedGroupDetailDto> CreateGroupAsync(Guid userId, SharedNameRequestDto request)
    {
        var name = ValidateName(request.Name, 100, "El nombre del grupo");
        var now = DateTime.UtcNow;
        var group = new SharedGroup
        {
            Id = Guid.NewGuid(),
            Name = name,
            InviteCode = await NewInviteCodeAsync(),
            CreatedAt = now
        };
        group.Members.Add(new SharedGroupMember { Id = Guid.NewGuid(), GroupId = group.Id, UserId = userId, JoinedAt = now });
        foreach (var category in DefaultCategories)
            group.Categories.Add(new SharedCategory { Id = Guid.NewGuid(), GroupId = group.Id, Name = category });

        db.SharedGroups.Add(group);
        await db.SaveChangesAsync();
        return await BuildDetailAsync(userId, group.Id);
    }

    public async Task<SharedGroupDetailDto> JoinGroupAsync(Guid userId, JoinSharedGroupRequestDto request)
    {
        var code = (request.Code ?? "").Trim().ToUpperInvariant();
        var group = await db.SharedGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.InviteCode == code)
            ?? throw new NotFoundException("El código de invitación no es válido.");

        if (group.Members.Any(m => m.UserId == userId))
            return await BuildDetailAsync(userId, group.Id);
        if (group.Members.Count >= MaxMembers)
            throw new ValidationException($"Un grupo no puede tener más de {MaxMembers} miembros.");

        db.SharedGroupMembers.Add(new SharedGroupMember
        {
            Id = Guid.NewGuid(),
            GroupId = group.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return await BuildDetailAsync(userId, group.Id);
    }

    public async Task<SharedGroupDetailDto> GetGroupAsync(Guid userId, Guid groupId)
    {
        await EnsureMemberAsync(userId, groupId);
        return await BuildDetailAsync(userId, groupId);
    }

    public async Task<SharedGroupDetailDto> RenameGroupAsync(Guid userId, Guid groupId, SharedNameRequestDto request)
    {
        await EnsureMemberAsync(userId, groupId);
        var name = ValidateName(request.Name, 100, "El nombre del grupo");
        var group = await db.SharedGroups.FirstAsync(g => g.Id == groupId);
        group.Name = name;
        await db.SaveChangesAsync();
        return await BuildDetailAsync(userId, groupId);
    }

    public async Task<SharedGroupDetailDto> RegenerateInviteCodeAsync(Guid userId, Guid groupId)
    {
        await EnsureMemberAsync(userId, groupId);
        var group = await db.SharedGroups.FirstAsync(g => g.Id == groupId);
        group.InviteCode = await NewInviteCodeAsync();
        await db.SaveChangesAsync();
        return await BuildDetailAsync(userId, groupId);
    }

    // A member can only leave once their balance is settled, so nobody is left owing
    // money to someone who can no longer see the group. The last member leaving deletes it.
    public async Task LeaveGroupAsync(Guid userId, Guid groupId)
    {
        var membership = await EnsureMemberAsync(userId, groupId);
        var memberCount = await db.SharedGroupMembers.CountAsync(m => m.GroupId == groupId);

        if (memberCount == 1)
        {
            // Cascades to members, categories, expenses, splits and settlements.
            await db.SharedGroups.Where(g => g.Id == groupId).ExecuteDeleteAsync();
            return;
        }

        var balances = await ComputeBalancesAsync(groupId);
        if (balances.GetValueOrDefault(userId).Balance != 0m)
            throw new ValidationException("Saldá tu deuda antes de salir del grupo.");

        db.SharedGroupMembers.Remove(membership);
        await db.SaveChangesAsync();
    }

    // ---------- Expenses ----------

    public async Task<SharedPeriodDto> GetPeriodAsync(Guid userId, Guid groupId, int year, int month)
    {
        await EnsureMemberAsync(userId, groupId);
        if (year < 2000 || year > 2100 || month < 1 || month > 12)
            throw new ValidationException("Período inválido.");

        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1);

        var expenses = await db.SharedExpenses
            .Where(e => e.GroupId == groupId && e.Date >= from && e.Date < to)
            .Include(e => e.Category)
            .Include(e => e.PaidBy)
            .Include(e => e.Splits).ThenInclude(s => s.User)
            .OrderByDescending(e => e.Date).ThenBy(e => e.Description)
            .AsSplitQuery()
            .ToListAsync();

        var settlements = await db.SharedSettlements
            .Where(s => s.GroupId == groupId && s.Date >= from && s.Date < to)
            .Include(s => s.FromUser)
            .Include(s => s.ToUser)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        var shares = expenses
            .SelectMany(e => e.Splits)
            .GroupBy(s => s.UserId)
            .Select(g => new SharedAmountDto { Id = g.Key, Name = g.First().User.Name, Amount = g.Sum(s => s.Amount) })
            .OrderByDescending(s => s.Amount)
            .ToList();

        var byCategory = expenses
            .GroupBy(e => e.CategoryId)
            .Select(g => new SharedAmountDto
            {
                Id = g.Key,
                Name = g.First().Category?.Name ?? "Sin categoría",
                Amount = g.Sum(e => e.Total)
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        return new SharedPeriodDto
        {
            Year = year,
            Month = month,
            Expenses = expenses.Select(ToDto).ToList(),
            Settlements = settlements.Select(ToDto).ToList(),
            Total = expenses.Sum(e => e.Total),
            Shares = shares,
            ByCategory = byCategory
        };
    }

    // Expense total per month for the `months` months ending at year/month.
    public async Task<List<MonthlyAmountDto>> GetTrendAsync(Guid userId, Guid groupId, int year, int month, int months)
    {
        await EnsureMemberAsync(userId, groupId);
        if (year < 2000 || year > 2100 || month < 1 || month > 12 || months < 1 || months > 36)
            throw new ValidationException("Período inválido.");

        var to = new DateOnly(year, month, 1).AddMonths(1);
        var from = to.AddMonths(-months);

        var totals = await db.SharedExpenses
            .Where(e => e.GroupId == groupId && e.Date >= from && e.Date < to)
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(e => e.Total) })
            .ToListAsync();

        return Enumerable.Range(0, months)
            .Select(i => from.AddMonths(i))
            .Select(d => new MonthlyAmountDto
            {
                Year = d.Year,
                Month = d.Month,
                Amount = totals.FirstOrDefault(t => t.Year == d.Year && t.Month == d.Month)?.Total ?? 0
            })
            .ToList();
    }

    public async Task<SharedExpenseDto> CreateExpenseAsync(Guid userId, Guid groupId, SharedExpenseRequestDto request)
    {
        await EnsureMemberAsync(userId, groupId);
        var (description, splits) = await ValidateExpenseAsync(groupId, request);

        var expense = new SharedExpense
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Date = request.Date,
            Description = description,
            Total = request.Total,
            CategoryId = request.CategoryId,
            PaidByUserId = request.PaidByUserId,
            CreatedByUserId = userId
        };
        foreach (var split in splits)
            expense.Splits.Add(new SharedExpenseSplit { Id = Guid.NewGuid(), ExpenseId = expense.Id, UserId = split.UserId, Amount = split.Amount });

        db.SharedExpenses.Add(expense);
        await db.SaveChangesAsync();
        return await LoadExpenseDtoAsync(expense.Id);
    }

    public async Task<SharedExpenseDto> UpdateExpenseAsync(Guid userId, Guid groupId, Guid expenseId, SharedExpenseRequestDto request)
    {
        await EnsureMemberAsync(userId, groupId);
        var expense = await db.SharedExpenses
            .Include(e => e.Splits)
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.GroupId == groupId)
            ?? throw new NotFoundException("Gasto no encontrado.");

        // People who already were on the expense stay valid even if they left the group since.
        var previousPeople = expense.Splits.Select(sp => sp.UserId).Append(expense.PaidByUserId);
        var (description, splits) = await ValidateExpenseAsync(groupId, request, previousPeople);

        expense.Date = request.Date;
        expense.Description = description;
        expense.Total = request.Total;
        expense.CategoryId = request.CategoryId;
        expense.PaidByUserId = request.PaidByUserId;

        db.SharedExpenseSplits.RemoveRange(expense.Splits);
        foreach (var split in splits)
            db.SharedExpenseSplits.Add(new SharedExpenseSplit { Id = Guid.NewGuid(), ExpenseId = expense.Id, UserId = split.UserId, Amount = split.Amount });

        await db.SaveChangesAsync();
        return await LoadExpenseDtoAsync(expense.Id);
    }

    public async Task DeleteExpenseAsync(Guid userId, Guid groupId, Guid expenseId)
    {
        await EnsureMemberAsync(userId, groupId);
        var deleted = await db.SharedExpenses
            .Where(e => e.Id == expenseId && e.GroupId == groupId)
            .ExecuteDeleteAsync();
        if (deleted == 0)
            throw new NotFoundException("Gasto no encontrado.");
    }

    // ---------- Categories ----------

    public async Task<SharedCategoryDto> CreateCategoryAsync(Guid userId, Guid groupId, SharedNameRequestDto request)
    {
        await EnsureMemberAsync(userId, groupId);
        var name = ValidateName(request.Name, 100, "El nombre de la categoría");
        var exists = await db.SharedCategories
            .AnyAsync(c => c.GroupId == groupId && c.Name.ToLower() == name.ToLower());
        if (exists)
            throw new ConflictException("Ya existe una categoría con ese nombre.");

        var category = new SharedCategory { Id = Guid.NewGuid(), GroupId = groupId, Name = name };
        db.SharedCategories.Add(category);
        await db.SaveChangesAsync();
        return new SharedCategoryDto { Id = category.Id, Name = category.Name };
    }

    // Expenses in the category are kept and become uncategorized.
    public async Task DeleteCategoryAsync(Guid userId, Guid groupId, Guid categoryId)
    {
        await EnsureMemberAsync(userId, groupId);
        var category = await db.SharedCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.GroupId == groupId)
            ?? throw new NotFoundException("Categoría no encontrada.");
        db.SharedCategories.Remove(category);
        await db.SaveChangesAsync();
    }

    // ---------- Settlements ----------

    public async Task<SharedSettlementDto> CreateSettlementAsync(Guid userId, Guid groupId, SharedSettlementRequestDto request)
    {
        await EnsureMemberAsync(userId, groupId);
        if (request.Amount <= 0m)
            throw new ValidationException("El monto debe ser mayor a cero.");
        if (decimal.Round(request.Amount, 2) != request.Amount)
            throw new ValidationException("El monto no puede tener más de 2 decimales.");
        if (request.FromUserId == request.ToUserId)
            throw new ValidationException("Quien paga y quien recibe deben ser personas distintas.");

        var memberIds = await MemberIdsAsync(groupId);
        if (!memberIds.Contains(request.FromUserId) || !memberIds.Contains(request.ToUserId))
            throw new ValidationException("Ambas personas deben ser miembros del grupo.");

        var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        if (note?.Length > 200)
            throw new ValidationException("La nota no puede superar los 200 caracteres.");

        var settlement = new SharedSettlement
        {
            Id = Guid.NewGuid(),
            GroupId = groupId,
            Date = request.Date,
            FromUserId = request.FromUserId,
            ToUserId = request.ToUserId,
            Amount = request.Amount,
            Note = note
        };
        db.SharedSettlements.Add(settlement);
        await db.SaveChangesAsync();

        var loaded = await db.SharedSettlements
            .Include(s => s.FromUser)
            .Include(s => s.ToUser)
            .FirstAsync(s => s.Id == settlement.Id);
        return ToDto(loaded);
    }

    public async Task DeleteSettlementAsync(Guid userId, Guid groupId, Guid settlementId)
    {
        await EnsureMemberAsync(userId, groupId);
        var deleted = await db.SharedSettlements
            .Where(s => s.Id == settlementId && s.GroupId == groupId)
            .ExecuteDeleteAsync();
        if (deleted == 0)
            throw new NotFoundException("Pago no encontrado.");
    }

    // ---------- Helpers ----------

    // Non-members get NotFound rather than Forbidden so group ids can't be probed.
    private async Task<SharedGroupMember> EnsureMemberAsync(Guid userId, Guid groupId) =>
        await db.SharedGroupMembers.FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId)
        ?? throw new NotFoundException("Grupo no encontrado.");

    private async Task<HashSet<Guid>> MemberIdsAsync(Guid groupId) =>
        (await db.SharedGroupMembers.Where(m => m.GroupId == groupId).Select(m => m.UserId).ToListAsync()).ToHashSet();

    private static string ValidateName(string? value, int maxLength, string what)
    {
        var name = value?.Trim() ?? "";
        if (name.Length == 0)
            throw new ValidationException($"{what} es obligatorio.");
        if (name.Length > maxLength)
            throw new ValidationException($"{what} no puede superar los {maxLength} caracteres.");
        return name;
    }

    private async Task<(string Description, List<SharedSplitRequestDto> Splits)> ValidateExpenseAsync(
        Guid groupId, SharedExpenseRequestDto request, IEnumerable<Guid>? alsoAllowed = null)
    {
        var description = ValidateName(request.Description, 200, "La descripción");
        if (request.Total <= 0m)
            throw new ValidationException("El total debe ser mayor a cero.");
        if (decimal.Round(request.Total, 2) != request.Total)
            throw new ValidationException("El total no puede tener más de 2 decimales.");

        var memberIds = await MemberIdsAsync(groupId);
        memberIds.UnionWith(alsoAllowed ?? []);
        if (!memberIds.Contains(request.PaidByUserId))
            throw new ValidationException("Quien pagó debe ser miembro del grupo.");

        if (request.CategoryId is { } categoryId &&
            !await db.SharedCategories.AnyAsync(c => c.Id == categoryId && c.GroupId == groupId))
            throw new ValidationException("Categoría no encontrada.");

        var splits = (request.Splits ?? []).Where(s => s.Amount != 0m).ToList();
        if (splits.Count == 0)
            throw new ValidationException("Al menos un miembro tiene que participar del gasto.");
        if (splits.Select(s => s.UserId).Distinct().Count() != splits.Count)
            throw new ValidationException("Cada miembro puede aparecer una sola vez en la división.");
        foreach (var split in splits)
        {
            if (!memberIds.Contains(split.UserId))
                throw new ValidationException("Cada parte debe corresponder a un miembro del grupo.");
            if (split.Amount < 0m)
                throw new ValidationException("Las partes no pueden ser negativas.");
            if (decimal.Round(split.Amount, 2) != split.Amount)
                throw new ValidationException("Las partes no pueden tener más de 2 decimales.");
        }
        if (splits.Sum(s => s.Amount) != request.Total)
            throw new ValidationException("Las partes deben sumar el total.");

        return (description, splits);
    }

    // Balance per user over the whole history: what they paid minus their share,
    // plus settlements they paid minus settlements they received.
    private async Task<Dictionary<Guid, (decimal Paid, decimal Share, decimal Balance)>> ComputeBalancesAsync(Guid groupId)
    {
        var paid = await db.SharedExpenses
            .Where(e => e.GroupId == groupId)
            .GroupBy(e => e.PaidByUserId)
            .Select(g => new { UserId = g.Key, Amount = g.Sum(e => e.Total) })
            .ToListAsync();
        var shares = await db.SharedExpenseSplits
            .Where(s => s.Expense.GroupId == groupId)
            .GroupBy(s => s.UserId)
            .Select(g => new { UserId = g.Key, Amount = g.Sum(s => s.Amount) })
            .ToListAsync();
        var sent = await db.SharedSettlements
            .Where(s => s.GroupId == groupId)
            .GroupBy(s => s.FromUserId)
            .Select(g => new { UserId = g.Key, Amount = g.Sum(s => s.Amount) })
            .ToListAsync();
        var received = await db.SharedSettlements
            .Where(s => s.GroupId == groupId)
            .GroupBy(s => s.ToUserId)
            .Select(g => new { UserId = g.Key, Amount = g.Sum(s => s.Amount) })
            .ToListAsync();

        var result = new Dictionary<Guid, (decimal Paid, decimal Share, decimal Balance)>();
        void Add(Guid id, decimal paidDelta, decimal shareDelta, decimal balanceDelta)
        {
            var current = result.GetValueOrDefault(id);
            result[id] = (current.Paid + paidDelta, current.Share + shareDelta, current.Balance + balanceDelta);
        }
        foreach (var p in paid) Add(p.UserId, p.Amount, 0m, p.Amount);
        foreach (var s in shares) Add(s.UserId, 0m, s.Amount, -s.Amount);
        foreach (var s in sent) Add(s.UserId, 0m, 0m, s.Amount);
        foreach (var r in received) Add(r.UserId, 0m, 0m, -r.Amount);
        return result;
    }

    // Greedy: the biggest debtor pays the biggest creditor until everyone is at zero.
    // Produces at most n-1 payments, which for a couple is just "A owes B".
    private static List<SharedDebtDto> SimplifyDebts(List<SharedMemberDto> members)
    {
        var debtors = members.Where(m => m.Balance < 0m)
            .Select(m => (Member: m, Amount: -m.Balance)).OrderByDescending(d => d.Amount).ToList();
        var creditors = members.Where(m => m.Balance > 0m)
            .Select(m => (Member: m, Amount: m.Balance)).OrderByDescending(c => c.Amount).ToList();

        var debts = new List<SharedDebtDto>();
        int i = 0, j = 0;
        while (i < debtors.Count && j < creditors.Count)
        {
            var amount = Math.Min(debtors[i].Amount, creditors[j].Amount);
            debts.Add(new SharedDebtDto
            {
                FromUserId = debtors[i].Member.UserId,
                FromName = debtors[i].Member.Name,
                ToUserId = creditors[j].Member.UserId,
                ToName = creditors[j].Member.Name,
                Amount = amount
            });
            debtors[i] = (debtors[i].Member, debtors[i].Amount - amount);
            creditors[j] = (creditors[j].Member, creditors[j].Amount - amount);
            if (debtors[i].Amount == 0m) i++;
            if (creditors[j].Amount == 0m) j++;
        }
        return debts;
    }

    private async Task<SharedGroupDetailDto> BuildDetailAsync(Guid userId, Guid groupId)
    {
        var group = await db.SharedGroups
            .Include(g => g.Members).ThenInclude(m => m.User)
            .Include(g => g.Categories)
            .AsSplitQuery()
            .FirstAsync(g => g.Id == groupId);

        var balances = await ComputeBalancesAsync(groupId);
        var members = group.Members
            .OrderBy(m => m.JoinedAt)
            .Select(m =>
            {
                var b = balances.GetValueOrDefault(m.UserId);
                return new SharedMemberDto
                {
                    UserId = m.UserId,
                    Name = m.User.Name,
                    IsMe = m.UserId == userId,
                    Paid = b.Paid,
                    Share = b.Share,
                    Balance = b.Balance
                };
            })
            .ToList();

        return new SharedGroupDetailDto
        {
            Id = group.Id,
            Name = group.Name,
            InviteCode = group.InviteCode,
            Members = members,
            Categories = group.Categories
                .OrderBy(c => c.Name)
                .Select(c => new SharedCategoryDto { Id = c.Id, Name = c.Name })
                .ToList(),
            Debts = SimplifyDebts(members)
        };
    }

    private async Task<string> NewInviteCodeAsync()
    {
        while (true)
        {
            var code = RandomNumberGenerator.GetString(InviteAlphabet, InviteCodeLength);
            if (!await db.SharedGroups.AnyAsync(g => g.InviteCode == code))
                return code;
        }
    }

    private async Task<SharedExpenseDto> LoadExpenseDtoAsync(Guid expenseId)
    {
        var expense = await db.SharedExpenses
            .Include(e => e.Category)
            .Include(e => e.PaidBy)
            .Include(e => e.Splits).ThenInclude(s => s.User)
            .AsSplitQuery()
            .FirstAsync(e => e.Id == expenseId);
        return ToDto(expense);
    }

    private static SharedExpenseDto ToDto(SharedExpense e) => new()
    {
        Id = e.Id,
        Date = e.Date,
        Description = e.Description,
        Total = e.Total,
        CategoryId = e.CategoryId,
        CategoryName = e.Category?.Name,
        PaidByUserId = e.PaidByUserId,
        PaidByName = e.PaidBy.Name,
        Splits = e.Splits
            .OrderByDescending(s => s.Amount)
            .Select(s => new SharedSplitDto { UserId = s.UserId, Name = s.User.Name, Amount = s.Amount })
            .ToList()
    };

    private static SharedSettlementDto ToDto(SharedSettlement s) => new()
    {
        Id = s.Id,
        Date = s.Date,
        FromUserId = s.FromUserId,
        FromName = s.FromUser.Name,
        ToUserId = s.ToUserId,
        ToName = s.ToUser.Name,
        Amount = s.Amount,
        Note = s.Note
    };
}
