using System.Net.Http.Json;

namespace AspireTest.Web;

public class TaskApiClient(HttpClient httpClient)
{
    public async Task<TaskItem[]> GetTasksAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<TaskItem[]>("/tasks", cancellationToken) ?? [];
    }

    public async Task<TaskItem?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<TaskItem>($"/tasks/{id}", cancellationToken);
    }

    public async Task<TaskItem?> CreateTaskAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/tasks", task, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TaskItem>(cancellationToken);
    }

    public async Task<TaskItem?> UpdateTaskAsync(int id, TaskItem task, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"/tasks/{id}", task, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TaskItem>(cancellationToken);
    }

    public async Task DeleteTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/tasks/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

public record TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}
