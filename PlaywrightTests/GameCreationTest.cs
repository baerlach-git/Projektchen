using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.RegularExpressions;


namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class GameCreationTest : PageTest
{
    [Test]
    public async Task CreateAndDeleteGame()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
        });
        var page = await context.NewPageAsync();

        await page.GotoAsync("https://localhost:7012/");
        
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Application" })).ToBeVisibleAsync();

        await page.GetByRole(AriaRole.Link, new() { Name = "MUDNEWGAME" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "MudNewGame" })).ToBeVisibleAsync();
        
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Name*" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Name*" }).FillAsync("Test");
        await page.GetByRole(AriaRole.Spinbutton, new() { Name = "Release Year*" }).ClickAsync();
        await page.GetByRole(AriaRole.Spinbutton, new() { Name = "Release Year*" }).FillAsync("2000");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Publisher*" }).ClickAsync();
        await page.GetByText("Circle Enix").ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Developer*" }).ClickAsync();
        await page.GetByText("Gutpunch Studios").ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Platform*" }).ClickAsync();
        await page.GetByText("PC").ClickAsync();
        await page.GetByText("Xbox").ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Platform*" }).PressAsync("Escape");
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Genre*" }).ClickAsync();
        await page.GetByText("Platformer").ClickAsync();
        await page.GetByText("RPG").ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Genre*" }).PressAsync("Escape");
        await page.GetByRole(AriaRole.Button, new() { Name = "Validate" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Paragraph)).ToContainTextAsync("0 errors");
        await page.GetByRole(AriaRole.Button, new() { Name = "Submit" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Added 1 games, 2 game genre relations and 2 game platform relations");
        await page.GetByRole(AriaRole.Link, new() { Name = "MUDGAMES" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Row, new() { Name = "Expand all groups Name Sort" })).ToBeVisibleAsync();
        
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Search" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Search" }).FillAsync("Test");
        await page.GetByRole(AriaRole.Row, new() { Name = "Test 2000" }).GetByRole(AriaRole.Button).ClickAsync();
        await Expect(page.Locator("tbody")).ToContainTextAsync("Release Year: 2000");
        await Expect(page.Locator("tbody")).ToContainTextAsync("Average Rating:");
        await Expect(page.Locator("tbody")).ToContainTextAsync("#Comments: 0");
        await Expect(page.Locator("tbody")).ToContainTextAsync("Genre: Platformer,RPG");
        await Expect(page.Locator("tbody")).ToContainTextAsync("Platform: PC,Xbox");
        await Expect(page.Locator("tbody")).ToContainTextAsync("Developer: Gutpunch Studios");
        await Expect(page.Locator("tbody")).ToContainTextAsync("Publisher: Circle Enix");
        await page.GetByRole(AriaRole.Cell, new() { Name = "Test Release Year: 2000" }).GetByRole(AriaRole.Button).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Close" })).ToBeVisibleAsync();
        
        await page.GetByRole(AriaRole.Button, new() { Name = "Delete Test" }).ClickAsync();
        await Expect(page.GetByRole(AriaRole.Alert)).ToContainTextAsync("Deleted 1 games, 2 game genre relations, 2 game platform relations, 0 comments and 0 ratings.");

    }
}









