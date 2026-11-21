namespace api.Models;
using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public required Guid Id { get; set; } 

    public required string Name { get; set; }

    public required string Password { get; set; }
    
    public required string Email { get; set;}

    // "Disconnect MCP clients": access tokens issued to OAuth clients at or before this
    // moment are rejected, and their refresh tokens are deleted.
    public DateTime? McpTokensRevokedAt { get; set; }
}