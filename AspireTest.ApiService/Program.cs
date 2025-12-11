using AspireTest.ApiService.Data;
using AspireTest.ApiService.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add DbContext
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseInMemoryDatabase("TasksDb"));

// Add Redis
builder.AddRedisClient("cache");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/", () => "API service is running. Navigate to /weatherforecast to see sample data.");

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Task Management Endpoints
app.MapGet("/tasks", async (TaskDbContext db) =>
{
    return await db.Tasks.OrderByDescending(t => t.CreatedDate).ToListAsync();
})
.WithName("GetTasks");

app.MapGet("/tasks/{id}", async (int id, TaskDbContext db) =>
{
    return await db.Tasks.FindAsync(id) is TaskItem task
        ? Results.Ok(task)
        : Results.NotFound();
})
.WithName("GetTaskById");

app.MapPost("/tasks", async (TaskItem task, TaskDbContext db) =>
{
    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
})
.WithName("CreateTask");

app.MapPut("/tasks/{id}", async (int id, TaskItem inputTask, TaskDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    task.Title = inputTask.Title;
    task.Description = inputTask.Description;
    task.IsCompleted = inputTask.IsCompleted;
    task.CompletedDate = inputTask.IsCompleted ? DateTime.UtcNow : null;

    await db.SaveChangesAsync();
    return Results.Ok(task);
})
.WithName("UpdateTask");

app.MapDelete("/tasks/{id}", async (int id, TaskDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteTask");

// Redis Cache Endpoints
app.MapPost("/cache/{key}", async (string key, CacheItem item, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var value = System.Text.Json.JsonSerializer.Serialize(item);
    await db.StringSetAsync(key, value, TimeSpan.FromMinutes(10));
    return Results.Created($"/cache/{key}", new { key, value = item, expiresIn = "10 minutes" });
})
.WithName("SetCacheValue");

app.MapGet("/cache/{key}", async (string key, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var value = await db.StringGetAsync(key);

    if (value.IsNullOrEmpty)
    {
        return Results.NotFound(new { message = $"Key '{key}' not found in cache" });
    }

    var item = System.Text.Json.JsonSerializer.Deserialize<CacheItem>(value.ToString());
    return Results.Ok(new { key, value = item });
})
.WithName("GetCacheValue");

app.MapGet("/cache", async (IConnectionMultiplexer redis) =>
{
    var server = redis.GetServer(redis.GetEndPoints().First());
    var db = redis.GetDatabase();
    var keys = server.Keys(pattern: "*").ToList();

    var items = new List<object>();
    foreach (var key in keys)
    {
        var value = await db.StringGetAsync(key);
        if (!value.IsNullOrEmpty)
        {
            try
            {
                var item = System.Text.Json.JsonSerializer.Deserialize<CacheItem>(value.ToString());
                items.Add(new { key = key.ToString(), value = item });
            }
            catch
            {
                items.Add(new { key = key.ToString(), value = value.ToString() });
            }
        }
    }

    return Results.Ok(items);
})
.WithName("GetAllCacheKeys");

app.MapDelete("/cache/{key}", async (string key, IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    var deleted = await db.KeyDeleteAsync(key);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteCacheValue");

app.MapDefaultEndpoints();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

record CacheItem(string Data, string? Metadata = null)
{
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

// Make the implicit Program class public for integration tests
public partial class Program { }
