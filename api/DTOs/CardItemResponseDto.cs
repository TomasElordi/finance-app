using api.Models.Enums;

namespace api.DTOs;

public class CardItemResponseDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required decimal Amount { get; set; }
    public required CardItemType Type { get; set; }
    public Guid? AccountId { get; set; }
    public string? AccountName { get; set; }
    public Guid? PersonId { get; set; }
    public required string PersonName { get; set; }
    // Installment number in the requested month (1-based).
    public required int InstallmentNumber { get; set; }
    public required int Installments { get; set; }
}
