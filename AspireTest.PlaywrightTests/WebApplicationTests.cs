using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace AspireTest.PlaywrightTests;

[TestClass]
public sealed class WebApplicationTests : PageTest
{
    // We'll need to find the actual web frontend URL from the dashboard
    // For now, assuming it will be on a dynamic port
    private string? _webFrontendUrl;

    [TestInitialize]
    public async Task Setup()
    {
        // Navigate to dashboard to find the web frontend URL
        await Page.GotoAsync("https://localhost:17297/login?t=1249ea9b1ec2663f9b45b4c37561e83a");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Try to find webfrontend endpoint
        // This is a simplified approach - in practice, we'd parse the dashboard API
        _webFrontendUrl = "https://localhost:5146"; // Aspire web frontend port
    }

    [TestMethod]
    public async Task WebFrontend_Should_Load_Successfully()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        // Arrange & Act
        await Page.GotoAsync(_webFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        // Assert - Page should load without errors
        Assert.IsTrue(await Page.TitleAsync() != "");
    }

    [TestMethod]
    public async Task WebFrontend_Should_Display_Weather_Data()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        // Arrange
        await Page.GotoAsync(_webFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        // Act - Look for weather-related content
        var hasWeatherContent = await Page.GetByText("Weather", new() { Exact = false }).CountAsync() > 0 ||
                               await Page.GetByText("Temperature", new() { Exact = false }).CountAsync() > 0 ||
                               await Page.GetByText("Forecast", new() { Exact = false }).CountAsync() > 0;

        // Assert
        Assert.IsTrue(hasWeatherContent, "Web frontend should display weather-related content");
    }
}
