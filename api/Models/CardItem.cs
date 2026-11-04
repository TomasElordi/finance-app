namespace api.Models;
using System.ComponentModel.DataAnnotations;
using api.Models.Enums;

// A line of the credit card statement. A purchase in installments is stored once and
// shows up in every month from (StartYear, StartMonth) for Installments months.
// A recurring item shows up every month from its start until (EndYear, EndMonth),
// or indefinitely while the end is null.
public class CardItem
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required string Name { get; set; }
    // Amount of a single installment (what the statement shows each month). Always positive.
    public required decimal Amount { get; set; }
    public required CardItemType Type { get; set; }
    public Guid? AccountId { get; set; }
    public Account? Account { get; set; }
    public Guid? PersonId { get; set; }
    public Person? Person { get; set; }
    public required int StartYear { get; set; }
    public required int StartMonth { get; set; }
    public required int Installments { get; set; }
    public bool IsRecurring { get; set; }
    // Last month a recurring item is charged (inclusive). Null while it's still active.
    public int? EndYear { get; set; }
    public int? EndMonth { get; set; }
}
