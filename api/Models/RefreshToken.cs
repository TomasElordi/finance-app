namespace api.Models;
using System.ComponentModel.DataAnnotations;

public class RefreshToken
{
    [Key]
    public required Guid Id { get; set; } 
    public required string Token { get; set; }
    public required DateTime ExpirationDate { get; set; }
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
}