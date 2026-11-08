using System.ComponentModel;
using System.Security.Claims;
using api.Exceptions;
using api.Services;
using ModelContextProtocol.Server;
using static api.Mcp.McpErrors;

namespace api.Mcp;

// Read-only financial reports built from the journal entries.
[McpServerToolType]
public class ReportTools(IReportService service, IHttpContextAccessor httpContextAccessor)
{
    private Guid CurrentUserId =>
        Guid.Parse(httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [McpServerTool(Name = "get_income_statement", ReadOnly = true)]
    [Description("Income statement (estado de resultados) for one month: amount per income account, amount per expense account, total income, total expenses and net result. Use it to answer how much was earned or spent in a month and on what.")]
    public Task<object> GetIncomeStatement(
        [Description("Year, e.g. 2026. Defaults to the current year.")] int? year = null,
        [Description("Month 1-12. Defaults to the current month.")] int? month = null)
    {
        var (y, m) = Period(year, month);
        return Run(async () =>
        {
            ValidatePeriod(y, m);
            return (object)await service.GetIncomeStatementAsync(CurrentUserId, y, m);
        });
    }

    [McpServerTool(Name = "get_balance_sheet", ReadOnly = true)]
    [Description("Balance sheet (balance) for a year: balance of each asset, liability and equity account and the totals. Use it to answer how much money the user has in each account and how much they owe.")]
    public Task<object> GetBalanceSheet(
        [Description("Year, e.g. 2026. Defaults to the current year.")] int? year = null)
    {
        var y = year ?? DateTime.UtcNow.Year;
        return Run(async () =>
        {
            ValidatePeriod(y, 1);
            return (object)await service.GetBalanceSheetAsync(CurrentUserId, y);
        });
    }

    [McpServerTool(Name = "get_expense_trend", ReadOnly = true)]
    [Description("Total expenses per month for the `months` months ending at year/month (inclusive), oldest first. Pass accountId (an Expense account from list_accounts) to see the trend of a single category.")]
    public Task<object> GetExpenseTrend(
        [Description("Last year of the range. Defaults to the current year.")] int? year = null,
        [Description("Last month 1-12 of the range. Defaults to the current month.")] int? month = null,
        [Description("Number of months, 1-36")] int months = 12,
        [Description("Optional Expense account id to filter by")] Guid? accountId = null)
    {
        var (y, m) = Period(year, month);
        return Run(async () =>
        {
            ValidatePeriod(y, m);
            if (months is < 1 or > 36)
                throw new ValidationException("months must be between 1 and 36.");
            return (object)await service.GetExpenseTrendAsync(CurrentUserId, y, m, months, accountId);
        });
    }

    [McpServerTool(Name = "get_account_movements", ReadOnly = true)]
    [Description("Movements of one account in a month, most recent first: entry id and title, date, Debit/Credit and amount. Use it to see where the money of an account went or what makes up a category's total.")]
    public Task<object> GetAccountMovements(
        [Description("Account id from list_accounts")] Guid accountId,
        [Description("Year, e.g. 2026. Defaults to the current year.")] int? year = null,
        [Description("Month 1-12. Defaults to the current month.")] int? month = null)
    {
        var (y, m) = Period(year, month);
        return Run(async () =>
        {
            ValidatePeriod(y, m);
            return (object)await service.GetAccountMovementsAsync(CurrentUserId, accountId, y, m);
        });
    }

    private static (int, int) Period(int? year, int? month)
    {
        var today = DateTime.UtcNow;
        return (year ?? today.Year, month ?? today.Month);
    }
}
