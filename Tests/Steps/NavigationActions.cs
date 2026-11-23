using Microsoft.Playwright;

namespace CreateLeaseContract.Tests.Steps;

internal static class NavigationActions
{
    public static Task OpenInNavigation(IPage page, string item) =>
        page.GetByText(item).First.ClickAsync();
}