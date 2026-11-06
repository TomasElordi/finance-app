namespace api.DTOs;

public class SharedGroupSummaryDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required List<string> MemberNames { get; set; }
    // Positive: the group owes the current user. Negative: the current user owes.
    public required decimal MyBalance { get; set; }
}

public class SharedMemberDto
{
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
    public required bool IsMe { get; set; }
    // All-time totals: what the member paid, their share of the expenses, and the
    // resulting balance including settlements (positive = is owed money).
    public required decimal Paid { get; set; }
    public required decimal Share { get; set; }
    public required decimal Balance { get; set; }
}

public class SharedDebtDto
{
    public required Guid FromUserId { get; set; }
    public required string FromName { get; set; }
    public required Guid ToUserId { get; set; }
    public required string ToName { get; set; }
    public required decimal Amount { get; set; }
}

public class SharedCategoryDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}

public class SharedGroupDetailDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string InviteCode { get; set; }
    public required List<SharedMemberDto> Members { get; set; }
    public required List<SharedCategoryDto> Categories { get; set; }
    // Fewest payments that leave everyone at zero.
    public required List<SharedDebtDto> Debts { get; set; }
}

public class SharedSplitDto
{
    public required Guid UserId { get; set; }
    public required string Name { get; set; }
    public required decimal Amount { get; set; }
}

public class SharedExpenseDto
{
    public required Guid Id { get; set; }
    public required DateOnly Date { get; set; }
    public required string Description { get; set; }
    public required decimal Total { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public required Guid PaidByUserId { get; set; }
    public required string PaidByName { get; set; }
    public required List<SharedSplitDto> Splits { get; set; }
}

public class SharedSettlementDto
{
    public required Guid Id { get; set; }
    public required DateOnly Date { get; set; }
    public required Guid FromUserId { get; set; }
    public required string FromName { get; set; }
    public required Guid ToUserId { get; set; }
    public required string ToName { get; set; }
    public required decimal Amount { get; set; }
    public string? Note { get; set; }
}

public class SharedAmountDto
{
    public Guid? Id { get; set; }
    public required string Name { get; set; }
    public required decimal Amount { get; set; }
}

public class SharedPeriodDto
{
    public required int Year { get; set; }
    public required int Month { get; set; }
    public required List<SharedExpenseDto> Expenses { get; set; }
    public required List<SharedSettlementDto> Settlements { get; set; }
    public required decimal Total { get; set; }
    // Each member's share of the month's expenses (Id = user id).
    public required List<SharedAmountDto> Shares { get; set; }
    // Month total per category (Id = category id, null for uncategorized).
    public required List<SharedAmountDto> ByCategory { get; set; }
}

public class SharedNameRequestDto
{
    public required string Name { get; set; }
}

public class JoinSharedGroupRequestDto
{
    public required string Code { get; set; }
}

public class SharedSplitRequestDto
{
    public required Guid UserId { get; set; }
    public required decimal Amount { get; set; }
}

public class SharedExpenseRequestDto
{
    public required DateOnly Date { get; set; }
    public required string Description { get; set; }
    public required decimal Total { get; set; }
    public Guid? CategoryId { get; set; }
    public required Guid PaidByUserId { get; set; }
    public required List<SharedSplitRequestDto> Splits { get; set; }
}

public class SharedSettlementRequestDto
{
    public required DateOnly Date { get; set; }
    public required Guid FromUserId { get; set; }
    public required Guid ToUserId { get; set; }
    public required decimal Amount { get; set; }
    public string? Note { get; set; }
}
