using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;

namespace CreateLeaseContract.Tests.Steps;

[Binding]
public class StepDefinitionsCommon
{
    private static readonly string BaseUrl = "https://localhost:7095/login?returnUrl=%2F";
    protected readonly ActivePage _activePage;
    private IPage _page => _activePage.Page;
    
    public StepDefinitionsCommon(ActivePage page)
    {
        _activePage = page;
    }

    [Given("CW2 is started and logged in as user (.*) in administration (.*)")]
    public async Task NavigateToWebsite(string user, string administration)
    {
        IResponse? _response = await _page.GotoAsync(BaseUrl);
        Assert.That(_response?.Ok, Is.True);

        await _page.GetByLabel("Gebruikerscode").FillAsync(user);
        await _page.GetByLabel("Wachtwoord").FillAsync(user);

        await _page.GetByRole(AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();
    }

    [When(@"I navigate to (.*) in the modules")]
    public async Task NavigateInModules(string moduleName)
    {
        string? navigationGroup = null;
        if (moduleName.EndsWith(" Activiteiten", StringComparison.Ordinal))
        {
            navigationGroup = "Activiteiten";
            moduleName = moduleName[..^" Activiteiten".Length];
        }
        else if (moduleName.EndsWith(" Algemeen", StringComparison.Ordinal))
        {
            navigationGroup = "Algemeen";
            moduleName = moduleName[..^" Algemeen".Length];
        }

        _activePage.Page = await _page.RunAndWaitForPopupAsync(() => _page.GetByRole(AriaRole.Link, new() { Name = moduleName }).ClickAsync());
        if (navigationGroup is not null)
            await NavigationActions.OpenGroupAsync(_page, navigationGroup);
    }

    [When("I close (.*)")]
    public Task CloseWindow(string groupName) =>
        NavigationActions.CloseInNavigation(_page, groupName);

    [When("I open (.*) in the main menu")]
    public Task OpenInNavigation(string item) =>
        NavigationActions.OpenInNavigation(_page, item);

    [Then("the label (.*) is present")]
    public async Task TheLabelIsPresent(string label)
    {
        ILocator title = _page.GetByRole(AriaRole.Heading, new() { Name = label, Exact = true });
        await title.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        string actualTitle = (await title.InnerTextAsync()).Trim();
        StringAssert.Contains(label, actualTitle);
    }

    [Then(@"the window heading (.*) is shown")]
    public async Task TheWindowIsShown(string windowName)
    {
        // Web-first assertion: retries until the (new) page heading contains the text,
        // avoiding races where the previous page's heading is still visible.
        ILocator heading = _page.Locator("h1:visible").First;
        await Assertions.Expect(heading).ToContainTextAsync(windowName.Trim());
    }

}
