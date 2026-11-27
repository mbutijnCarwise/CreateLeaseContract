using CreateLeaseContract.Tests.Steps;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace CreateLeaseContract.Tests;

public class NavigationActionsTests : PageTest
{
    private async Task PrepareModuleAsync(bool activitiesExpanded, bool ignoreFirstToggle)
    {
        await Context.RouteAsync("https://navigation.test/module", route => route.FulfillAsync(new()
        {
            ContentType = "text/html",
            Body = """
                <h1>LeaseWise &middot; Algemeen</h1>
                <nav aria-label="LeaseWise">
                  <div role="tree">
                    <div>
                      <button aria-hidden="true"
                        onclick="const item=this.nextElementSibling; const expanded=item.getAttribute('aria-expanded') !== 'true'; item.setAttribute('aria-expanded', expanded); document.querySelector('[role=group][aria-label=Algemeen]').hidden=!expanded">Toggle</button>
                      <div role="treeitem" aria-expanded="true">Algemeen</div>
                    </div>
                    <div role="group" aria-label="Algemeen">
                      <a role="treeitem" href="#" aria-selected="false"
                        onclick="this.setAttribute('aria-selected', 'true'); document.querySelector('h1').innerHTML='LeaseWise &middot; Algemeen'">Startpagina</a>
                    </div>
                    <div>
                      <button aria-hidden="true"
                        onclick="const item=this.nextElementSibling; const expanded=item.getAttribute('aria-expanded') !== 'true'; item.setAttribute('aria-expanded', expanded); document.querySelector('[role=group][aria-label=Activiteiten]').hidden=!expanded">Toggle</button>
                      <div role="treeitem" aria-expanded="false">Activiteiten</div>
                    </div>
                    <div role="group" aria-label="Activiteiten" hidden>
                      <a role="treeitem" href="#" aria-selected="false"
                        onclick="this.setAttribute('aria-selected', 'true'); document.querySelector('h1').innerHTML='LeaseWise &middot; Activiteiten'">Startpagina</a>
                      <a role="treeitem" href="#" onclick="document.querySelector('h1').textContent='Leaseoffertes'">Leaseoffertes</a>
                    </div>
                  </div>
                </nav>
                """
        }));
        await Page.SetContentAsync("""<a href="https://navigation.test/module" target="_blank">LeaseWise</a>""");
        if (ignoreFirstToggle)
        {
            await Context.AddInitScriptAsync("""
                document.addEventListener('click', event => {
                    event.stopImmediatePropagation();
                    event.preventDefault();
                }, { capture: true, once: true });
                """);
        }
        if (activitiesExpanded)
        {
            await Context.AddInitScriptAsync("""
                document.addEventListener('DOMContentLoaded', () => {
                    const item = [...document.querySelectorAll('[role=treeitem]')].find(el => el.textContent === 'Activiteiten');
                    item.setAttribute('aria-expanded', 'true');
                    document.querySelector('[role=group][aria-label=Activiteiten]').hidden = false;
                });
                """);
        }
    }

    [TestCase("LeaseWise Activiteiten", false, false)]
    [TestCase("LeaseWise Activiteiten", true, false)]
    [TestCase("LeaseWise Activiteiten", false, true)]
    [TestCase("LeaseWise Algemeen", false, false)]
    [TestCase("LeaseWise", false, false)]
    public async Task NavigateInModulesSelectsRequestedGroup(string moduleName, bool activitiesExpanded, bool ignoreFirstToggle)
    {
        await PrepareModuleAsync(activitiesExpanded, ignoreFirstToggle);
        var activePage = new ActivePage(Page);
        var steps = new StepDefinitionsCommon(activePage);
        await steps.NavigateInModules(moduleName);

        IPage modulePage = activePage.Page;
        bool requestedActivities = moduleName.EndsWith(" Activiteiten", StringComparison.Ordinal);
        await Expect(modulePage.GetByRole(AriaRole.Treeitem, new() { Name = "Algemeen", Exact = true }))
            .ToHaveAttributeAsync("aria-expanded", requestedActivities ? "false" : "true");
        await Expect(modulePage.GetByRole(AriaRole.Treeitem, new() { Name = "Activiteiten", Exact = true }))
            .ToHaveAttributeAsync("aria-expanded", requestedActivities ? "true" : "false");
        await steps.TheWindowIsShown(requestedActivities ? "LeaseWise \u00b7 Activiteiten" : "LeaseWise \u00b7 Algemeen");
        if (moduleName != "LeaseWise")
        {
            await Expect(modulePage.GetByRole(AriaRole.Group, new() { Name = requestedActivities ? "Activiteiten" : "Algemeen", Exact = true })
                .GetByRole(AriaRole.Treeitem, new() { Name = "Startpagina", Exact = true }))
                .ToHaveAttributeAsync("aria-selected", "true");
        }
        if (requestedActivities)
        {
            await steps.OpenInNavigation("Leaseoffertes");
            await steps.TheLabelIsPresent("Leaseoffertes");
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task CloseGroupAndOpenMenuPath(bool activitiesExpanded)
    {
        await PrepareModuleAsync(activitiesExpanded, false);
        var activePage = new ActivePage(Page);
        var steps = new StepDefinitionsCommon(activePage);
        await steps.NavigateInModules("LeaseWise");

        ILocator algemeen = activePage.Page.GetByRole(AriaRole.Treeitem, new() { Name = "Algemeen", Exact = true });
        await steps.CloseWindow("Algemeen");
        await Expect(algemeen).ToHaveAttributeAsync("aria-expanded", "false");
        await steps.CloseWindow("Algemeen");
        await Expect(algemeen).ToHaveAttributeAsync("aria-expanded", "false");

        await steps.OpenInNavigation("Activiteiten -> Leaseoffertes");
        await Expect(activePage.Page.GetByRole(AriaRole.Treeitem, new() { Name = "Activiteiten", Exact = true }))
            .ToHaveAttributeAsync("aria-expanded", "true");
        await steps.TheLabelIsPresent("Leaseoffertes");
    }
}
