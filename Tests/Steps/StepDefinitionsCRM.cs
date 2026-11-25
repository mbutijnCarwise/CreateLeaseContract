using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;

namespace CreateLeaseContract.Tests.Steps;

[Binding]
public class StepDefinitionsCRM
{
    private readonly IPage _page;

    public StepDefinitionsCRM(IPage page)
    {
        _page = page;
    }

    [Then(@"the following menu items are present")]
    public async Task TheFollowingMenuItemsArePresent(Table table)
    {
        // A single-row Gherkin table is parsed as the header, so the items live in table.Header.
        foreach (string item in table.Header)
        {
            ILocator menuItem = _page.GetByRole(AriaRole.Link, new() { Name = item });
            await Assertions.Expect(menuItem).ToBeVisibleAsync();
        }
    }
}
