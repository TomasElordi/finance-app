namespace api.Models;
using System.ComponentModel.DataAnnotations;

public class User
{
    [Key]
    public required Guid Id { get; set; } 

    public required string Name { get; set; }

    public required string Password { get; set; }
    
    public required string Email { get; set;}
}