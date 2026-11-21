namespace api.Models;
using System.ComponentModel.DataAnnotations;

// An access token (by its jti) that was revoked before it expired, e.g. on logout.
// Access tokens are stateless JWTs, so every request checks this list. Rows are only
// needed until the token would have expired anyway.
public class RevokedToken
{
    [Key]
    public required Guid Id { get; set; }
    public required string Jti { get; set; }
    public required DateTime ExpiresAt { get; set; }
}
