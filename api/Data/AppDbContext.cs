using api.Models;
using Microsoft.EntityFrameworkCore;
namespace api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Entry> Entries { get; set; }
    public DbSet<EntryLine> EntryLines { get; set; }
    public DbSet<RefreshToken> RefreshToken { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<Person> People { get; set; }
    public DbSet<CardItem> CardItems { get; set; }
    public DbSet<SharedGroup> SharedGroups { get; set; }
    public DbSet<SharedGroupMember> SharedGroupMembers { get; set; }
    public DbSet<SharedCategory> SharedCategories { get; set; }
    public DbSet<SharedExpense> SharedExpenses { get; set; }
    public DbSet<SharedExpenseSplit> SharedExpenseSplits { get; set; }
    public DbSet<SharedSettlement> SharedSettlements { get; set; }
    public DbSet<OAuthClient> OAuthClients { get; set; }
    public DbSet<OAuthAuthorizationCode> OAuthAuthorizationCodes { get; set; }
    public DbSet<OAuthRefreshToken> OAuthRefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Budget>()
            .HasIndex(b => new { b.UserId, b.AccountId, b.Year, b.Month })
            .IsUnique();

        modelBuilder.Entity<Person>()
            .Property(p => p.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<CardItem>(e =>
        {
            e.Property(i => i.Name).HasMaxLength(200);
            e.HasIndex(i => i.UserId);
            // The associated account is optional: deleting it just unlinks the item.
            e.HasOne(i => i.Account).WithMany().HasForeignKey(i => i.AccountId).OnDelete(DeleteBehavior.SetNull);
            // People can't be deleted while they have card items (checked in CardService).
            e.HasOne(i => i.Person).WithMany().HasForeignKey(i => i.PersonId).OnDelete(DeleteBehavior.Restrict);
        });

        // Shared expenses: everything hangs off the group and goes away with it.
        // Users are never deleted, so references to them are restricted.
        modelBuilder.Entity<SharedGroup>(e =>
        {
            e.Property(g => g.Name).HasMaxLength(100);
            e.Property(g => g.InviteCode).HasMaxLength(16);
            e.HasIndex(g => g.InviteCode).IsUnique();
        });

        modelBuilder.Entity<SharedGroupMember>(e =>
        {
            e.HasIndex(m => new { m.GroupId, m.UserId }).IsUnique();
            e.HasIndex(m => m.UserId);
            e.HasOne(m => m.Group).WithMany(g => g.Members).HasForeignKey(m => m.GroupId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.User).WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SharedCategory>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(100);
            e.HasOne(c => c.Group).WithMany(g => g.Categories).HasForeignKey(c => c.GroupId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SharedExpense>(e =>
        {
            e.Property(x => x.Description).HasMaxLength(200);
            e.HasIndex(x => new { x.GroupId, x.Date });
            e.HasOne(x => x.Group).WithMany().HasForeignKey(x => x.GroupId).OnDelete(DeleteBehavior.Cascade);
            // Deleting a category keeps its expenses, uncategorized.
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.PaidBy).WithMany().HasForeignKey(x => x.PaidByUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SharedExpenseSplit>(e =>
        {
            e.HasIndex(s => new { s.ExpenseId, s.UserId }).IsUnique();
            e.HasOne(s => s.Expense).WithMany(x => x.Splits).HasForeignKey(s => s.ExpenseId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SharedSettlement>(e =>
        {
            e.Property(s => s.Note).HasMaxLength(200);
            e.HasIndex(s => new { s.GroupId, s.Date });
            e.HasOne(s => s.Group).WithMany().HasForeignKey(s => s.GroupId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.FromUser).WithMany().HasForeignKey(s => s.FromUserId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.ToUser).WithMany().HasForeignKey(s => s.ToUserId).OnDelete(DeleteBehavior.Restrict);
        });

        // OAuth for MCP clients. Revoking a client removes its codes and tokens.
        modelBuilder.Entity<OAuthClient>(e =>
        {
            e.ToTable("oauth_clients");
            e.Property(c => c.ClientId).HasMaxLength(64);
            e.Property(c => c.Name).HasMaxLength(200);
            e.HasIndex(c => c.ClientId).IsUnique();
        });

        modelBuilder.Entity<OAuthAuthorizationCode>(e =>
        {
            e.ToTable("oauth_authorization_codes");
            e.HasIndex(c => c.CodeHash).IsUnique();
            e.HasOne(c => c.Client).WithMany().HasForeignKey(c => c.ClientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OAuthRefreshToken>(e =>
        {
            e.ToTable("oauth_refresh_tokens");
            e.HasIndex(t => t.TokenHash).IsUnique();
            e.HasOne(t => t.Client).WithMany().HasForeignKey(t => t.ClientId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
