using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;

namespace CreateLeaseContract.Tests.Steps;

[Binding]
public class StepDefinitionsCRM
{
    protected readonly ActivePage _activePage;
    private IPage _page => _activePage.Page;

    public StepDefinitionsCRM(ActivePage page)
    {
        _activePage = page;
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
