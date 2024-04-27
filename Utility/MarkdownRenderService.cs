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
        var content = JsonContent.Create(new
        {
            md_content = markdown,
            width = int.Parse(_config["MarkdownRenderer:Width"] ?? "768")
        });
        try
        {
            var response = await _client.PostAsync(API, content);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsByteArrayAsync();
            _logger.LogError($"Failed to render markdown: {response.ReasonPhrase}");
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to render markdown: {e.Message}");
        }
        return null;
    }
}