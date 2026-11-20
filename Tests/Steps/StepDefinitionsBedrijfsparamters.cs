using Microsoft.Playwright;
using System.Threading.Tasks;
using Reqnroll;
using StringAssert = NUnit.Framework.StringAssert;

namespace Tests.Steps
{
    [Binding]
    public class StepDefinitionsBedrijfsparamters
    {
        private readonly IPage _page;

        public StepDefinitionsBedrijfsparamters(IPage page)
        {
            _page = page;
        }

        [When(@"I navigate to Bedrijfsparameters in the modules")]
        public async Task NavigateToBedrijfsparameters()
        {
            await _page.GetByRole(AriaRole.Link, new() { Name = "Bedrijfsparameters" }).ClickAsync();
        }
    }
}