using AspireTest.ApiService.Data;
using AspireTest.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireTest.ApiService.Tests;

[TestFixture]
public class TaskDbContextTests
{
    private TaskDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TaskDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new TaskDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Test]
    public async Task CanAddTask()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var task = new TaskItem
        {
            Title = "Test Task",
            Description = "Test Description",
            CreatedDate = DateTime.UtcNow
        };

        // Act
        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        // Assert
        var tasks = await context.Tasks.ToListAsync();
        Assert.AreEqual(3, tasks.Count); // 2 seeded + 1 added
        Assert.IsTrue(tasks.Any(t => t.Title == "Test Task"));
    }

    [Test]
    public async Task CanUpdateTask()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var task = context.Tasks.First();
        var originalTitle = task.Title;

        // Act
        task.Title = "Updated Title";
        task.IsCompleted = true;
        task.CompletedDate = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Assert
        var updatedTask = await context.Tasks.FindAsync(task.Id);
        Assert.IsNotNull(updatedTask);
        Assert.AreNotEqual(originalTitle, updatedTask.Title);
        Assert.AreEqual("Updated Title", updatedTask.Title);
        Assert.IsTrue(updatedTask.IsCompleted);
        Assert.IsNotNull(updatedTask.CompletedDate);
    }

    [Test]
    public async Task CanDeleteTask()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var task = context.Tasks.First();
        var taskId = task.Id;

        // Act
        context.Tasks.Remove(task);
        await context.SaveChangesAsync();

        // Assert
        var deletedTask = await context.Tasks.FindAsync(taskId);
        Assert.IsNull(deletedTask);
    }

    [Test]
    public async Task CanQueryCompletedTasks()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var task = context.Tasks.First();
        task.IsCompleted = true;
        task.CompletedDate = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // Act
        var completedTasks = await context.Tasks
            .Where(t => t.IsCompleted)
            .ToListAsync();

        // Assert
        Assert.AreEqual(1, completedTasks.Count);
        Assert.IsTrue(completedTasks.All(t => t.IsCompleted));
    }

    [Test]
    public async Task DatabaseSeedDataExists()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        // Act
        var tasks = await context.Tasks.ToListAsync();

        // Assert
        Assert.AreEqual(2, tasks.Count);
        Assert.IsTrue(tasks.Any(t => t.Title == "Welcome to Task Manager"));
        Assert.IsTrue(tasks.Any(t => t.Title == "Complete the Aspire tutorial"));
    }

    [Test]
    public void TaskItem_ShouldRequireTitle()
    {
        // Arrange & Act
        var task = new TaskItem
        {
            Description = "Test Description"
        };

        // Assert
        Assert.AreEqual(string.Empty, task.Title);
    }

    [Test]
    public void TaskItem_ShouldHaveCreatedDate()
    {
        // Arrange & Act
        var task = new TaskItem
        {
            Title = "Test Task"
        };

        // Assert
        Assert.AreNotEqual(default(DateTime), task.CreatedDate);
        Assert.IsTrue(task.CreatedDate <= DateTime.UtcNow);
    }
}
