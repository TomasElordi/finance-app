namespace api.Models;
using System.ComponentModel.DataAnnotations;

public class RefreshToken
{
    [Key]
    public required Guid Id { get; set; } 
    // SHA-256 hash (hex) of the refresh token handed to the client, never the token itself.
    public required string Token { get; set; }
    public required DateTime ExpirationDate { get; set; }
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
}