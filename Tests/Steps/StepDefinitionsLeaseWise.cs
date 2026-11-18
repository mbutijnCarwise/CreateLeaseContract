using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;

namespace CreateLeaseContract.Tests.Steps;

[Binding]
public sealed class StepDefinitionsLeaseWise
{
    private readonly IPage _page;
    public StepDefinitionsLeaseWise(IPage page)
    {
        _page = page;
    }


}