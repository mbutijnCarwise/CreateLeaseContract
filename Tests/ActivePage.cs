using Microsoft.Playwright;

public sealed class ActivePage
{
    public ActivePage(IPage page) => Page = page;

    public IPage Page { get; set; }
}