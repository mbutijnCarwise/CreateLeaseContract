using Microsoft.Playwright;
using Reqnroll;
using Reqnroll.BoDi;

namespace CreateLeaseContract.Tests;

[Binding]
public sealed class PlaywrightHooks
{
    private readonly IObjectContainer _objectContainer;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public PlaywrightHooks(IObjectContainer objectContainer)
    {
        _objectContainer = objectContainer;
    }

    [BeforeTestRun]
    public static void ConfigureAssertions() =>
        Assertions.SetDefaultExpectTimeout(15_000);

    [BeforeScenario]
    public async Task StartBrowser()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        IPage page = await _browser.NewPageAsync();
        _objectContainer.RegisterInstanceAs(page);
    }

    [AfterScenario]
    public async Task StopBrowser()
    {
        try
        {
            if (_browser is not null)
                await _browser.CloseAsync();
        }
        finally
        {
            _playwright?.Dispose();
        }
    }
}
