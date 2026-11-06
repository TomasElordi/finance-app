namespace api.Models;
using System.ComponentModel.DataAnnotations;

// A group of users that share expenses (e.g. a couple). Every member sees and edits
// everything in the group. New members join with the invite code.
public class SharedGroup
{
    [Key]
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string InviteCode { get; set; }
    public required DateTime CreatedAt { get; set; }
    public ICollection<SharedGroupMember> Members { get; set; } = [];
    public ICollection<SharedCategory> Categories { get; set; } = [];
}

public class SharedGroupMember
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid GroupId { get; set; }
    public SharedGroup Group { get; set; } = null!;
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required DateTime JoinedAt { get; set; }
}

public class SharedCategory
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid GroupId { get; set; }
    public SharedGroup Group { get; set; } = null!;
    public required string Name { get; set; }
}

// An expense paid by one member and split between members. The splits always add up
// to Total; a member that doesn't take part simply has no split.
public class SharedExpense
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid GroupId { get; set; }
    public SharedGroup Group { get; set; } = null!;
    public required DateOnly Date { get; set; }
    public required string Description { get; set; }
    public required decimal Total { get; set; }
    public Guid? CategoryId { get; set; }
    public SharedCategory? Category { get; set; }
    public required Guid PaidByUserId { get; set; }
    public User PaidBy { get; set; } = null!;
    public required Guid CreatedByUserId { get; set; }
    public ICollection<SharedExpenseSplit> Splits { get; set; } = [];
}

public class SharedExpenseSplit
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid ExpenseId { get; set; }
    public SharedExpense Expense { get; set; } = null!;
    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public required decimal Amount { get; set; }
}

// A payment between members to settle debts: FromUser paid Amount to ToUser.
public class SharedSettlement
{
    [Key]
    public required Guid Id { get; set; }
    public required Guid GroupId { get; set; }
    public SharedGroup Group { get; set; } = null!;
    public required DateOnly Date { get; set; }
    public required Guid FromUserId { get; set; }
    public User FromUser { get; set; } = null!;
    public required Guid ToUserId { get; set; }
    public User ToUser { get; set; } = null!;
    public required decimal Amount { get; set; }
    public string? Note { get; set; }
}
