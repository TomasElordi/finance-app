using api.Models.Enums;

namespace api.DTOs;

// Year/Month is the statement being edited; CurrentInstallment is the installment number
// shown on that statement (e.g. 3 for "3/6"). The first month is derived from both.
public class CardItemRequestDto
{
    public required string Name { get; set; }
    public required decimal Amount { get; set; }
    public required CardItemType Type { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? PersonId { get; set; }
    public required int Year { get; set; }
    public required int Month { get; set; }
    public int CurrentInstallment { get; set; } = 1;
    public int Installments { get; set; } = 1;
    public bool IsRecurring { get; set; }
    // For an existing recurring item that started before Year/Month: keep earlier months
    // as they were and apply the changes from Year/Month on.
    public bool FromThisMonth { get; set; }
}
