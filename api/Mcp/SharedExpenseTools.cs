using System.ComponentModel;
using System.Security.Claims;
using api.DTOs;
using api.Exceptions;
using api.Services;
using ModelContextProtocol.Server;
using static api.Mcp.McpErrors;

namespace api.Mcp;

// Shared expenses (gastos en común) between members of a group, e.g. a couple.
// Everything goes through SharedGroupService, which checks the caller is a member.
[McpServerToolType]
public class SharedExpenseTools(ISharedGroupService service, IHttpContextAccessor httpContextAccessor)
{
    private Guid CurrentUserId =>
        Guid.Parse(httpContextAccessor.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [McpServerTool(Name = "list_shared_groups", ReadOnly = true)]
    [Description("Lists the shared-expense groups (gastos en común) the current user belongs to, with the member names and the user's balance in each (positive: the others owe the user; negative: the user owes). Start here to get the groupId the other shared-expense tools need.")]
    public Task<List<SharedGroupSummaryDto>> ListSharedGroups()
        => service.GetGroupsAsync(CurrentUserId);

    [McpServerTool(Name = "get_shared_group", ReadOnly = true)]
    [Description("Gets a shared-expense group: its members (userId, name, isMe, total paid, total share, balance), its categories (id, name) and the simplified debts (who owes whom and how much). Use it to resolve member and category names to ids before creating expenses or settlements.")]
    public Task<object> GetSharedGroup([Description("Group id from list_shared_groups")] Guid groupId)
        => Run(async () => (object)await service.GetGroupAsync(CurrentUserId, groupId));

    [McpServerTool(Name = "list_shared_expenses", ReadOnly = true)]
    [Description("Lists the expenses and settlements of a shared group for one month, with the month total, each member's share and the total per category.")]
    public Task<object> ListSharedExpenses(
        [Description("Group id from list_shared_groups")] Guid groupId,
        [Description("Year, e.g. 2026. Defaults to the current year.")] int? year = null,
        [Description("Month 1-12. Defaults to the current month.")] int? month = null)
    {
        var today = DateTime.UtcNow;
        return Run(async () => (object)await service.GetPeriodAsync(CurrentUserId, groupId, year ?? today.Year, month ?? today.Month));
    }

    [McpServerTool(Name = "create_shared_expense")]
    [Description("""
        Records a shared expense in a group: who paid it and how it's split between members.
        Split modes:
        - "equal" (default): split evenly between the members in `shares` (value ignored), or between all members if `shares` is empty.
        - "percent": each entry in `shares` has the member's percentage; they must add up to 100 (e.g. 70 and 30).
        - "amount": each entry in `shares` has the member's exact amount; they must add up to the total.
        Members left out of `shares` don't take part in the expense. Amounts are rounded to cents so the parts always add up to the total.
        """)]
    public Task<object> CreateSharedExpense(
        [Description("Group id from list_shared_groups")] Guid groupId,
        [Description("What the expense was, e.g. \"Supermercado\"")] string description,
        [Description("Total amount paid, positive, up to 2 decimals")] decimal total,
        [Description("userId of the member who paid (from get_shared_group)")] Guid paidByUserId,
        [Description("\"equal\", \"percent\" or \"amount\"")] string splitMode = "equal",
        [Description("Members taking part and their value (percentage or amount depending on splitMode). Optional for \"equal\".")] List<ShareInput>? shares = null,
        [Description("Category id from get_shared_group. Optional.")] Guid? categoryId = null,
        [Description("Date of the expense, e.g. 2026-11-05. Defaults to today.")] DateOnly? date = null)
        => Run(async () =>
        {
            var group = await service.GetGroupAsync(CurrentUserId, groupId);
            var splits = BuildSplits(total, splitMode, shares, group.Members.Select(m => m.UserId).ToList());
            var expense = await service.CreateExpenseAsync(CurrentUserId, groupId, new SharedExpenseRequestDto
            {
                Date = date ?? DateOnly.FromDateTime(DateTime.UtcNow),
                Description = description,
                Total = total,
                CategoryId = categoryId,
                PaidByUserId = paidByUserId,
                Splits = splits
            });
            return (object)expense;
        });

    [McpServerTool(Name = "update_shared_expense", Idempotent = true)]
    [Description("""
        Edits a shared expense for every member of the group. Every field replaces the current value, so first read the expense with list_shared_expenses and send back the fields that don't change.
        The split works like in create_shared_expense: with splitMode "equal" and no `shares`, it's split between ALL current members, so pass `shares` to keep it between specific members.
        """)]
    public Task<object> UpdateSharedExpense(
        [Description("Group id")] Guid groupId,
        [Description("Expense id from list_shared_expenses")] Guid expenseId,
        [Description("What the expense was")] string description,
        [Description("Total amount paid, positive, up to 2 decimals")] decimal total,
        [Description("userId of the member who paid")] Guid paidByUserId,
        [Description("Date of the expense, e.g. 2026-11-05")] DateOnly date,
        [Description("\"equal\", \"percent\" or \"amount\"")] string splitMode = "equal",
        [Description("Members taking part and their value (percentage or amount depending on splitMode). Optional for \"equal\".")] List<ShareInput>? shares = null,
        [Description("Category id from get_shared_group. Optional.")] Guid? categoryId = null)
        => Run(async () =>
        {
            var group = await service.GetGroupAsync(CurrentUserId, groupId);
            var splits = BuildSplits(total, splitMode, shares, group.Members.Select(m => m.UserId).ToList());
            return (object)await service.UpdateExpenseAsync(CurrentUserId, groupId, expenseId, new SharedExpenseRequestDto
            {
                Date = date,
                Description = description,
                Total = total,
                CategoryId = categoryId,
                PaidByUserId = paidByUserId,
                Splits = splits
            });
        });

    [McpServerTool(Name = "delete_shared_expense", Destructive = true)]
    [Description("Deletes a shared expense for every member of the group. Use list_shared_expenses to find its id. Confirm with the user before deleting.")]
    public Task<object> DeleteSharedExpense(
        [Description("Group id")] Guid groupId,
        [Description("Expense id from list_shared_expenses")] Guid expenseId)
        => Run(async () =>
        {
            await service.DeleteExpenseAsync(CurrentUserId, groupId, expenseId);
            return (object)"Expense deleted.";
        });

    [McpServerTool(Name = "create_settlement")]
    [Description("Records a payment between two members to settle debts (e.g. a bank transfer): fromUserId paid `amount` to toUserId. Use the debts from get_shared_group to know who owes whom.")]
    public Task<object> CreateSettlement(
        [Description("Group id")] Guid groupId,
        [Description("userId of the member who paid")] Guid fromUserId,
        [Description("userId of the member who received the money")] Guid toUserId,
        [Description("Amount paid, positive, up to 2 decimals")] decimal amount,
        [Description("Date of the payment, e.g. 2026-11-05. Defaults to today.")] DateOnly? date = null,
        [Description("Optional note, e.g. \"Transferencia\"")] string? note = null)
        => Run(async () => (object)await service.CreateSettlementAsync(CurrentUserId, groupId, new SharedSettlementRequestDto
        {
            Date = date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Amount = amount,
            Note = note
        }));

    [McpServerTool(Name = "delete_settlement", Destructive = true)]
    [Description("Deletes a payment between members (settlement), e.g. one recorded by mistake. Use list_shared_expenses to find its id. Confirm with the user before deleting.")]
    public Task<object> DeleteSettlement(
        [Description("Group id")] Guid groupId,
        [Description("Settlement id from list_shared_expenses")] Guid settlementId)
        => Run(async () =>
        {
            await service.DeleteSettlementAsync(CurrentUserId, groupId, settlementId);
            return (object)"Settlement deleted.";
        });

    // Turns the split mode into exact amounts in cents; leftover cents go to the
    // largest shares first, matching the web app.
    private static List<SharedSplitRequestDto> BuildSplits(decimal total, string splitMode, List<ShareInput>? shares, List<Guid> memberIds)
    {
        shares ??= [];
        var totalCents = (long)decimal.Round(total * 100m);
        List<(Guid UserId, decimal Weight)> weights = splitMode.Trim().ToLowerInvariant() switch
        {
            "equal" => (shares.Count == 0 ? memberIds : shares.Select(s => s.UserId).Distinct().ToList())
                .Select(id => (id, 1m)).ToList(),
            "percent" => ValidatePercents(shares),
            "amount" => shares.Select(s => (s.UserId, s.Value)).ToList(),
            _ => throw new ValidationException("splitMode must be \"equal\", \"percent\" or \"amount\".")
        };

        if (splitMode.Trim().Equals("amount", StringComparison.OrdinalIgnoreCase))
            return weights.Select(w => new SharedSplitRequestDto { UserId = w.UserId, Amount = w.Weight }).ToList();

        var weightSum = weights.Sum(w => w.Weight);
        if (weights.Count == 0 || weightSum <= 0m)
            throw new ValidationException("At least one member must take part in the expense.");

        var raw = weights.Select(w => totalCents * w.Weight / weightSum).ToList();
        var parts = raw.Select(r => (long)Math.Floor(r)).ToList();
        var leftover = totalCents - parts.Sum();
        foreach (var i in Enumerable.Range(0, parts.Count)
                     .Where(i => weights[i].Weight > 0m)
                     .OrderByDescending(i => raw[i] - Math.Floor(raw[i]))
                     .ThenByDescending(i => weights[i].Weight))
        {
            if (leftover-- <= 0) break;
            parts[i]++;
        }

        return weights
            .Select((w, i) => new SharedSplitRequestDto { UserId = w.UserId, Amount = parts[i] / 100m })
            .ToList();
    }

    private static List<(Guid, decimal)> ValidatePercents(List<ShareInput> shares)
    {
        if (shares.Any(s => s.Value < 0m))
            throw new ValidationException("Percentages can't be negative.");
        var sum = shares.Sum(s => s.Value);
        if (sum != 100m)
            throw new ValidationException($"Percentages add up to {sum}, they must add up to 100.");
        return shares.Select(s => (s.UserId, s.Value)).ToList();
    }
}

public class ShareInput
{
    [Description("userId of the member")]
    public required Guid UserId { get; set; }
    [Description("Percentage (percent mode) or amount (amount mode). Ignored in equal mode.")]
    public decimal Value { get; set; }
}
