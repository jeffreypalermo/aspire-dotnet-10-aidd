using AspireTest.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireTest.ApiService.Data;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedDate).IsRequired();
        });

        // Seed some initial data
        modelBuilder.Entity<TaskItem>().HasData(
            new TaskItem
            {
                Id = 1,
                Title = "Welcome to Task Manager",
                Description = "This is a sample task. Try creating, updating, and deleting tasks!",
                CreatedDate = DateTime.UtcNow
            },
            new TaskItem
            {
                Id = 2,
                Title = "Complete the Aspire tutorial",
                Description = "Learn about .NET Aspire distributed applications",
                CreatedDate = DateTime.UtcNow
            }
        );
    }
}
