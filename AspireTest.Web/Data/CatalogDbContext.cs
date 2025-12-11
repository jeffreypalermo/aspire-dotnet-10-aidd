using AspireTest.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireTest.Web.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });

        // Seed initial data
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "Sample Product 1",
                Description = "This is a sample product stored in SQL Server",
                Price = 29.99m,
                StockQuantity = 100,
                CreatedDate = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "Sample Product 2",
                Description = "Another sample product from the catalog",
                Price = 49.99m,
                StockQuantity = 50,
                CreatedDate = DateTime.UtcNow
            }
        );
    }
}
