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
    }
}