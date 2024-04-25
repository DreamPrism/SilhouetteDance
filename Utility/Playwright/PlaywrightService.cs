using Markdig;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;

namespace SilhouetteDance.Utility;

public class PlaywrightService : IAsyncDisposable
{
    private readonly IBrowser _browser;
    private readonly IPage _page;
    private readonly IConfiguration _config;

    public PlaywrightService(IConfiguration config)
    {
        _config = config;
        var playwright = Playwright.CreateAsync().Result;
        _browser = playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                ExecutablePath = _config["Playwright:ExecutablePath"]
            }
        ).Result;
        _page = _browser.NewPageAsync().Result;
    }

    public async Task<List<byte[]>> RenderMarkdownToImage(string markdown)
    {
        var pipeline = new MarkdownPipelineBuilder().UseEmphasisExtras().UseAdvancedExtensions().Build();
        var html = Markdown.ToHtml(markdown, pipeline);
        // Read the CSS from a file
        var css = "";
        if (!string.IsNullOrEmpty(_config["Playwright:StylePath"]))
            css = await File.ReadAllTextAsync(_config["Playwright:StylePath"]);

        // Add the CSS to the HTML
        html = $"<style>{css}</style>\n<article class=\"markdown-body\">\n{html}\n</article>";
        await _page.AddStyleTagAsync(new PageAddStyleTagOptions { Path = _config["Playwright:StylePath"] });
        await _page.SetViewportSizeAsync(768, 900);
        await _page.SetContentAsync(html);
        var screenshots = new List<byte[]>();
        while (true)
        {
            screenshots.Add(await _page.ScreenshotAsync());
            var scrollHeight = await _page.EvaluateAsync<double>("() => document.documentElement.scrollHeight");
            var scrollTop = await _page.EvaluateAsync<double>("() => window.pageYOffset");
            if (scrollTop + 900 >= scrollHeight)
            {
                break;
            }
            await _page.EvaluateAsync("() => { window.scrollBy(0, 900); }");
        }

        return screenshots;
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser == null) return;
        await _browser!.CloseAsync();
        await _browser!.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}