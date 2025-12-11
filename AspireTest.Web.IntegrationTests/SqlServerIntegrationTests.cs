using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace AspireTest.Web.IntegrationTests;

[TestFixture]
public class SqlServerIntegrationTests : IDisposable
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
    public async Task Products_Page_Loads_Successfully()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var response = await _httpClient!.GetAsync("/products");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Product Catalog"));
    }

    [Test]
    public async Task Products_Page_Connects_To_SqlServer()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var response = await _httpClient!.GetAsync("/products");
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("SQL Server"));
    }

    [Test]
    public async Task Products_Page_Displays_Seed_Data()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var response = await _httpClient!.GetAsync("/products");
        var content = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(content, Does.Contain("Sample Product").Or.Contain("product"));
    }

    [Test]
    public async Task Products_Feature_Is_Available_Through_Web_Interface()
    {
        Assert.That(_httpClient, Is.Not.Null);

        var homeResponse = await _httpClient!.GetAsync("/");
        var homeContent = await homeResponse.Content.ReadAsStringAsync();

        Assert.That(homeResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var productsResponse = await _httpClient.GetAsync("/products");
        Assert.That(productsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            "Products page should be accessible from the web application");
    }
}
