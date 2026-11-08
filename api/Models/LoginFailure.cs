namespace api.Models;
using System.ComponentModel.DataAnnotations;

// A failed login attempt, kept for a short window to throttle password guessing.
// Stored in the database because Lambda instances don't share memory.
public class LoginFailure
{
    [Key]
    public required Guid Id { get; set; }
    // Trimmed and lowercased, so changing the case doesn't get around the limit.
    public required string Email { get; set; }
    public required DateTime CreatedAt { get; set; }
}
