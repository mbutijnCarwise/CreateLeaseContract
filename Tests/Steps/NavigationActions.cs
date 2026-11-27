using Microsoft.Playwright;
using NUnit.Framework;

namespace CreateLeaseContract.Tests.Steps;

internal static class NavigationActions
{
    public static async Task OpenGroupAsync(IPage page, string groupName)
    {
        ILocator navigation = page.GetByRole(AriaRole.Navigation)
            .Filter(new() { Has = page.GetByRole(AriaRole.Treeitem, new() { Name = groupName, Exact = true }) });
        if (groupName != "Algemeen")
        {
            await SetGroupExpandedAsync(
                navigation.GetByRole(AriaRole.Treeitem, new() { Name = "Algemeen", Exact = true }), false);
        }

        await SetGroupExpandedAsync(
            navigation.GetByRole(AriaRole.Treeitem, new() { Name = groupName, Exact = true }), true);

        ILocator startPage = navigation.GetByRole(AriaRole.Group, new() { Name = groupName, Exact = true })
            .GetByRole(AriaRole.Treeitem, new() { Name = "Startpagina", Exact = true });
        await startPage.ClickAsync();
        await Assertions.Expect(startPage).ToHaveAttributeAsync("aria-selected", "true");
    }

    private static async Task SetGroupExpandedAsync(ILocator group, bool expanded)
    {
        await Assertions.Expect(group).ToBeVisibleAsync();
        string expected = expanded ? "true" : "false";
        const int maxAttempts = 5;
        for (int attempt = 1; ; attempt++)
        {
            if (await group.GetAttributeAsync("aria-expanded") != expected)
            {
                // The tree's toggle button is a sibling of the treeitem and is hidden from the accessibility tree.
                await group.Locator("xpath=..")
                    .GetByRole(AriaRole.Button, new() { IncludeHidden = true }).ClickAsync();
            }
            try
            {
                await Assertions.Expect(group).ToHaveAttributeAsync("aria-expanded", expected, new() { Timeout = 3000 });
                return;
            }
            catch (PlaywrightException) when (attempt < maxAttempts)
            {
                TestContext.Out.WriteLine($"Navigation group did not reach aria-expanded={expected} (attempt {attempt}); retrying.");
            }
        }
    }

    public static async Task OpenInNavigation(IPage page, string item)
    {
        string[] path = item.Split(" -> ");
        ILocator navigation = page.GetByRole(AriaRole.Navigation)
            .Filter(new() { Has = page.GetByRole(AriaRole.Treeitem, new() { Name = path[0], Exact = true }) });
        ILocator scope = navigation;
        for (int index = 0; index < path.Length; index++)
        {
            ILocator navigationItem = scope.GetByRole(AriaRole.Treeitem, new() { Name = path[index], Exact = true })
                .Filter(new() { Visible = true });
            await Assertions.Expect(navigationItem).ToBeVisibleAsync();
            if (await navigationItem.GetAttributeAsync("aria-expanded") is not null)
            {
                await SetGroupExpandedAsync(navigationItem, true);
                scope = scope.GetByRole(AriaRole.Group, new() { Name = path[index], Exact = true });
            }
            else
            {
                Assert.That(index, Is.EqualTo(path.Length - 1), $"Navigation item '{path[index]}' is not an expandable group.");
                await navigationItem.ClickAsync();
            }
        }
    }

    public static Task CloseInNavigation(IPage page, string groupName) =>
        SetGroupExpandedAsync(
            page.GetByRole(AriaRole.Navigation)
                .GetByRole(AriaRole.Treeitem, new() { Name = groupName, Exact = true }), false);
}