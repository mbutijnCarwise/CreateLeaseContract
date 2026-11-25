using Microsoft.Playwright;
using Reqnroll;

namespace CreateLeaseContract.Tests.Steps;

[Binding]
public sealed class StepDefinitionsAccountWise
{
    private readonly IPage _page;
    
    public StepDefinitionsAccountWise(IPage page)
    {
        _page = page;
    }

    [Given("I have navigated to (.*) in the main menu of AccountWise")]
    public async Task NavigateToAccountWiseMenu(string module)
    {
        ILocator accountWise = _page.GetByRole(AriaRole.Link, new() { Name = "AccountWise" });
        string href = await accountWise.GetAttributeAsync("href")
            ?? throw new InvalidOperationException("The AccountWise module link has no href.");
        await _page.GotoAsync(new Uri(new Uri(_page.Url), href).ToString());
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        ILocator algemeen = _page.GetByRole(AriaRole.Treeitem, new() { Name = "Algemeen", Exact = true, Level = 1 });
        await Assertions.Expect(algemeen).ToBeVisibleAsync();
        if (await algemeen.GetAttributeAsync("aria-expanded") == "true")
            await algemeen.Locator("xpath=../button").ClickAsync();
        await Assertions.Expect(algemeen).ToHaveAttributeAsync("aria-expanded", "false");

        ILocator menuItem = _page.GetByRole(AriaRole.Navigation, new() { Name = "AccountWise" })
            .GetByRole(AriaRole.Treeitem, new() { Name = module, Exact = true, Level = 1 });
        await Assertions.Expect(menuItem).ToBeVisibleAsync();
        if (await menuItem.GetAttributeAsync("aria-expanded") == "false")
            await menuItem.Locator("xpath=../button").ClickAsync();
        await Assertions.Expect(menuItem).ToHaveAttributeAsync("aria-expanded", "true");
    }

    [Then(@"I should see the following items are in the (.*) sub menu of AccountWise")]
    public async Task VerifySubMenuItems(string module, Table table)
    {
        ILocator menu = _page.GetByRole(AriaRole.Group, new() { Name = module, Exact = true });
        foreach (var row in table.Rows)
        {
            ILocator menuItem = menu.GetByRole(AriaRole.Treeitem, new() { Name = row[0], Exact = true });
            await Assertions.Expect(menuItem).ToBeVisibleAsync();
        }
    }
}
