using System.Diagnostics;
using System.Net;
using Lagrange.Core;
using Microsoft.Extensions.Configuration;
using SilhouetteDance.Core.Command.Attributes;
using SilhouetteDance.Core.Message;
using SilhouetteDance.Core.Message.Entities;

namespace SilhouetteDance.Function;

public class WolframAlpha : FunctionBase
{
    private readonly IConfiguration _config;
    private readonly HttpClient _client;
    private string APIKey => _config["WolframAlpha:APIKey"];
    private string Proxy => _config["WolframAlpha:Proxy"];
    private bool Available { get; }

    public WolframAlpha(ResContext _resContext, IConfiguration config) : base(_resContext)
    {
        _config = config;
        if (string.IsNullOrEmpty(APIKey)) Available = false;
        else
            try
            {
                var handler = new HttpClientHandler();
                if (!string.IsNullOrEmpty(Proxy)) handler.Proxy = new WebProxy(Proxy);
                _client = new HttpClient(handler);
                Available = true;
            }
            catch
            {
                Available = false;
            }
    }

    [Command("wa")]
    public async Task<MessageStruct> Query([Composite] string[] msg)
    {
        var query = string.Join(' ', msg).Trim(' ');
        if (!Available)
            return new MessageStruct { new TextEntity("当前WolframAlpha服务不可用") };
        var watch = Stopwatch.StartNew();
        var param = $"i={WebUtility.UrlEncode(query)}&appid={APIKey}&fontsize=16";
        var url = $"http://api.wolframalpha.com/v1/simple?{param}";
        var result = await _client.GetByteArrayAsync(url);
        return new MessageStruct
        {
            new TextEntity($"本次生成回复用时:{watch.Elapsed.TotalSeconds:F1}s\n"),
            new ImageEntity(result, default)
        };
    }
}