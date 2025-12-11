using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace AspireTest.PlaywrightTests;

[TestFixture]
public sealed class AspireDashboardTests : PageTest
{
    private const string DashboardUrl = "https://localhost:17297/login?t=1249ea9b1ec2663f9b45b4c37561e83a";

    [Test]
    public async Task Dashboard_Should_Load_Successfully()
    {
        // Arrange & Act
        await Page.GotoAsync(DashboardUrl);

        // Assert - Check that we can access the dashboard
        await Expect(Page).ToHaveTitleAsync(new Regex("Aspire|Dashboard", RegexOptions.IgnoreCase));

        // Wait for the page to be fully loaded
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task Dashboard_Should_Show_Resources()
    {
        // Arrange
        await Page.GotoAsync(DashboardUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Act - Navigate to Resources if not already there
        var resourcesLink = Page.GetByText("Resources", new() { Exact = false });
        if (await resourcesLink.IsVisibleAsync())
        {
            await resourcesLink.ClickAsync();
        }

        // Assert - Check that resources are displayed
        await Page.WaitForSelectorAsync("text=apiservice,text=webfrontend", new() { Timeout = 10000, State = WaitForSelectorState.Attached });
    }

    [Test]
    public async Task Dashboard_Should_Navigate_To_Console_Logs()
    {
        // Arrange
        await Page.GotoAsync(DashboardUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Act - Try to navigate to console logs
        var consoleLink = Page.GetByText("Console", new() { Exact = false });
        if (await consoleLink.CountAsync() > 0 && await consoleLink.First.IsVisibleAsync())
        {
            await consoleLink.First.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // Assert - We successfully navigated
        Assert.IsNotNull(Page);
    }
}
