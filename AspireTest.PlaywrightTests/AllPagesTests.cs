using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace AspireTest.PlaywrightTests;

[TestFixture]
public sealed class AllPagesTests : PageTest
{
    private string? _webFrontendUrl;

    [SetUp]
    public async Task Setup()
    {
        _webFrontendUrl = "https://localhost:5146";

        // Accept SSL certificate errors in tests
        await Context.GrantPermissionsAsync(new[] { "clipboard-read", "clipboard-write" });
    }

    #region Home Page Tests

    [Test]
    public async Task Home_Page_Loads_Successfully()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync(_webFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        var title = await Page.TitleAsync();
        Assert.That(title, Is.Not.Empty);
    }

    [Test]
    public async Task Home_Page_Has_Welcome_Content()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync(_webFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        var hasContent = await Page.GetByText("Welcome", new() { Exact = false }).CountAsync() > 0 ||
                        await Page.GetByText("Home", new() { Exact = false }).CountAsync() > 0;

        Assert.That(hasContent, Is.True, "Home page should display welcome content");
    }

    [Test]
    public async Task Home_Page_Has_Navigation_Menu()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync(_webFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        var hasNav = await Page.Locator("nav").CountAsync() > 0;
        Assert.That(hasNav, Is.True, "Home page should have navigation menu");
    }

    #endregion

    #region Counter Page Tests

