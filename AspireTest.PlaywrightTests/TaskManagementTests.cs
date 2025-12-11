using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace AspireTest.PlaywrightTests;

[TestFixture]
public sealed class TaskManagementTests : PageTest
{
    private const string WebFrontendUrl = "https://localhost:5146/tasks"; // Aspire web frontend port

    [Test]
    public async Task TaskPage_Should_Load()
    {
        // Arrange & Act
        await Page.GotoAsync(WebFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        // Assert
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Task Manager" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task TaskPage_Should_Show_Add_Button()
    {
        // Arrange
        await Page.GotoAsync(WebFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        // Act & Assert
        var addButton = Page.GetByRole(AriaRole.Button, new() { Name = "Add New Task" });
        await Expect(addButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task Can_Create_New_Task()
    {
        // Arrange
        await Page.GotoAsync(WebFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        // Act - Click Add New Task button
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add New Task" }).ClickAsync();

        // Wait for form to appear
        await Page.WaitForSelectorAsync("text=Add New Task", new() { State = WaitForSelectorState.Visible });

        // Fill in task details
        await Page.GetByLabel("Title").FillAsync("Playwright Test Task");
        await Page.GetByLabel("Description").FillAsync("This task was created by Playwright");

        // Save the task
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        // Wait for the task to appear in the list
        await Page.WaitForSelectorAsync("text=Playwright Test Task");

        // Assert - Task should be visible
        await Expect(Page.GetByText("Playwright Test Task")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Can_Mark_Task_As_Complete()
    {
        // Arrange - Create a task first
        await Page.GotoAsync(WebFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add New Task" }).ClickAsync();
        await Page.GetByLabel("Title").FillAsync("Task to Complete");
        await Page.GetByLabel("Description").FillAsync("Will be marked as complete");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Page.WaitForSelectorAsync("text=Task to Complete");

        // Act - Find and click Mark Complete button for this task
        var taskCard = Page.Locator(".card:has-text('Task to Complete')");
        await taskCard.GetByRole(AriaRole.Button, new() { Name = "Mark Complete" }).ClickAsync();

        // Wait for the page to update
        await Page.WaitForTimeoutAsync(1000);

        // Assert - Task should now show as completed
        await Expect(taskCard.GetByText("Completed")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Can_Edit_Task()
    {
        // Arrange - Create a task first
        await Page.GotoAsync(WebFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add New Task" }).ClickAsync();
        await Page.GetByLabel("Title").FillAsync("Task to Edit");
        await Page.GetByLabel("Description").FillAsync("Original description");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Page.WaitForSelectorAsync("text=Task to Edit");

        // Act - Find and click Edit button for this task
        var taskCard = Page.Locator(".card:has-text('Task to Edit')");
        await taskCard.GetByRole(AriaRole.Button, new() { Name = "Edit" }).ClickAsync();

        // Update the task
        await Page.GetByLabel("Title").FillAsync("Edited Task Title");
        await Page.GetByLabel("Description").FillAsync("Updated description");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();

        // Wait for update
        await Page.WaitForSelectorAsync("text=Edited Task Title");

        // Assert - Task should show updated title
        await Expect(Page.GetByText("Edited Task Title")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Updated description")).ToBeVisibleAsync();
    }

    [Test]
    public async Task Can_Delete_Task()
    {
        // Arrange - Create a task first
        await Page.GotoAsync(WebFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add New Task" }).ClickAsync();
        await Page.GetByLabel("Title").FillAsync("Task to Delete");
        await Page.GetByLabel("Description").FillAsync("Will be deleted");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Save" }).ClickAsync();
        await Page.WaitForSelectorAsync("text=Task to Delete");

        // Act - Find and click Delete button for this task
        var taskCard = Page.Locator(".card:has-text('Task to Delete')");
        await taskCard.GetByRole(AriaRole.Button, new() { Name = "Delete" }).ClickAsync();

        // Wait for deletion
        await Page.WaitForTimeoutAsync(1000);

        // Assert - Task should no longer be visible
        await Expect(Page.GetByText("Task to Delete")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Can_Cancel_Adding_Task()
    {
        // Arrange
        await Page.GotoAsync(WebFrontendUrl, new() { WaitUntil = WaitUntilState.NetworkIdle });

        // Act - Click Add New Task button
        await Page.GetByRole(AriaRole.Button, new() { Name = "Add New Task" }).ClickAsync();

        // Fill in partial task details
        await Page.GetByLabel("Title").FillAsync("Cancelled Task");

        // Cancel instead of saving
        await Page.GetByRole(AriaRole.Button, new() { Name = "Cancel" }).ClickAsync();

        // Assert - Form should be hidden, task should not appear
        await Expect(Page.GetByText("Add New Task").First).Not.ToBeVisibleAsync();
        await Expect(Page.GetByText("Cancelled Task")).Not.ToBeVisibleAsync();
    }

    [Test]
    public async Task Task_Navigation_Link_Should_Work()
    {
        // Arrange
        await Page.GotoAsync("https://localhost:5146", new() { WaitUntil = WaitUntilState.NetworkIdle });

        // Act - Click the Tasks navigation link
        await Page.GetByRole(AriaRole.Link, new() { Name = "Tasks" }).ClickAsync();

        // Assert - Should navigate to tasks page
        await Expect(Page).ToHaveURLAsync(new Regex("/tasks$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Task Manager" })).ToBeVisibleAsync();
    }
}
