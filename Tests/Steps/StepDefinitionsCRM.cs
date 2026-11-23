using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;

namespace CreateLeaseContract.Tests.Steps;

[Binding]
public class StepDefinitionsCRM
{
    private readonly IPage _page;

    public StepDefinitionsCRM(IPage page)
    {
        _page = page;
    }
}
