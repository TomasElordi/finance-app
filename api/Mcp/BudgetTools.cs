using System.ComponentModel;
using System.Security.Claims;
using api.DTOs;
using api.Exceptions;
using api.Services;
using ModelContextProtocol.Server;
using static api.Mcp.McpErrors;

namespace api.Mcp;

// Monthly budget (presupuesto) per expense account.
[McpServerToolType]
public class BudgetTools(IBudgetService service, IHttpContextAccessor httpContextAccessor)
{
    private Guid CurrentUserId =>
        Guid.Parse(httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [McpServerTool(Name = "get_budget", ReadOnly = true)]
    [Description("""
        Budget status for one month: for each budgeted expense account, the budgeted amount, the actual amount spent and the percentage used; plus the month totals (total budgeted, total actual spending and total income).
        Use it to answer how the user is doing against their budget.
        """)]
    public Task<object> GetBudget(
        [Description("Year, e.g. 2026. Defaults to the current year.")] int? year = null,
        [Description("Month 1-12. Defaults to the current month.")] int? month = null)
    {
        var (y, m) = Period(year, month);
        return Run(async () =>
        {
            ValidatePeriod(y, m);
            var userId = CurrentUserId;
            return (object)new
            {
                Year = y,
                Month = m,
                Overview = await service.GetPeriodOverviewAsync(userId, y, m),
                Accounts = await service.GetPeriodSummaryAsync(userId, y, m)
            };
        });
    }

    [McpServerTool(Name = "list_budgets", ReadOnly = true)]
    [Description("Lists the budget rows of a month (budget id, account id and name, amount). Use it to get the budgetId delete_budget needs.")]
    public Task<object> ListBudgets(
        [Description("Year, e.g. 2026. Defaults to the current year.")] int? year = null,
        [Description("Month 1-12. Defaults to the current month.")] int? month = null)
    {
        var (y, m) = Period(year, month);
        return Run(async () =>
        {
            ValidatePeriod(y, m);
            return (object)await service.GetBudgetsAsync(CurrentUserId, y, m);
        });
    }

    [McpServerTool(Name = "set_budgets", Idempotent = true)]
    [Description("Sets the budgeted amount of one or more expense accounts for a month. Existing amounts for those accounts are replaced; other accounts are left as they are. Accounts must be Expense accounts from list_accounts.")]
    public Task<object> SetBudgets(
        [Description("Year, e.g. 2026")] int year,
        [Description("Month 1-12")] int month,
        [Description("Accounts and their budgeted amount")] List<BudgetInput> items)
        => Run(async () =>
        {
            ValidatePeriod(year, month);
            if (items.Count == 0)
                throw new ValidationException("At least one item is required.");
            if (items.Any(i => i.Amount < 0m))
                throw new ValidationException("Amounts can't be negative.");
            if (items.Select(i => i.AccountId).Distinct().Count() != items.Count)
                throw new ValidationException("Each account can appear only once.");

            return (object)await service.UpsertBudgetsAsync(CurrentUserId, items
                .Select(i => new UpsertBudgetRequestDto { AccountId = i.AccountId, Year = year, Month = month, Amount = i.Amount })
                .ToList());
        });

    [McpServerTool(Name = "replicate_previous_budget")]
    [Description("Copies the previous month's budget into year/month. Fails if that month already has a budget or the previous month has none.")]
    public Task<object> ReplicatePreviousBudget(
        [Description("Target year, e.g. 2026")] int year,
        [Description("Target month 1-12")] int month)
        => Run(async () =>
        {
            ValidatePeriod(year, month);
            return (object)await service.ReplicatePreviousMonthAsync(CurrentUserId, year, month);
        });

    [McpServerTool(Name = "delete_budget", Destructive = true)]
    [Description("Deletes one budget row (one account in one month). Get the id from list_budgets. Confirm with the user before deleting.")]
    public Task<object> DeleteBudget([Description("Budget id from list_budgets")] Guid budgetId)
        => Run(async () => await service.DeleteBudgetAsync(CurrentUserId, budgetId)
            ? (object)"Budget deleted."
            : "Not found: budget not found.");

    private static (int, int) Period(int? year, int? month)
    {
        var today = DateTime.UtcNow;
        return (year ?? today.Year, month ?? today.Month);
    }
}

public class BudgetInput
{
    [Description("Expense account id from list_accounts")]
    public required Guid AccountId { get; set; }
    [Description("Budgeted amount for the month, zero or positive")]
    public required decimal Amount { get; set; }
}
