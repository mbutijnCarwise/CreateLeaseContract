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
    private string _huurder = "mbu_debiteur";

    public StepDefinitionsRentWise(ActivePage page)
    {
        _activePage = page;
    }

    [Given("I have navigated to (.*) in the sub menu of Verhuuractiviteiten")]
    public async Task OpenOvereenkomstenInNavigation(string subItem)
    {
        _activePage.Page = await _page.RunAndWaitForPopupAsync(() => _page.GetByRole(AriaRole.Link, new() { Name = "RentWise" }).ClickAsync());

        // Tariff messages ("Tariefsoort" question, "Tarief: Geen geldig tarief gevonden") can pop up at unpredictable
        // moments and block every further click. Playwright runs this handler before an action whenever one is visible.
        Regex answer = new("^(Ja|Yes|OK)$");
        ILocator tariefDialog = _page.Locator("dxbl-modal-dialog")
            .Filter(new() { HasTextRegex = new Regex("tarief", RegexOptions.IgnoreCase) })
            .Filter(new() { Has = _page.GetByRole(AriaRole.Button, new() { NameRegex = answer }) });
        await _page.AddLocatorHandlerAsync(tariefDialog.First, async dialog =>
        {
            ILocator button = dialog.GetByRole(AriaRole.Button, new() { NameRegex = answer }).First;
            TestContext.Out.WriteLine($"Answering {(await button.InnerTextAsync()).Trim()}: {(await dialog.InnerTextAsync()).Trim()}");
            await button.ClickAsync();
        }, new() { NoWaitAfter = true });

        // Right after the RentWise tab opens, the server may not handle the menu click yet, so the click can be lost.
        // Click again until the sub menu item appears.
        ILocator menuItem = _page.GetByRole(AriaRole.Navigation).GetByText("Verhuuractiviteiten", new() { Exact = true }).First;
        ILocator subMenuItem = _page.GetByRole(AriaRole.Button, new() { Name = subItem, Exact = true }).First;
        const int maxAttempts = 5;
        for (int attempt = 1; ; attempt++)
        {
            try
            {
                await menuItem.ClickAsync(new() { Timeout = 10000 });
                await Assertions.Expect(subMenuItem).ToBeVisibleAsync(new() { Timeout = 5000 });
                break;
            }
            catch (Exception ex) when (ex is PlaywrightException or TimeoutException)
            {
                if (attempt == maxAttempts)
                {
                    string screenshot = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"navigation-stuck-{DateTime.Now:HHmmss}.png");
                    await _page.ScreenshotAsync(new() { Path = screenshot, FullPage = true });
                    TestContext.AddTestAttachment(screenshot, "Page when the sub menu item did not appear");
                    TestContext.Out.WriteLine($"Screenshot: {screenshot}");
                    TestContext.Out.WriteLine(await _page.Locator("body").AriaSnapshotAsync());
                    throw;
                }
                TestContext.Out.WriteLine($"Sub menu item '{subItem}' not shown after opening Verhuuractiviteiten (attempt {attempt}); trying again.");
            }
        }
        await subMenuItem.ClickAsync();
    }

    [When("I click on button (.*)")]
    public async Task ClickOnToevoegen(string buttonName)
    {
        // Use the visible button: hidden copies (e.g. on tabs that are open in the background) can come first.
        await _page.GetByRole(AriaRole.Button, new() { Name = buttonName, Exact = true })
            .Filter(new() { Visible = true }).First.ClickAsync();
    }

    [When("I click on tab (.*)")]
    public async Task ClickOnTabHuurderDebiteur(string tabName)
    {
        await _page.GetByRole(AriaRole.Tab, new() { Name = tabName }).First.ClickAsync();
    }

    /// <summary>
    /// Lets the registered tariff handler close a pending message before the next field is changed.
    /// </summary>
    private async Task CloseMessageDialogsAsync(int timeoutMs = 500)
    {
        ILocator dialog = _page.Locator("dxbl-modal-dialog")
            .Filter(new() { HasTextRegex = new Regex("tarief", RegexOptions.IgnoreCase) }).First;
        try
        {
            await dialog.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            return;
        }
        await Assertions.Expect(dialog).ToBeHiddenAsync();
    }

    [When("I fill in these remaining quotation fields:")]
    public async Task FillInTheseFields(Table table)
    {
        Assert.That(table.Rows, Has.Count.EqualTo(1), "Expected one quotation data row.");
        var columns = table.Rows[0];

        _quotation = new(
            columns["Huurder"],
            columns["Datum opvolging"],
            columns["Datum/tijd vertrek"],
            columns["Datum/tijd retour"],
            Enum.Parse<Categorie>(columns["Categorie"], true),
            "",
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
            ("Categorie", _quotation.Categorie.ToString()),
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

        ILocator kentekenField = _page.GetByLabel("Kenteken", new() { Exact = true })
            .Filter(new() { Visible = true }).First;
        _quotation.Kenteken = await SelectFirstKentekenOptionAsync(kentekenField);
    }

    private async Task<string> SelectFirstKentekenOptionAsync(ILocator field)
    {
        await field.FillAsync("");
        if (await field.GetAttributeAsync("aria-expanded") != "true")
        {
            await field.Locator("xpath=ancestor::dxbl-combo-box[1]")
                .GetByRole(AriaRole.Button, new() { Name = "Keuzelijst openen of sluiten", Exact = true })
                .ClickAsync();
        }
        await Assertions.Expect(field).ToHaveAttributeAsync("aria-expanded", "true");

        string? dropdownId = await field.GetAttributeAsync("aria-controls");
        Assert.That(dropdownId, Is.Not.Null.And.Not.Empty, "Kenteken must reference its dropdown.");
        ILocator option = _page.Locator($"[id='{dropdownId}']")
            .GetByRole(AriaRole.Option, new() { NameRegex = KentekenOptionPattern })
            .Filter(new() { Visible = true }).First;
        await option.WaitForAsync(new() { State = WaitForSelectorState.Visible });

        string kenteken = (await option.InnerTextAsync()).Trim();
        await option.ClickAsync();
        await field.PressAsync("Tab");
        await CloseMessageDialogsAsync();
        await Assertions.Expect(field).ToHaveValueAsync(kenteken);
        return kenteken;
    }

    private static readonly Regex KentekenOptionPattern = new(
        @"^(?=[A-Z0-9-]*[A-Z])(?=[A-Z0-9-]*[0-9])[A-Z0-9]+(?:-[A-Z0-9]+)+$");

    [When("I fill in these fields in the Voorwaarden tab:")]
    public async Task FillInFieldsInVoorwaardenTab(Table table)
    {
        Assert.That(table.Rows, Has.Count.EqualTo(1), "Expected one Voorwaarden data row.");
        var row = table.Rows[0];

        // The inputs have an aria-label that includes the section ("Basis Soort", "Verlenging Soort"),
        // which overrides the visible "Soort"/"Eenheid" label.
        ILocator soort = _page.GetByLabel("Basis Soort", new() { Exact = true });
        await SelectLookupValueAsync(_page, "Basis Soort", row["Soort"]);
        await CloseMessageDialogsAsync();

        ILocator eenheid = _page.GetByLabel("Basis Eenheid", new() { Exact = true });
        string selectedEenheid = await SelectLookupValueAsync(_page, "Basis Eenheid");
        await CloseMessageDialogsAsync();

        await Assertions.Expect(soort).ToHaveValueAsync(row["Soort"]);
        await Assertions.Expect(eenheid).ToHaveValueAsync(selectedEenheid);
        _reservation.Soort = row["Soort"];
        _reservation.Eenheid = selectedEenheid;
    }

    internal static async Task<string> SelectLookupValueAsync(IPage page, string label, string? value = null)
    {
        ILocator field = page.GetByLabel(label, new() { Exact = true }).Filter(new() { Visible = true });
        await field.FillAsync("");
        ILocator button = field.Locator("xpath=ancestor::dxbl-input-editor[1]")
            .GetByRole(AriaRole.Button, new() { Name = $"Zoeken {label}", Exact = true });
        string? listId = await button.GetAttributeAsync("aria-controls");
        Assert.That(listId, Is.Not.Null.And.Not.Empty, $"{label} must reference its lookup list.");
        await button.ClickAsync();

        ILocator list = page.Locator($"[id='{listId}']");
        ILocator codeCell = value is not null
            ? list.GetByRole(AriaRole.Gridcell, new() { Name = value, Exact = true })
            : list.GetByRole(AriaRole.Row).Locator("td:first-child")
                .Filter(new() { HasTextRegex = new Regex(@"^\s*(?!\?\s*$)\S+\s*$") }).First;
        await Assertions.Expect(codeCell).ToBeVisibleAsync();
        string selectedValue = (await codeCell.InnerTextAsync()).Trim();
        string description = (await codeCell.Locator("xpath=ancestor::tr[1]")
            .GetByRole(AriaRole.Gridcell).Nth(1).InnerTextAsync()).Trim();
        await codeCell.DblClickAsync();
        await list.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        await Assertions.Expect(field).ToHaveValueAsync(selectedValue);

        string? descriptionId = await field.GetAttributeAsync("aria-describedby");
        Assert.That(descriptionId, Is.Not.Null.And.Not.Empty, $"{label} must reference its resolved description.");
        await Assertions.Expect(page.Locator($"[id='{descriptionId}']")).ToHaveTextAsync(description);
        TestContext.Out.WriteLine($"Resolved {label}: {selectedValue} ({description})");
        return selectedValue;
    }

    [When("I select the first vehicle")]
    public async Task SelectFirstVehicle()
    {
        ILocator availableCars = _page.GetByRole(AriaRole.Region, new() { Name = "Beschikbare auto's", Exact = true });
        ILocator firstVehicle = availableCars.GetByRole(AriaRole.Treegrid)
            .Locator("tbody tr[role='row'][data-visible-index]").First;

        ILocator nieuweReservering = availableCars.GetByRole(AriaRole.Button, new() { Name = "Nieuwe reservering", Exact = true });

        // The button is only enabled once the server has handled the row click. When the server is busy
        // (e.g. still loading the grid) the click can be lost, so click again until the button is enabled.
        const int maxAttempts = 5;
        for (int attempt = 1; ; attempt++)
        {
            await firstVehicle.Locator("td[data-field='kenteken']").ClickAsync();
            try
            {
                await Assertions.Expect(nieuweReservering).ToBeEnabledAsync(new() { Timeout = 3000 });
                break;
            }
            catch (PlaywrightException) when (attempt < maxAttempts)
            {
                TestContext.Out.WriteLine($"'Nieuwe reservering' not enabled after selecting the first vehicle (attempt {attempt}); clicking again.");
            }
        }

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

        _reservation = new Reservation("mbu_debiteur", categorie, kenteken, autoOmschrijving, brandstof);
    }

    [When("I fill in the (.*) field with (.*)")]
    public async Task FillInField(string field, string value)
    {
        // Some labels (e.g. Huurder) exist on several tabs; only the one on the opened tab is visible.
        ILocator input = _page.GetByLabel(field, new() { Exact = true }).Filter(new() { Visible = true }).First;
        await FillAndLeaveAsync(input, value, true);
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
        Categorie categorie = Enum.Parse<Categorie>(await CellOf("Categorie"), true);
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

        // Retrieve the data from the table after it was stored
        string kenteken = await CellTextAsync(row, "Kenteken");
        string categorie = await CellTextAsync(row, "Categorie");

        Assert.Multiple(() =>
        {
            Assert.That(_huurder, Is.EqualTo(_reservation.Huurder).IgnoreCase, "Huurder");
            Assert.That(kenteken, Is.EqualTo(_reservation.Kenteken), "Kenteken");
            Assert.That(categorie, Is.EqualTo(_reservation.Categorie.ToString()).IgnoreCase, "Categorie");
        });
    }

    /// <summary>
    /// Returns the text of the cell in <paramref name="row"/> under the column with header <paramref name="header"/>.
    /// The column position is determined at run time, so reordered or added columns don't break the test.
    /// </summary>
    private static async Task<string> CellTextAsync(ILocator row, string header)
    {
        ILocator grid = row.Locator("xpath=ancestor::dxbl-grid[1]");
        ILocator columnHeader = grid.GetByRole(AriaRole.Columnheader, new() { Name = header, Exact = true });
        // Exactly one header must match, otherwise we can't be sure which column is meant.
        await Assertions.Expect(columnHeader).ToHaveCountAsync(1);

        // cellIndex counts every cell in the header row, including helper columns, so it matches the row's cells.
        int cellIndex = await columnHeader.EvaluateAsync<int>("th => th.cellIndex");
        string? text = await row.EvaluateAsync<string?>("(tr, i) => tr.cells[i]?.innerText ?? null", cellIndex);
        Assert.That(text, Is.Not.Null, $"Row has no cell at position {cellIndex} for column '{header}'.");
        TestContext.Out.WriteLine($"Column '{header}' is at position {cellIndex}: '{text!.Trim()}'");
        return text.Trim();
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
        Assert.That(_reservation.Kenteken, Is.Not.Null, "The kenteken for this reservation was not read after the first save.");
        string kenteken = _reservation.Kenteken;

        ILocator filter = _page.GetByLabel("Filter Kenteken", new() { Exact = true });
        await filter.FillAsync(kenteken);
        await filter.PressAsync("Enter");

        ILocator row = _page.GetByRole(AriaRole.Row)
            .Filter(new() { Has = _page.GetByRole(AriaRole.Gridcell, new() { Name = kenteken, Exact = true }) })
            .Filter(new() { Has = _page.GetByRole(AriaRole.Gridcell, new() { Name = _reservation.Kenteken, Exact = true }) });
        await row.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        return row;
    }

}
