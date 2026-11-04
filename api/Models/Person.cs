namespace api.Models;
using System.ComponentModel.DataAnnotations;

// Someone a card item can be charged to. Items without a person belong to the user.
public class Person
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required string Name { get; set; }
}
