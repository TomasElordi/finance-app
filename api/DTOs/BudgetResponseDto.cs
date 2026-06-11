namespace api.DTOs;

public class BudgetResponseDto
{
    public required Guid Id { get; set; }
    public required Guid AccountId { get; set; }
    public required string AccountName { get; set; }
    public required int Year { get; set; }
    public required int Month { get; set; }
    public required decimal Amount { get; set; }
}
