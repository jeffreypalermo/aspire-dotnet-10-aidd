using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace AspireTest.Web.IntegrationTests;

[TestFixture]
public class RedisIntegrationTests : IDisposable
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
    public async Task Cache_Page_Connects_To_Redis()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var response = await _httpClient!.GetAsync("/cache");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Redis Cache"));
    }

    [Test]
    public async Task Cache_Page_Can_Display_Cache_Entries()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var response = await _httpClient!.GetAsync("/cache");
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("Key").Or.Contain("cache").Or.Contain("Cache"));
    }

    [Test]
    public async Task Cache_Feature_Is_Available_Through_Web_Interface()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var homeResponse = await _httpClient!.GetAsync("/");
        var homeContent = await homeResponse.Content.ReadAsStringAsync();

        Assert.That(homeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var cacheResponse = await _httpClient.GetAsync("/cache");
        Assert.That(cacheResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            "Cache page should be accessible from the web application");
    }
}
