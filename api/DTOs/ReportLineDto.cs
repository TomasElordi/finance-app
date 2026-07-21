namespace api.DTOs;

public class ReportLineDto
{
    public required Guid AccountId { get; set; }
    public required string AccountName { get; set; }
    public required decimal Amount { get; set; }
}
