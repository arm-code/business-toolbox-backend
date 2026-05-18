using BusinessToolbox.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessToolbox.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration for Expense
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.UserId).IsRequired();
        });

        // Seed some default categories with fixed GUIDs
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = Guid.Parse("f1a7d6e4-4c4c-4e4c-8e4c-1c4c4e4c4c4c"), Name = "Comida", Icon = "🍎" },
            new Category { Id = Guid.Parse("f2a7d6e4-4c4c-4e4c-8e4c-1c4c4e4c4c4c"), Name = "Transporte", Icon = "🚗" },
            new Category { Id = Guid.Parse("f3a7d6e4-4c4c-4e4c-8e4c-1c4c4e4c4c4c"), Name = "Servicios", Icon = "💡" },
            new Category { Id = Guid.Parse("f4a7d6e4-4c4c-4e4c-8e4c-1c4c4e4c4c4c"), Name = "Entretenimiento", Icon = "🎬" },
            new Category { Id = Guid.Parse("f5a7d6e4-4c4c-4e4c-8e4c-1c4c4e4c4c4c"), Name = "Otros", Icon = "📦" }
        );
    }
}
