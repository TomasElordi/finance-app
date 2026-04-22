namespace api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using api.Models.Enums;

public class Account
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required string Name { get; set; }
    public required NatureType Nature { get; set;}
};

