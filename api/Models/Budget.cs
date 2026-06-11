namespace api.Models;
using System.ComponentModel.DataAnnotations;

public class Budget
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public required int Year { get; set; }
    public required int Month { get; set; }
    public required decimal Amount { get; set; }
}
