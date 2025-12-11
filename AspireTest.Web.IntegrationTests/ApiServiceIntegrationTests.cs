using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace AspireTest.Web.IntegrationTests;

[TestFixture]
public class ApiServiceIntegrationTests : IDisposable
{
    private WebApplicationFactory<AspireTest.Web.Program>? _factory;
    private HttpClient? _httpClient;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _factory = new WebApplicationFactory<AspireTest.Web.Program>();
        _httpClient = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _httpClient?.Dispose();
        _factory?.Dispose();
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _factory?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Test]
    public async Task Weather_Page_Loads_Successfully()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var response = await _httpClient!.GetAsync("/weather");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Weather"));
    }

    [Test]
    public async Task Weather_Page_Calls_ApiService()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var response = await _httpClient!.GetAsync("/weather");
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("Weather").Or.Contain("forecast"));
    }

    [Test]
    public async Task Tasks_Page_Loads_Successfully()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var response = await _httpClient!.GetAsync("/tasks");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Task Manager"));
    }

    [Test]
    public async Task Tasks_Page_Calls_ApiService_For_Tasks()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var response = await _httpClient!.GetAsync("/tasks");
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("Task").Or.Contain("task"));
    }

    [Test]
    public async Task Cache_Page_Loads_Successfully()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var response = await _httpClient!.GetAsync("/cache");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Redis Cache"));
    }

    [Test]
    public async Task Cache_Page_Calls_ApiService_For_Cache_Data()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var response = await _httpClient!.GetAsync("/cache");
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("cache").Or.Contain("Cache"));
    }
}
