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

    [When("I navigate to (.*) Activiteiten in the modules")]
    public async Task NavigateInModules(string button)
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = button }).ClickAsync();
    }

}
