namespace api.DTOs;

// Installments still to be paid after a given month.
public class CardUpcomingDto
{
    public required decimal Total { get; set; }
    public required List<CardUpcomingPersonDto> People { get; set; }
    public required List<MonthlyAmountDto> Months { get; set; }
}

public class CardUpcomingPersonDto
{
    public Guid? PersonId { get; set; }
    public required string PersonName { get; set; }
    public required decimal Total { get; set; }
}
