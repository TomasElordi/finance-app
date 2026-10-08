namespace api.DTOs;

public class MonthlyAmountDto
{
    public required int Year { get; set; }
    public required int Month { get; set; }
    public required decimal Amount { get; set; }
}
