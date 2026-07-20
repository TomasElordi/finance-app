namespace api.DTOs;

public class BudgetOverviewDto
{
    public required decimal TotalBudgeted { get; set; }
    public required decimal TotalActual { get; set; }
    public required decimal TotalIncome { get; set; }
}
