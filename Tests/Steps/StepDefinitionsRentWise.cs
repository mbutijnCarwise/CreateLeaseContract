using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;
using System.Globalization;

namespace CreateLeaseContract.Tests.Steps;

[Binding]
public sealed class StepDefinitionsRentWise 
{
    private const string ScreenshotDirectory = @"C:\Users\m.butijn\.copilot\session-state\fc325177-a56b-4cee-a3e4-d001b56f66ed\files";
    private readonly IPage _page;
    private Quotation? _quotation;

    public StepDefinitionsRentWise(IPage page)
    {
        _page = page;
    }

    [Given("I have navigated to (.*) in the sub menu")]
    public async Task OpenOvereenkomstenInNavigation(string subItem)
    {
        await _page.GetByRole(AriaRole.Link, new() { Name = "RentWise" }).ClickAsync();
        await OpenInNavigation("Verhuuractiviteiten");
        await _page.GetByRole(AriaRole.Button, new() { Name = subItem, Exact = true }).ClickAsync();
    }

    [When("I open (.*) in the main menu")]
    public async Task OpenInNavigation(string item)
    {
        await _page.GetByText(item).ClickAsync();
    }

    [When("I click on button (.*)")]
    public async Task ClickOnToevoegen(string buttonName)
    {
        if (buttonName.Equals("OK", StringComparison.OrdinalIgnoreCase))
        {
            await CaptureScreenshotAsync("quotation-before-ok.png");
        }

        await _page.GetByRole(AriaRole.Button, new() { Name = buttonName, Exact = true }).ClickAsync();
    }

    [When("I confirm the tariff message if it appears")]
    public async Task ConfirmTariffMessageIfItAppears()
    {
        await CaptureScreenshotAsync("quotation-before-optional-confirmation.png");

        ILocator okButton = _page.GetByRole(AriaRole.Dialog)
            .GetByRole(AriaRole.Button, new() { Name = "OK", Exact = true });

        try
        {
            await okButton.WaitForAsync(new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = 2000
            });
        }
        catch (TimeoutException)
        {
            TestContext.Out.WriteLine("No visible OK button was found in a dialog.");
            await CaptureScreenshotAsync("quotation-after-optional-confirmation.png");
            return;
        }

        await okButton.ClickAsync();
        await CaptureScreenshotAsync("quotation-after-optional-confirmation.png");
    }

    private async Task CaptureScreenshotAsync(string fileName)
    {
        string path = Path.Combine(ScreenshotDirectory, fileName);
        await _page.ScreenshotAsync(new() { Path = path, FullPage = true });
        TestContext.Out.WriteLine($"Screenshot saved to {path}");
    }

    // [When("I fill in Huurder with (.*)")]
    // public async Task FillInHuurder(string huurder)
    // {
    //     ILocator huurderField = _page.GetByLabel("Huurder", new() { Exact = true });
    //     await huurderField.FillAsync(huurder);
    //     await huurderField.PressAsync("Tab");
    // }

    [When("I fill in these remaining fields:")]
    public async Task FillInTheseFields(Table table)
    {
        Assert.That(table.Rows, Has.Count.EqualTo(1), "Expected one quotation data row.");
        var columns = table.Rows[0];

        Quotation quotation = new(
            columns["Huurder"],
            columns["Datum opvolging"],
            columns["Datum/tijd vertrek"],
            columns["Datum/tijd retour"],
            columns["Categorie"],
            columns["Kenteken"],
            int.Parse(columns["Te rijden kilometers"], CultureInfo.InvariantCulture),
            decimal.Parse(columns["Aanbetaling"], CultureInfo.InvariantCulture),
            decimal.Parse(columns["Waarborg"], CultureInfo.InvariantCulture),
            columns["Opmerking"]
        );

        _quotation = quotation;

        (string Label, string Value)[] fields =
        {
            ("Huurder", quotation.Huurder),
            ("Datum opvolging", quotation.DatumOpvolging),
            ("Datum/tijd vertrek", quotation.DatumTijdVertrek),
            ("Datum/tijd retour", quotation.DatumTijdRetour),
            ("Categorie", quotation.Categorie),
            ("Kenteken", quotation.Kenteken),
            ("Te rijden kilometers", quotation.TeRijdenKilometers.ToString(CultureInfo.InvariantCulture)),
            ("Aanbetaling", quotation.Aanbetaling.ToString(CultureInfo.InvariantCulture)),
            ("Waarborg", quotation.Waarborg.ToString(CultureInfo.InvariantCulture)),
            ("Opmerking", quotation.Opmerking)
        };

        foreach ((string label, string value) in fields)
        {
            ILocator field = _page.GetByLabel(label, new() { Exact = true });
            await field.FillAsync(value);
            await field.PressAsync("Tab");
        }
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

    [Then("the following items are shown in the sub menu:")]
    public async Task TheFollowingItemsAreShownInTheMainMenu(Table table)
    {
        ILocator menu = _page.GetByRole(AriaRole.Navigation, new() { Name = "Verhuuractiviteiten" });
        foreach (var row in table.Rows)
        {
            string item = row[0];
            await menu.GetByText(item, new() { Exact = true })
                .WaitForAsync(new() { State = WaitForSelectorState.Visible });
        }
    }

    [Then("the label (.*) is present")]
    public async Task TheLabelIsPresent(string label)
    {
        ILocator title = _page.GetByRole(AriaRole.Heading, new() { Name = label, Exact = true });
        await title.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        string actualTitle = (await title.InnerTextAsync()).Trim();
        StringAssert.Contains(label, actualTitle);
    }

    [Then("the new added quotation is present")]
    public async Task TheNewAddedQuotationIsPresent()
    {
        Quotation quotation = _quotation ?? throw new InvalidOperationException("Quotation data has not been entered.");
        ILocator kentekenCell = _page.GetByRole(AriaRole.Cell, new() { Name = quotation.Kenteken, Exact = true });

        await kentekenCell.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        string savedKenteken = (await kentekenCell.InnerTextAsync()).Trim();
        Assert.That(savedKenteken, Is.EqualTo(quotation.Kenteken));
    }

}