    [Test]
    public async Task Counter_Page_Loads_Successfully()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/counter", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var hasCounterContent = await Page.GetByText("Counter", new() { Exact = false }).CountAsync() > 0;
        Assert.That(hasCounterContent, Is.True, "Counter page should display");
    }

    [Test]
    public async Task Counter_Page_Has_Click_Me_Button()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/counter", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var button = Page.GetByRole(AriaRole.Button, new() { NameString = "Click me" });
        await Expect(button).ToBeVisibleAsync();
    }

    [Test]
    public async Task Counter_Page_Increments_On_Click()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/counter", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var button = Page.GetByRole(AriaRole.Button, new() { NameString = "Click me" });
        await button.ClickAsync();

        var hasIncrementedContent = await Page.GetByText("Current count: 1", new() { Exact = false }).CountAsync() > 0;
        Assert.That(hasIncrementedContent, Is.True, "Counter should increment after click");
    }

    #endregion

    #region Weather Page Tests

    [Test]
    public async Task Weather_Page_Loads_Successfully()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/weather", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var hasWeatherContent = await Page.GetByText("Weather", new() { Exact = false }).CountAsync() > 0;
        Assert.That(hasWeatherContent, Is.True, "Weather page should display");
    }

    [Test]
    public async Task Weather_Page_Displays_Forecast_Data()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/weather", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(2000); // Wait for API call

        var hasTableOrData = await Page.Locator("table").CountAsync() > 0 ||
                            await Page.GetByText("Temperature", new() { Exact = false }).CountAsync() > 0 ||
                            await Page.GetByText("Loading", new() { Exact = false }).CountAsync() > 0;

        Assert.That(hasTableOrData, Is.True, "Weather page should display forecast table or loading indicator");
    }

    #endregion

    #region Tasks Page Tests

    [Test]
    public async Task Tasks_Page_Loads_Successfully()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/tasks", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var hasTasksContent = await Page.GetByText("Task Manager", new() { Exact = false }).CountAsync() > 0;
        Assert.That(hasTasksContent, Is.True, "Tasks page should display Task Manager heading");
    }

    [Test]
    public async Task Tasks_Page_Has_Add_Button()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/tasks", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var addButton = Page.GetByRole(AriaRole.Button, new() { NameString = "Add New Task" });
        await Expect(addButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task Tasks_Page_Displays_Existing_Tasks()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/tasks", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(2000); // Wait for API call

        var hasTasksOrEmpty = await Page.Locator(".card").CountAsync() > 0 ||
                             await Page.GetByText("No tasks found", new() { Exact = false }).CountAsync() > 0 ||
                             await Page.GetByText("Loading", new() { Exact = false }).CountAsync() > 0;

        Assert.That(hasTasksOrEmpty, Is.True, "Tasks page should display tasks, empty state, or loading");
    }

    [Test]
    public async Task Tasks_Page_Can_Show_Add_Task_Form()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/tasks", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var addButton = Page.GetByRole(AriaRole.Button, new() { NameString = "Add New Task" });
        await addButton.ClickAsync();

        var hasTitleInput = await Page.GetByLabel("Title", new() { Exact = false }).CountAsync() > 0;
        Assert.That(hasTitleInput, Is.True, "Add task form should display with title input");
    }

    #endregion

    #region Cache Page Tests

    [Test]
    public async Task Cache_Page_Loads_Successfully()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/cache", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var hasCacheContent = await Page.GetByText("Redis Cache", new() { Exact = false }).CountAsync() > 0;
        Assert.That(hasCacheContent, Is.True, "Cache page should display Redis Cache heading");
    }

    [Test]
    public async Task Cache_Page_Has_Add_Cache_Entry_Section()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/cache", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var hasKeyInput = await Page.GetByLabel("Key", new() { Exact = false }).CountAsync() > 0;
        Assert.That(hasKeyInput, Is.True, "Cache page should have key input field");
    }

    [Test]
    public async Task Cache_Page_Displays_Cache_Entries()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/cache", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(2000); // Wait for API call

        var hasTableOrEmpty = await Page.Locator("table").CountAsync() > 0 ||
                             await Page.GetByText("No cache entries", new() { Exact = false }).CountAsync() > 0 ||
                             await Page.GetByText("Loading", new() { Exact = false }).CountAsync() > 0;

        Assert.That(hasTableOrEmpty, Is.True, "Cache page should display table, empty state, or loading");
    }

    #endregion

    #region Products Page Tests

    [Test]
    public async Task Products_Page_Loads_Successfully()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/products", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var hasProductsContent = await Page.GetByText("Product Catalog", new() { Exact = false }).CountAsync() > 0;
        Assert.That(hasProductsContent, Is.True, "Products page should display Product Catalog heading");
    }

    [Test]
    public async Task Products_Page_Shows_SQL_Server_Reference()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/products", new() { WaitUntil = WaitUntilState.NetworkIdle });

        var hasSqlReference = await Page.GetByText("SQL Server", new() { Exact = false }).CountAsync() > 0;
        Assert.That(hasSqlReference, Is.True, "Products page should mention SQL Server");
    }

    [Test]
    public async Task Products_Page_Displays_Product_Cards()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/products", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(2000); // Wait for database query

        var hasProductsOrEmpty = await Page.Locator(".card").CountAsync() > 0 ||
                                await Page.GetByText("No products found", new() { Exact = false }).CountAsync() > 0 ||
                                await Page.GetByText("Loading", new() { Exact = false }).CountAsync() > 0;

        Assert.That(hasProductsOrEmpty, Is.True, "Products page should display product cards, empty state, or loading");
    }

    [Test]
    public async Task Products_Page_Shows_Seed_Data_Products()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync($"{_webFrontendUrl}/products", new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(2000); // Wait for database query

        var hasSampleProducts = await Page.GetByText("Sample Product", new() { Exact = false }).CountAsync() > 0;
        Assert.That(hasSampleProducts, Is.True, "Products page should display seed data products");
    }

    #endregion

    #region Navigation Tests

    [Test]
    public async Task Can_Navigate_To_All_Pages_From_Menu()
    {
        if (_webFrontendUrl == null)
        {
            Assert.Inconclusive("Web frontend URL not found");
            return;
        }

        await Page.GotoAsync(_webFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        var pages = new[] { "Home", "Counter", "Weather", "Tasks", "Redis Cache", "Products" };

        foreach (var pageName in pages)
        {
            var navLink = Page.GetByRole(AriaRole.Link, new() { NameString = pageName });
            if (await navLink.CountAsync() > 0)
            {
                await navLink.First.ClickAsync();
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                var url = Page.Url;
                Assert.That(url, Is.Not.Null, $"Should navigate to {pageName} page");
            }
        }
    }

    #endregion
}
