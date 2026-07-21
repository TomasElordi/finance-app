namespace api.DTOs;

public class IncomeStatementDto
{
    public required int Year { get; set; }
    public required int Month { get; set; }
    public required List<ReportLineDto> Income { get; set; }
    public required List<ReportLineDto> Expenses { get; set; }
    public required decimal TotalIncome { get; set; }
    public required decimal TotalExpenses { get; set; }
    public required decimal NetResult { get; set; }
}
