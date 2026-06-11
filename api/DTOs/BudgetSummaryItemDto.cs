namespace api.DTOs;

public class BudgetSummaryItemDto
{
    public required Guid AccountId { get; set; }
    public required string AccountName { get; set; }
    public required decimal BudgetedAmount { get; set; }
    public required decimal ActualAmount { get; set; }
    public required decimal Percentage { get; set; }
}
