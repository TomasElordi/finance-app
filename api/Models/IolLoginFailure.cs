namespace api.Models;
using System.ComponentModel.DataAnnotations;

// A login IOL rejected when fetching quotes. Too many in a row and IOL may lock the user's
// broker account, so InvestmentQuoteService stops trying for a while. Only who and when:
// nothing about the credentials. Stored in the database because Lambda instances don't share
// memory.
public class IolLoginFailure
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required DateTime CreatedAt { get; set; }
}
