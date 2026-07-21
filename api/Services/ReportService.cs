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
}
