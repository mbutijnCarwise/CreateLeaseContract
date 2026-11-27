using CreateLeaseContract.Tests.Steps;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace CreateLeaseContract.Tests;

public class RentWiseLookupTests : PageTest
{
    private async Task PrepareLookupAsync(string initialDescription)
    {
        await Page.SetContentAsync("""
            <dxbl-input-editor>
              <input aria-label="Basis Soort" aria-describedby="soort-description">
              <button aria-controls="soort-list" onclick="document.getElementById('soort-list').hidden=false">Zoeken Basis Soort</button>
            </dxbl-input-editor>
            <span id="soort-description"></span>
            <dxbl-input-editor>
              <input aria-label="Basis Eenheid" aria-describedby="eenheid-description">
              <button aria-controls="eenheid-list" onclick="openEenheid()">Zoeken Basis Eenheid</button>
            </dxbl-input-editor>
            <span id="eenheid-description"></span>
            <div id="soort-list" role="dialog" hidden>
              <table role="treegrid">
                <tr role="row" ondblclick="selectSoort()">
                  <td role="gridcell">SL</td><td role="gridcell">Shortlease</td>
                </tr>
              </table>
            </div>
            <div id="eenheid-list" role="dialog" hidden>
              <table role="treegrid">
                <tr role="row"><td role="gridcell"></td><td role="gridcell"></td></tr>
                <tr role="row"><td role="gridcell">?</td><td role="gridcell"></td></tr>
                <tr role="row" data-choice ondblclick="selectEenheid(this)">
                  <td role="gridcell">MAAND20</td><td role="gridcell">Maand</td>
                </tr>
              </table>
            </div>
            <script>
              function selectSoort() {
                document.querySelector('[aria-label="Basis Soort"]').value = 'SL';
                document.getElementById('soort-list').hidden = true;
                setTimeout(() => {
                  document.querySelector('[aria-label="Basis Eenheid"]').value = 'DAG';
                  document.getElementById('eenheid-description').textContent = 'Dag';
                  document.getElementById('soort-description').textContent = 'Shortlease';
                }, 200);
              }
              function selectEenheid(row) {
                document.querySelector('[aria-label="Basis Eenheid"]').value = row.cells[0].textContent;
                document.getElementById('eenheid-description').textContent = 'Maand';
                document.getElementById('eenheid-list').hidden = true;
              }
              function openEenheid() {
                const search = document.querySelector('[aria-label="Basis Eenheid"]').value;
                const row = document.querySelector('#eenheid-list tr[data-choice]');
                row.hidden = search !== '' && search !== row.cells[0].textContent;
                document.getElementById('eenheid-list').hidden = false;
              }
            </script>
            """);
        await Page.Locator("#soort-description").EvaluateAsync("(el, text) => el.textContent = text", initialDescription);
    }

    [TestCase("")]
    [TestCase("Extern")]
    public async Task ResolvingSoortFinishesBeforeSelectingEenheid(string initialDescription)
    {
        await PrepareLookupAsync(initialDescription);
        await StepDefinitionsRentWise.SelectLookupValueAsync(Page, "Basis Soort", "SL");
        await Expect(Page.GetByLabel("Basis Eenheid", new() { Exact = true })).ToHaveValueAsync("DAG");
        await StepDefinitionsRentWise.SelectLookupValueAsync(Page, "Basis Eenheid", "MAAND20");
        await Expect(Page.GetByLabel("Basis Eenheid", new() { Exact = true })).ToHaveValueAsync("MAAND20");
        await Expect(Page.Locator("#eenheid-description")).ToHaveTextAsync("Maand");
    }

    [Test]
    public async Task SelectFirstValidEenheidReturnsAvailableUnitWhenMaand20IsAbsent()
    {
        await PrepareLookupAsync("");
        await Page.Locator("#eenheid-list tr[data-choice] td").First.EvaluateAsync("el => el.textContent = 'MAAND30'");
        await StepDefinitionsRentWise.SelectLookupValueAsync(Page, "Basis Soort", "SL");

        string selected = await StepDefinitionsRentWise.SelectLookupValueAsync(Page, "Basis Eenheid");

        Assert.That(selected, Is.EqualTo("MAAND30"));
        await Expect(Page.GetByLabel("Basis Eenheid", new() { Exact = true })).ToHaveValueAsync(selected);
    }
}
