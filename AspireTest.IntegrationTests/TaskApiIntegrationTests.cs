using System.Net;
using System.Net.Http.Json;
using AspireTest.ApiService.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AspireTest.IntegrationTests;

[TestFixture]
public class TaskApiIntegrationTests
{
    private static WebApplicationFactory<Program>? _factory;
    private static HttpClient? _client;

    [OneTimeSetUp]
    public void ClassInitialize()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void ClassCleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Test]
    public async Task GetTasks_ReturnsSuccessAndTasks()
    {
        // Act
        var response = await _client!.GetAsync("/tasks");

        // Assert
        response.EnsureSuccessStatusCode();
        var tasks = await response.Content.ReadFromJsonAsync<TaskItem[]>();
        Assert.IsNotNull(tasks);
        Assert.That(tasks.Length, Is.GreaterThanOrEqualTo(2)); // At least the 2 seeded tasks
    }

    [Test]
    public async Task CreateTask_ReturnsCreatedTask()
    {
        // Arrange
        var newTask = new TaskItem
        {
            Title = "Integration Test Task",
            Description = "Created during integration testing",
            CreatedDate = DateTime.UtcNow
        };

        // Act
        var response = await _client!.PostAsJsonAsync("/tasks", newTask);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>();
        Assert.IsNotNull(createdTask);
        Assert.AreEqual("Integration Test Task", createdTask.Title);
        Assert.That(createdTask.Id, Is.GreaterThan(0));
    }

    [Test]
    public async Task GetTaskById_ExistingTask_ReturnsTask()
    {
        // Arrange - Create a task first
        var newTask = new TaskItem
        {
            Title = "Test Task for GetById",
            Description = "Testing GetById endpoint",
            CreatedDate = DateTime.UtcNow
        };
        var createResponse = await _client!.PostAsJsonAsync("/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>();

        // Act
        var response = await _client.GetAsync($"/tasks/{createdTask!.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var task = await response.Content.ReadFromJsonAsync<TaskItem>();
        Assert.IsNotNull(task);
        Assert.AreEqual(createdTask.Id, task.Id);
        Assert.AreEqual("Test Task for GetById", task.Title);
    }

    [Test]
    public async Task GetTaskById_NonExistentTask_ReturnsNotFound()
    {
        // Act
        var response = await _client!.GetAsync("/tasks/99999");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task UpdateTask_ExistingTask_ReturnsUpdatedTask()
    {
        // Arrange - Create a task first
        var newTask = new TaskItem
        {
            Title = "Task to Update",
            Description = "Will be updated",
            CreatedDate = DateTime.UtcNow
        };
        var createResponse = await _client!.PostAsJsonAsync("/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>();

        // Modify the task
        createdTask!.Title = "Updated Task Title";
        createdTask.IsCompleted = true;

        // Act
        var response = await _client.PutAsJsonAsync($"/tasks/{createdTask.Id}", createdTask);

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedTask = await response.Content.ReadFromJsonAsync<TaskItem>();
        Assert.IsNotNull(updatedTask);
        Assert.AreEqual("Updated Task Title", updatedTask.Title);
        Assert.IsTrue(updatedTask.IsCompleted);
        Assert.IsNotNull(updatedTask.CompletedDate);
    }

    [Test]
    public async Task UpdateTask_NonExistentTask_ReturnsNotFound()
    {
        // Arrange
        var taskToUpdate = new TaskItem
        {
            Id = 99999,
            Title = "Non-existent Task",
            CreatedDate = DateTime.UtcNow
        };

        // Act
        var response = await _client!.PutAsJsonAsync($"/tasks/{taskToUpdate.Id}", taskToUpdate);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task DeleteTask_ExistingTask_ReturnsNoContent()
    {
        // Arrange - Create a task first
        var newTask = new TaskItem
        {
            Title = "Task to Delete",
            Description = "Will be deleted",
            CreatedDate = DateTime.UtcNow
        };
        var createResponse = await _client!.PostAsJsonAsync("/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>();

        // Act
        var response = await _client.DeleteAsync($"/tasks/{createdTask!.Id}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

        // Verify task is deleted
        var getResponse = await _client.GetAsync($"/tasks/{createdTask.Id}");
        Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Test]
    public async Task DeleteTask_NonExistentTask_ReturnsNotFound()
    {
        // Act
        var response = await _client!.DeleteAsync("/tasks/99999");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task CreateAndCompleteTask_Workflow()
    {
        // Arrange - Create a task
        var newTask = new TaskItem
        {
            Title = "Workflow Test Task",
            Description = "Testing complete workflow",
            CreatedDate = DateTime.UtcNow
        };

        // Act 1 - Create task
        var createResponse = await _client!.PostAsJsonAsync("/tasks", newTask);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskItem>();
        Assert.IsNotNull(createdTask);
        Assert.IsFalse(createdTask.IsCompleted);

        // Act 2 - Mark as completed
        createdTask.IsCompleted = true;
        var updateResponse = await _client.PutAsJsonAsync($"/tasks/{createdTask.Id}", createdTask);
        var updatedTask = await updateResponse.Content.ReadFromJsonAsync<TaskItem>();

        // Assert
        Assert.IsNotNull(updatedTask);
        Assert.IsTrue(updatedTask.IsCompleted);
        Assert.IsNotNull(updatedTask.CompletedDate);
    }
}
