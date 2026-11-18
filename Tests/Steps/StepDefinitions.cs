using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;

namespace CreateLeaseContract.Tests.Steps;

[Binding]
public sealed class StepDefinitions
{
    private static readonly string BaseUrl = "https://localhost:7095/login?returnUrl=%2F";
    private readonly IPage _page;
    private IResponse? _response;
    
    public StepDefinitions(IPage page)
    {
        _page = page;
    }

    // Given steps go here
    [Given("CW2 is started and logged in as user (.*) in administration (.*)")]
    public async Task NavigateToWebsite(string user, string administration)
    {
        _response = await _page.GotoAsync(BaseUrl);
        await _page.GetByLabel("Gebruikerscode").FillAsync(user);
        await _page.GetByLabel("Wachtwoord").FillAsync(user);

        await _page.GetByRole(AriaRole.Button, new() { Name = "Inloggen" }).ClickAsync();
    }

    [When("I navigate to RentWise Activiteiten")]
    public async Task ClickActiviteitenInRibbonGroupRentWise()
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "RentWise" }).ClickAsync();
    }

    [When("I open (.*) in the navigation")]
    public async Task OpenInNavigation(string item)
    {
        await _page.GetByText(item).ClickAsync();
    }

    [When("I go to Overeenkomsten in the navigation")]
    public async Task OpenOvereenkomstenInNavigation()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "Overeenkomsten", Exact = true }).ClickAsync();
    }

    [When("I double-click a randomly selected vehicle agreement that hadn't been digitally signed")]
    public async Task DoubleClickRandomUnsignedVehicleAgreement()
    {
        for (int i = 2; i < 10; i++) // todo: replace 10 with the actual number of rows in the table
        {
            ILocator agreement = _page.Locator($"xpath=//html/body/div[1]/div/main/div/dxbl-drawer/div/div/div/dxbl-tabs/div/div[2]/div/div/section/div/div/dxbl-grid/dxbl-virtual-scroll-viewer/div/table/tbody/tr[{i}]/td[5]");
            string? digitallySigned = await agreement.TextContentAsync();
            TestContext.Out.WriteLine($"Text content of agreement at row {i-1}: {digitallySigned}");
            
            if (string.IsNullOrEmpty(digitallySigned))
            {
                TestContext.WriteLine($"Found unsigned agreement at row {i-1}");
                await agreement.DblClickAsync();
                break;
            }
        }

    }

}
