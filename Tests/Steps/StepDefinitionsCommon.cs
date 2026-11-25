using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;

namespace CreateLeaseContract.Tests.Steps;

[Binding]
public class StepDefinitionsCommon
{
    private static readonly string BaseUrl = "https://localhost:7095/login?returnUrl=%2F";
    protected readonly IPage _page;
    
    public StepDefinitionsCommon(IPage page)
    {
        _page = page;
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
        if (moduleName.EndsWith(" Activiteiten", StringComparison.Ordinal))
            moduleName = moduleName[..^" Activiteiten".Length];
        else if (moduleName.EndsWith(" Algemeen", StringComparison.Ordinal))
            moduleName = moduleName[..^" Algemeen".Length];

        await _page.GetByRole(AriaRole.Link, new() { Name = moduleName }).ClickAsync();
    }

    [When("I open (.*) in the main menu")]
    public Task OpenInNavigation(string item) =>
        NavigationActions.OpenInNavigation(_page, item);

    [Then(@"the window (.*) is shown")]
    public async Task TheWindowIsShown(string windowName)
    {
        // Web-first assertion: retries until the (new) page heading contains the text,
        // avoiding races where the previous page's heading is still visible.
        ILocator heading = _page.Locator("h1:visible").First;
        await Assertions.Expect(heading).ToContainTextAsync(windowName.Trim());
    }

}
