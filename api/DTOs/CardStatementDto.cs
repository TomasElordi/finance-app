namespace api.DTOs;

public class CardStatementDto
{
    public required int Year { get; set; }
    public required int Month { get; set; }
    public required List<CardItemResponseDto> Items { get; set; }
    public required List<CardPersonTotalDto> People { get; set; }
    public required decimal Total { get; set; }
}

public class CardPersonTotalDto
{
    // Null for the user's own items.
    public Guid? PersonId { get; set; }
    public required string PersonName { get; set; }
    public required decimal Expenses { get; set; }
    public required decimal Taxes { get; set; }
    public required decimal Credits { get; set; }
    // Expenses + Taxes - Credits
    public required decimal Total { get; set; }
}
