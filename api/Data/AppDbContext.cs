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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Budget>()
            .HasIndex(b => new { b.UserId, b.AccountId, b.Year, b.Month })
            .IsUnique();
    }
}