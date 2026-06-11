using api.Data;
using api.DTOs;
using api.Exceptions;
using api.Models;
using api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class BudgetService(AppDbContext db) : IBudgetService
{
    public async Task<List<BudgetResponseDto>> GetBudgetsAsync(Guid userId, int year, int month)
    {
        return await db.Budgets
            .Where(b => b.UserId == userId && b.Year == year && b.Month == month)
            .Select(b => new BudgetResponseDto
            {
                Id = b.Id,
                AccountId = b.AccountId,
                AccountName = b.Account.Name,
                Year = b.Year,
                Month = b.Month,
                Amount = b.Amount
            })
            .ToListAsync();
    }

    public async Task<List<BudgetResponseDto>> UpsertBudgetsAsync(Guid userId, List<UpsertBudgetRequestDto> items)
    {
        var accountIds = items.Select(i => i.AccountId).Distinct().ToList();

        var validAccounts = await db.Accounts
            .Where(a => a.UserId == userId && accountIds.Contains(a.Id) && a.Nature == NatureType.Expense)
            .Select(a => a.Id)
            .ToListAsync();

        if (validAccounts.Count != accountIds.Count)
            throw new ValidationException("One or more accounts are invalid or not of Expense type.");

        var results = new List<BudgetResponseDto>();

        foreach (var item in items)
        {
            var existing = await db.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId && b.AccountId == item.AccountId && b.Year == item.Year && b.Month == item.Month);

            if (existing != null)
            {
                existing.Amount = item.Amount;
            }
            else
            {
                existing = new Budget
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AccountId = item.AccountId,
                    Year = item.Year,
                    Month = item.Month,
                    Amount = item.Amount
                };
                db.Budgets.Add(existing);
            }

            await db.SaveChangesAsync();

            var accountName = await db.Accounts
                .Where(a => a.Id == item.AccountId)
                .Select(a => a.Name)
                .FirstAsync();

            results.Add(new BudgetResponseDto
            {
                Id = existing.Id,
                AccountId = existing.AccountId,
                AccountName = accountName,
                Year = existing.Year,
                Month = existing.Month,
                Amount = existing.Amount
            });
        }

        return results;
    }

    public async Task<bool> DeleteBudgetAsync(Guid userId, Guid budgetId)
    {
        var budget = await db.Budgets.FirstOrDefaultAsync(b => b.Id == budgetId);

        if (budget == null)
            return false;

        if (budget.UserId != userId)
            throw new ForbiddenException("Unauthorized.");

        db.Budgets.Remove(budget);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<BudgetSummaryItemDto>> GetPeriodSummaryAsync(Guid userId, int year, int month)
    {
        var periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);

        var budgets = await db.Budgets
            .Where(b => b.UserId == userId && b.Year == year && b.Month == month)
            .Include(b => b.Account)
            .ToListAsync();

        var budgetAccountIds = budgets.Select(b => b.AccountId).ToList();

        // Sum of Debit entry_lines for Expense accounts in the period (Debit increases Expense accounts)
        var actuals = await db.EntryLines
            .Where(el =>
                budgetAccountIds.Contains(el.AccountId) &&
                el.Type == EntryLineType.Debit &&
                el.Entry.UserId == userId &&
                el.Entry.Date >= periodStart &&
                el.Entry.Date < periodEnd)
            .GroupBy(el => el.AccountId)
            .Select(g => new { AccountId = g.Key, Total = g.Sum(el => el.Amount) })
            .ToDictionaryAsync(x => x.AccountId, x => x.Total);

        return budgets.Select(b =>
        {
            var actual = actuals.GetValueOrDefault(b.AccountId, 0m);
            var percentage = b.Amount > 0 ? Math.Round(actual / b.Amount * 100, 2) : 0m;
            return new BudgetSummaryItemDto
            {
                AccountId = b.AccountId,
                AccountName = b.Account.Name,
                BudgetedAmount = b.Amount,
                ActualAmount = actual,
                Percentage = percentage
            };
        }).ToList();
    }
}
