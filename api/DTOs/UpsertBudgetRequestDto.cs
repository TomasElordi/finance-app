namespace api.DTOs;

public class UpsertBudgetRequestDto
{
    public required Guid AccountId { get; set; }
    public required int Year { get; set; }
    public required int Month { get; set; }
    public required decimal Amount { get; set; }
}
