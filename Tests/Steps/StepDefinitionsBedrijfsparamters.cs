using Microsoft.Playwright;
using System.Threading.Tasks;
using Reqnroll;
using StringAssert = NUnit.Framework.StringAssert;

namespace Tests.Steps
{
    [Binding]
    public class StepDefinitionsBedrijfsparameters
    {
        private readonly IPage _page;

        public StepDefinitionsBedrijfsparameters(IPage page)
        {
            _page = page;
        }

    }
}