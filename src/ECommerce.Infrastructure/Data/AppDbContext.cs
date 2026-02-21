using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Data;

/// <summary>
/// EF Core DbContext for PostgreSQL.
/// Handles Users and StockItems.
/// Orders are now in MongoDB (see MongoDbContext).
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<StockItem> StockItems => Set<StockItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
        });

        // StockItem
        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.HasIndex(s => s.ProductId).IsUnique();
            entity.Property(s => s.ProductId).HasMaxLength(24).IsRequired();
        });
    }
}
