namespace api.DTOs;

public class BalanceSheetDto
{
    public required int Year { get; set; }
    public required List<ReportLineDto> Assets { get; set; }
    public required List<ReportLineDto> Liabilities { get; set; }
    public required List<ReportLineDto> Equity { get; set; }
    public required decimal TotalAssets { get; set; }
    public required decimal TotalLiabilities { get; set; }
    public required decimal TotalEquity { get; set; }
}
