using System.Net.Http.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SilhouetteDance.Utility;

public class MarkdownRenderService
{
    private HttpClient _client { get; } = new();
    private string API { get; set; }
    private IConfiguration _config { get; }
    private ILogger _logger { get; }

    public MarkdownRenderService(IConfiguration config, ILogger<MainApp> logger)
    {
        _config = config;
        _logger = logger;
        API = _config["MarkdownRenderer:API"];
    }

    public async Task<byte[]> RenderMarkdownAsync(string markdown)
    {
        JsonContent content;
        if (!string.IsNullOrEmpty(_config["MarkdownRenderer:Width"]))
            content = JsonContent.Create(new
            {
                md_content = markdown,
                width = int.Parse(_config["MarkdownRenderer:Width"])
            });
        else
            content = JsonContent.Create(new
            {
                md_content = markdown
            });
        try
        {
            var response = await _client.PostAsync(API, content);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsByteArrayAsync();
            _logger.LogWarning($"Failed to render markdown: {response.ReasonPhrase}, send raw markdown text.");
        }
        catch (Exception e)
        {
            _logger.LogWarning($"Failed to render markdown: {e.Message}, send raw markdown text.");
        }

        return null;
    }
}