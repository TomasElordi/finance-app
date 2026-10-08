using api.Data;
using api.DTOs;
using api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class ReportService(AppDbContext db) : IReportService
{
    private static readonly HashSet<NatureType> DebitNormalNatures = [NatureType.Asset, NatureType.Expense];
    private const string UnclosedResultLineName = "Resultado del ejercicio (no cerrado)";

    private static decimal NetAmount(NatureType nature, IEnumerable<(EntryLineType Type, decimal Amount)> lines)
    {
        bool isDebitNormal = DebitNormalNatures.Contains(nature);
        return lines.Sum(l => (isDebitNormal == (l.Type == EntryLineType.Debit)) ? l.Amount : -l.Amount);
    }

    public async Task<IncomeStatementDto> GetIncomeStatementAsync(Guid userId, int year, int month)
    {
        var periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);

        var lines = await db.EntryLines
            .Where(el =>
                el.Entry.UserId == userId &&
                !el.Entry.IsClosing &&
                (el.Account.Nature == NatureType.Income || el.Account.Nature == NatureType.Expense) &&
                el.Entry.Date >= periodStart &&
                el.Entry.Date < periodEnd)
            .Select(el => new { el.AccountId, el.Account.Name, el.Account.Nature, el.Type, el.Amount })
            .ToListAsync();

        var income = lines
            .Where(l => l.Nature == NatureType.Income)
            .GroupBy(l => new { l.AccountId, l.Name })
            .Select(g => new ReportLineDto
            {
                AccountId = g.Key.AccountId,
                AccountName = g.Key.Name,
                Amount = NetAmount(NatureType.Income, g.Select(l => (l.Type, l.Amount)))
            })
            .ToList();

        var expenses = lines
            .Where(l => l.Nature == NatureType.Expense)
            .GroupBy(l => new { l.AccountId, l.Name })
            .Select(g => new ReportLineDto
            {
                AccountId = g.Key.AccountId,
                AccountName = g.Key.Name,
                Amount = NetAmount(NatureType.Expense, g.Select(l => (l.Type, l.Amount)))
            })
            .ToList();

        var totalIncome = income.Sum(l => l.Amount);
        var totalExpenses = expenses.Sum(l => l.Amount);

        return new IncomeStatementDto
        {
            Year = year,
            Month = month,
            Income = income,
            Expenses = expenses,
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            NetResult = totalIncome - totalExpenses
        };
    }

    public async Task<BalanceSheetDto> GetBalanceSheetAsync(Guid userId, int year)
    {
        var cutoff = new DateTime(year + 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var lines = await db.EntryLines
            .Where(el => el.Entry.UserId == userId && el.Entry.Date < cutoff)
            .Select(el => new { el.AccountId, el.Account.Name, el.Account.Nature, el.Type, el.Amount })
            .ToListAsync();

        List<ReportLineDto> LinesForNature(NatureType nature) => lines
            .Where(l => l.Nature == nature)
            .GroupBy(l => new { l.AccountId, l.Name })
            .Select(g => new ReportLineDto
            {
                AccountId = g.Key.AccountId,
                AccountName = g.Key.Name,
                Amount = NetAmount(nature, g.Select(l => (l.Type, l.Amount)))
            })
            .ToList();

        var assets = LinesForNature(NatureType.Asset);
        var liabilities = LinesForNature(NatureType.Liability);
        var equity = LinesForNature(NatureType.Equity);

        var netResult = LinesForNature(NatureType.Income).Sum(l => l.Amount)
            - LinesForNature(NatureType.Expense).Sum(l => l.Amount);

        if (netResult != 0)
        {
            equity.Add(new ReportLineDto { AccountId = Guid.Empty, AccountName = UnclosedResultLineName, Amount = netResult });
        }

        return new BalanceSheetDto
        {
            Year = year,
            Assets = assets,
            Liabilities = liabilities,
            Equity = equity,
            TotalAssets = assets.Sum(l => l.Amount),
            TotalLiabilities = liabilities.Sum(l => l.Amount),
            TotalEquity = equity.Sum(l => l.Amount)
        };
    }

    // Net expenses (debits minus credits, excluding closings) for the `months` months ending at year/month
    public async Task<List<MonthlyAmountDto>> GetExpenseTrendAsync(Guid userId, int year, int month, int months, Guid? accountId)
    {
        var periodEnd = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
        var periodStart = periodEnd.AddMonths(-months);

        var totals = await db.EntryLines
            .Where(el =>
                el.Entry.UserId == userId &&
                !el.Entry.IsClosing &&
                el.Account.Nature == NatureType.Expense &&
                (accountId == null || el.AccountId == accountId) &&
                el.Entry.Date >= periodStart &&
                el.Entry.Date < periodEnd)
            .GroupBy(el => new { el.Entry.Date.Year, el.Entry.Date.Month, el.Type })
            .Select(g => new { g.Key.Year, g.Key.Month, g.Key.Type, Total = g.Sum(el => el.Amount) })
            .ToListAsync();

        return Enumerable.Range(0, months)
            .Select(i => periodStart.AddMonths(i))
            .Select(d => new MonthlyAmountDto
            {
                Year = d.Year,
                Month = d.Month,
                Amount = NetAmount(NatureType.Expense, totals
                    .Where(t => t.Year == d.Year && t.Month == d.Month)
                    .Select(t => (t.Type, t.Total)))
            })
            .ToList();
    }

    public async Task<List<AccountMovementDto>> GetAccountMovementsAsync(Guid userId, Guid accountId, int year, int month)
    {
        var periodStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);

        return await db.EntryLines
            .Where(el =>
                el.Entry.UserId == userId &&
                !el.Entry.IsClosing &&
                el.AccountId == accountId &&
                el.Entry.Date >= periodStart &&
                el.Entry.Date < periodEnd)
            .OrderByDescending(el => el.Entry.Date)
            .ThenBy(el => el.Entry.Title)
            .Select(el => new AccountMovementDto
            {
                EntryId = el.EntryId,
                EntryTitle = el.Entry.Title,
                Date = el.Entry.Date,
                Type = el.Type,
                Amount = el.Amount
            })
            .ToListAsync();
    }
}
