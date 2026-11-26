using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CreateLeaseContract.Tests.Steps;

[Binding]
public sealed class StepDefinitionsRentWise 
{
    protected readonly ActivePage _activePage;
    private IPage _page => _activePage.Page;
    private Quotation _quotation = null!;
    private Reservation _reservation = null!;

    public StepDefinitionsRentWise(ActivePage page)
    {
        _activePage = page;
    }

    [Given("I have navigated to (.*) in the sub menu of Verhuuractiviteiten")]
    public async Task OpenOvereenkomstenInNavigation(string subItem)
    {
        _activePage.Page = await _page.RunAndWaitForPopupAsync(() => _page.GetByRole(AriaRole.Link, new() { Name = "RentWise" }).ClickAsync());

        await NavigationActions.OpenInNavigation(_page, "Verhuuractiviteiten");
        await _page.GetByRole(AriaRole.Button, new() { Name = subItem, Exact = true }).First.ClickAsync();
    }

    [When("I click on button (.*)")]
    public async Task ClickOnToevoegen(string buttonName)
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = buttonName, Exact = true }).First.ClickAsync();
    }

    [When("I make a new reservation")]
    public async Task MakeNewReservation()
    {
        await _page.GetByRole(AriaRole.Button, new() { Name = "Nieuwe reservering", Exact = true }).First.ClickAsync();
    }

    [When("I click on tab (.*)")]
    public async Task ClickOnTabHuurderDebiteur(string tabName)
    {
        await _page.GetByRole(AriaRole.Tab, new() { Name = tabName }).First.ClickAsync();
    }

    /// <summary>
    /// Closes every DevExpress message box (e.g. "Tarief: Geen geldig tarief gevonden") that appears within
    /// <paramref name="timeoutMs"/>. The application can show only one at a time: when a second one is requested
    /// while the first is open, it is never shown and the server waits for it forever, which later blocks Opslaan.
    /// So call this after every field that can trigger such a message, before touching the next one.
    /// The message normally appears within 250 ms, so <paramref name="timeoutMs"/> leaves about twice that as margin.
    /// </summary>
    private async Task CloseMessageDialogsAsync(int timeoutMs = 500)
    {
        ILocator okButton = _page.Locator("dxbl-modal-dialog").GetByRole(AriaRole.Button, new() { Name = "OK", Exact = true });

        while (true)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await okButton.First.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
            }
            catch (TimeoutException)
            {
                return;
            }

            TestContext.Out.WriteLine($"Closing message after {stopwatch.ElapsedMilliseconds} ms: {(await _page.Locator("dxbl-modal-dialog").First.InnerTextAsync()).Trim()}");
            await okButton.First.ClickAsync();
            await okButton.First.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        }
    }

    [When("I fill in these remaining fields:")] // todo: specify in binding that this method is only for the quotation fields
    public async Task FillInTheseFields(Table table)
    {
        Assert.That(table.Rows, Has.Count.EqualTo(1), "Expected one quotation data row.");
        var columns = table.Rows[0];

        _quotation = new(
            columns["Huurder"],
            columns["Datum opvolging"],
            columns["Datum/tijd vertrek"],
            columns["Datum/tijd retour"],
            columns["Categorie"],
            columns["Kenteken"],
            int.Parse(columns["Te rijden kilometers"], CultureInfo.InvariantCulture),
            decimal.Parse(columns["Aanbetaling"], CultureInfo.InvariantCulture),
            decimal.Parse(columns["Waarborg"], CultureInfo.InvariantCulture),
            columns["Opmerking"],
            columns["Soort"],
            columns["Eenheid"]);

        (string Label, string Value)[] fields =
        {
            ("Huurder", _quotation.Huurder),
            ("Datum opvolging", _quotation.DatumOpvolging),
            ("Datum/tijd vertrek", _quotation.DatumTijdVertrek),
            ("Datum/tijd retour", _quotation.DatumTijdRetour),
            ("Categorie", _quotation.Categorie),
            ("Kenteken", _quotation.Kenteken),
            ("Te rijden kilometers", _quotation.TeRijdenKilometers.ToString(CultureInfo.InvariantCulture)),
            ("Aanbetaling", _quotation.Aanbetaling.ToString(CultureInfo.InvariantCulture)),
            ("Waarborg", _quotation.Waarborg.ToString(CultureInfo.InvariantCulture)),
            ("Opmerking", _quotation.Opmerking),
            ("Soort", _quotation.Soort),
            ("Eenheid", _quotation.Eenheid)};

        foreach ((string label, string value) in fields)
        {
            ILocator field = _page.GetByLabel(label, new() { Exact = true });
            bool mayShowMessage = TariffFields.Contains(label);
            if (DropdownFields.Contains(label))
            {
                // Soort and Eenheid exist for both Basis and Verlenging; the first one is Basis.
                await field.First.ClickAsync();
                await _page.GetByRole(AriaRole.Option, new() { Name = value, Exact = true }).ClickAsync();
                await CloseMessageDialogsAsync();
            }
            else if (label.StartsWith("Datum/tijd"))
            {
                // The label only points at the date input; the time is a separate input next to it.
                (string date, string time) = SplitDateTime(value);
                await FillAndLeaveAsync(field, date, mayShowMessage);
                await FillAndLeaveAsync(TimeInputOf(field), time, mayShowMessage);
            }
            else
            {
                await FillAndLeaveAsync(field, value, mayShowMessage);
            }
        }
    }

    [When("I fill in these fields in the Voorwaarden tab:")]
    public async Task FillInFieldsInVoorwaardenTab(Table table)
    {
        Assert.That(table.Rows, Has.Count.EqualTo(1), "Expected one Voorwaarden data row.");
        var row = table.Rows[0];

        ILocator soort = _page.GetByLabel("Basis Soort", new() { Exact = true });
        await soort.ClickAsync();
        await FillAndLeaveAsync(soort, row["Soort"], true);
        _reservation.Soort = row["Soort"];

        ILocator eenheid = _page.GetByLabel("Basis Eenheid", new() { Exact = true });
        await eenheid.ClickAsync();
        await FillAndLeaveAsync(eenheid, row["Eenheid"], false);
        _reservation.Eenheid = row["Eenheid"];
    }

    [When("I select the first vehicle")]
    public async Task SelectFirstVehicle()
    {
        ILocator availableCars = _page.GetByRole(AriaRole.Region, new() { Name = "Beschikbare auto's", Exact = true });
        ILocator firstVehicle = availableCars.GetByRole(AriaRole.Treegrid)
            .Locator("tbody tr[role='row'][data-visible-index]").First;

        await firstVehicle.Locator("td[data-field='kenteken']").ClickAsync();
        await Assertions.Expect(availableCars.GetByRole(AriaRole.Button, new() { Name = "Nieuwe reservering", Exact = true }))
            .ToBeEnabledAsync();

        async Task<string> VehicleValueAsync(string field)
        {
            ILocator cell = firstVehicle.Locator($"td[data-field='{field}']");
            await Assertions.Expect(cell).Not.ToBeEmptyAsync();
            return (await cell.InnerTextAsync()).Trim();
        }

        string categorieText = await VehicleValueAsync("categorieId");
        if (!Enum.TryParse(categorieText, ignoreCase: true, out Categorie categorie)
            || !string.Equals(categorie.ToString(), categorieText, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Unknown vehicle category: '{categorieText}'.");

        string kenteken = await VehicleValueAsync("kenteken");
        string autoOmschrijving = await VehicleValueAsync("cOmschrijving");
        string fuelText = await VehicleValueAsync("brandstofSoort");
        if (!Enum.TryParse(fuelText, ignoreCase: true, out Brandstof brandstof) || !Enum.IsDefined(brandstof))
            throw new InvalidOperationException($"Unknown vehicle fuel type: '{fuelText}'.");

        _reservation = new Reservation(categorie, kenteken, autoOmschrijving, brandstof);
    }

    [When("I fill in the Huurder field with (.*)")]
    public async Task FillInHuurderField(string huurder)
    {
        IList<ILocator> huurderFields = (IList<ILocator>) await _page.GetByLabel("Huurder", new() { Exact = true }).AllAsync();
        await FillAndLeaveAsync(huurderFields[1], huurder, true);
    }

    // Fields whose change makes the application determine the tariff again (PagePrijsberekeningWaarden),
    // which can show a "Tarief" message. Other fields need no wait for a message.
    private static readonly HashSet<string> TariffFields = ["Huurder", "Datum/tijd vertrek", "Datum/tijd retour", "Categorie", "Kenteken", "Soort", "Eenheid"];

    private static readonly HashSet<string> DropdownFields = ["Soort", "Eenheid"];

    private async Task FillAndLeaveAsync(ILocator field, string value, bool mayShowMessage)
    {
        await field.FillAsync(value);
        await field.PressAsync("Tab");
        if (mayShowMessage)
            await CloseMessageDialogsAsync();
    }

    private static ILocator TimeInputOf(ILocator dateField) =>
        dateField.Locator("xpath=../..").Locator("input").Nth(1);

    private static (string Date, string Time) SplitDateTime(string value)
    {
        string[] parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Assert.That(parts, Has.Length.EqualTo(2), $"Expected 'dd-MM-yyyy HH:mm' but got '{value}'.");
        return (parts[0], parts[1]);
    }

    [When("I double-click the top digitally unsigned vehicle agreement")]
    public async Task DoubleClickTopUnsignedAgreement()
    {
        ILocator grid = _page.Locator("dxbl-grid").Filter(new()
        {
            Has = _page.GetByRole(AriaRole.Columnheader, new() { Name = "Digitaal ondertekenen", Exact = true })
        });
        int nummerIndex = await ColumnIndexAsync(grid, "Nummer");
        int signedIndex = await ColumnIndexAsync(grid, "Digitaal ondertekenen");

        // The grid loads its rows asynchronously; wait until real data (an agreement number) is shown.
        ILocator nummerCells = grid.Locator($"tbody tr > td:nth-child({nummerIndex + 1})");
        await nummerCells.Filter(new() { HasTextRegex = new Regex(@"^\s*\d+\s*$") }).First
            .WaitForAsync(new() { State = WaitForSelectorState.Visible });

        IReadOnlyList<ILocator> rows = await grid.Locator("tbody tr").AllAsync();
        foreach (ILocator row in rows)
        {
            ILocator cells = row.Locator("td");
            if (await cells.CountAsync() <= signedIndex)
                continue;

            string nummer = (await cells.Nth(nummerIndex).InnerTextAsync()).Trim();
            string signed = (await cells.Nth(signedIndex).InnerTextAsync()).Trim();
            if (nummer.Length == 0 || signed.Length != 0)
                continue;

            TestContext.Out.WriteLine($"Opening unsigned agreement {nummer}");
            await cells.Nth(signedIndex).DblClickAsync();
            return;
        }

        Assert.Fail("No digitally unsigned agreement found in the loaded rows of Overeenkomsten.");
    }

    private static async Task<int> ColumnIndexAsync(ILocator grid, string header)
    {
        ILocator columnHeader = grid.GetByRole(AriaRole.Columnheader, new() { Name = header, Exact = true }).First;
        return await columnHeader.EvaluateAsync<int>("th => th.cellIndex");
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

    [Then("the new quotation has been given a number")]
    public async Task TheNewQuotationHasBeenGivenANumber()
    {
        // After the first Opslaan the tab is renamed from "Toevoegen offerte" to "Wijzigen offerte {nummer}".
        ILocator title = _page.GetByRole(AriaRole.Heading, new() { NameRegex = new Regex(@"^Wijzigen offerte \d+$") });
        await title.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        string text = (await title.InnerTextAsync()).Trim();
        _quotation.Nummer = int.Parse(text["Wijzigen offerte ".Length..], CultureInfo.InvariantCulture);
        TestContext.Out.WriteLine($"New quotation number: {_quotation.Nummer}");
    }

    [Then("the new added quotation is present")]
    public async Task TheNewAddedQuotationIsPresent()
    {
        ILocator row = await FindNewQuotationRowAsync();

        async Task<string> CellOf(string header)
        {
            ILocator columnHeader = _page.GetByRole(AriaRole.Columnheader, new() { Name = header, Exact = true });
            Assert.That(await columnHeader.CountAsync(), Is.EqualTo(1), $"Column {header} not found in the Offertes list.");
            // The grid has helper columns, so map the header to the cell by its DOM position.
            int cellIndex = await columnHeader.EvaluateAsync<int>("th => th.cellIndex");
            return (await row.Locator("td").Nth(cellIndex).InnerTextAsync()).Trim();
        }

        string nummer = await CellOf("Nummer");
        string categorie = await CellOf("Categorie");
        string kenteken = await CellOf("Kenteken");
        string huurder = await CellOf("Huurder");
        string vertrek = await CellOf("Vertrek");
        TestContext.Out.WriteLine($"List row: {nummer} | {categorie} | {kenteken} | {huurder} | {vertrek}");

        Assert.Multiple(() =>
        {
            Assert.That(nummer, Is.EqualTo(_quotation.Nummer!.Value.ToString(CultureInfo.InvariantCulture)), "Nummer");
            Assert.That(categorie, Is.EqualTo(_quotation.Categorie), "Categorie");
            Assert.That(kenteken, Is.EqualTo(_quotation.Kenteken), "Kenteken");
            // The list shows the renter's name (e.g. "mbu_debiteur"), not the code that was typed.
            Assert.That(huurder, Does.Contain(_quotation.Huurder).IgnoreCase, "Huurder");
            // The list shows only the departure date, without the time.
            Assert.That(vertrek, Is.EqualTo(SplitDateTime(_quotation.DatumTijdVertrek).Date), "Vertrek");
        });
    }

    [Then("this new reservation should be vissible in the table")]
    public async Task ThenThisNewReservationShouldBeVissibleInTheTable()
    {
        ILocator row = await FindNewReservationRowAsync();
        Assert.That(await row.CountAsync(), Is.EqualTo(1), "The new reservation row is not visible in the table.");
    }

    /// <summary>Filters the Offertes grid on the number of the new quotation and returns its row.</summary>
    private async Task<ILocator> FindNewQuotationRowAsync()
    {
        Assert.That(_quotation.Nummer, Is.Not.Null, "The quotation number was not read after the first save.");
        string nummer = _quotation.Nummer!.Value.ToString(CultureInfo.InvariantCulture);

        ILocator filter = _page.GetByLabel("Filter Nummer", new() { Exact = true });
        await filter.FillAsync(nummer);
        await filter.PressAsync("Enter");

        ILocator row = _page.GetByRole(AriaRole.Row)
            .Filter(new() { Has = _page.GetByRole(AriaRole.Gridcell, new() { Name = nummer, Exact = true }) })
            .Filter(new() { Has = _page.GetByRole(AriaRole.Gridcell, new() { Name = _quotation.Kenteken, Exact = true }) });
        await row.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        return row;
    }

    private async Task<ILocator> FindNewReservationRowAsync()
    {
        Assert.That(_reservation.Nummer, Is.Not.Null, "The reservation number was not read after the first save.");
        string nummer = _reservation.Nummer;

        ILocator filter = _page.GetByLabel("Filter Nummer", new() { Exact = true });
        await filter.FillAsync(nummer);
        await filter.PressAsync("Enter");

        ILocator row = _page.GetByRole(AriaRole.Row)
            .Filter(new() { Has = _page.GetByRole(AriaRole.Gridcell, new() { Name = nummer, Exact = true }) })
            .Filter(new() { Has = _page.GetByRole(AriaRole.Gridcell, new() { Name = _reservation.Kenteken, Exact = true }) });
        await row.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        return row;
    }

}
