using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;


namespace PlaywrightTests;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class MudCounterTest : PageTest
{
    [Test]
    public async Task CounterWorks()
    {
     var context =  await Browser.NewContextAsync(new BrowserNewContextOptions
     {
         IgnoreHTTPSErrors = true,
     });
    var page = await context.NewPageAsync();
    
    await page.GotoAsync("https://localhost:7012/");
    await Expect(page.GetByRole(AriaRole.Heading, new() { Name = "Application" })).ToBeVisibleAsync();
    await page.GetByRole(AriaRole.Link, new() { Name = "MUDCOUNTER" }).ClickAsync();
    await Expect(page.Locator("h6")).ToContainTextAsync("Current Count: 0");
    await page.GetByRole(AriaRole.Button, new() { Name = "Increment Count" }).ClickAsync();
    await Expect(page.Locator("h6")).ToContainTextAsync("Current Count: 1");

    }
}