using System.Diagnostics;
using System.Net;
using GenerativeAI.Models;
using GenerativeAI.Types;
using Lagrange.Core;
using Microsoft.Extensions.Configuration;
using SilhouetteDance.Core.Command.Attributes;
using SilhouetteDance.Core.Message;
using SilhouetteDance.Core.Message.Entities;

namespace SilhouetteDance.Function.GoogleGemini;

public class GeminiChat : FunctionBase
{
    private readonly IConfiguration _config;
    private string APIKey => _config["GoogleGemini:APIKey"];
    private string Proxy => _config["GoogleGemini:Proxy"];

    private readonly GeminiProVision ProVisionModal;
    private readonly GenerativeModel LatestModal;
    private readonly GenerativeModel DefaultModel;
    private bool Available { get; }

    public GeminiChat(ResContext resContext, IConfiguration config) : base(resContext)
    {
        _config = config;
        if (string.IsNullOrEmpty(APIKey)) Available = false;
        else
        {
            try
            {
                var handler = new HttpClientHandler();
                if (!string.IsNullOrEmpty(Proxy)) handler.Proxy = new WebProxy(Proxy);
                var httpClient = new HttpClient(handler);
                ProVisionModal = new GeminiProVision(APIKey, httpClient);
                LatestModal = new GenerativeModel(APIKey, "gemini-1.5-pro-latest", httpClient);
                DefaultModel = new GenerativeModel(APIKey, client: httpClient);
                Available = true;
            }
            catch
            {
                Available = false;
            }
        }
    }

    [Command("gmn ask")]
    public async Task<MessageStruct> SimpleChat(string msgText)
    {
        if (!Available)
            return new MessageStruct { new TextEntity("当前Gemini服务不可用") };

        var watch = Stopwatch.StartNew();
        var response = await DefaultModel.GenerateContentAsync(msgText);
        return GenerateReply(response, watch.Elapsed.TotalSeconds);
    }

    [Command("gmn vision")]
    public async Task<MessageStruct> ProVisionChat(
        [Metadata(MetadataAttribute.MetadataType.MessageStruct)]
        MessageStruct msg)
    {
        if (!Available)
            return new MessageStruct { new TextEntity("当前Gemini服务不可用") };

        if (!msg.Any(e => e is ImageEntity))
            return new MessageStruct { new TextEntity("请至少提供一张图片") };

        var watch = Stopwatch.StartNew();
        var parts = new List<Part>();
        var replaced = false;
        foreach (var entity in msg)
            switch (entity)
            {
                case TextEntity text:
                    if (!replaced && text.Text.Contains("gmn vision"))
                    {
                        parts.Add(new Part { Text = text.Text.Replace("gmn vision", "") });
                        replaced = true;
                    }

                    parts.Add(new Part { Text = text.Text });
                    break;
                case ImageEntity image:
                    if (image.Data == null)
                        await image.DownloadImageData();
                    parts.Add(new Part
                    {
                        InlineData = new GenerativeContentBlob
                        {
                            MimeType = "image/jpeg",
                            Data = Convert.ToBase64String(image.Data ?? Array.Empty<byte>())
                        }
                    });
                    break;
            }

        var response = await ProVisionModal.GenerateContentAsync(parts);
        var resultMarkdown = response.Text();
        return resultMarkdown != null
            ? GenerateReply(resultMarkdown, watch.Elapsed.TotalSeconds)
            : new MessageStruct { new TextEntity("无法获取有效的回复") };
    }

    [Command("gmn latest")]
    public async Task<MessageStruct> LatestChat(
        [Metadata(MetadataAttribute.MetadataType.MessageStruct)]
        MessageStruct msg)
    {
        if (!Available)
            return new MessageStruct { new TextEntity("当前Gemini服务不可用") };

        var watch = Stopwatch.StartNew();
        var parts = new List<Part>();
        var replaced = false;
        foreach (var entity in msg)
            switch (entity)
            {
                case TextEntity text:
                    if (!replaced && text.Text.Contains("gmn latest"))
                    {
                        parts.Add(new Part { Text = text.Text.Replace("gmn latest", "") });
                        replaced = true;
                    }

                    parts.Add(new Part { Text = text.Text });
                    break;
                case ImageEntity image:
                    if (image.Data == null)
                        await image.DownloadImageData();
                    parts.Add(new Part
                    {
                        InlineData = new GenerativeContentBlob
                        {
                            MimeType = "image/jpeg",
                            Data = Convert.ToBase64String(image.Data ?? Array.Empty<byte>())
                        }
                    });
                    break;
            }

        var response = await LatestModal.GenerateContentAsync(parts);
        var resultMarkdown = response.Text();
        return resultMarkdown != null
            ? GenerateReply(resultMarkdown, watch.Elapsed.TotalSeconds)
            : new MessageStruct { new TextEntity("无法获取有效的回复") };
    }

    [Command("gmn essay")]
    public async Task<MessageStruct> GenEssay(string msgText)
    {
        if (!Available)
            return new MessageStruct { new TextEntity("当前Gemini服务不可用") };

        var watch = Stopwatch.StartNew();
        var path = Path.Combine(_config["GoogleGemini:PromptsPath"] ?? "prompts", "essay.txt");
        if (!File.Exists(path))
            return new MessageStruct { new TextEntity("缺少生成作文所需的prompts") };

        var input = (await File.ReadAllTextAsync(path)).Replace("${TOPIC HERE}", msgText);
        var response = await DefaultModel.GenerateContentAsync(input);
        return GenerateReply(response, watch.Elapsed.TotalSeconds);
    }

    private static MessageStruct GenerateReply(string response, double timeConsumed) =>
        new()
        {
            new TextEntity($"本次生成回复用时:{timeConsumed:F1}s\n"),
            new MarkdownEntity(new MarkdownData { Content = response })
        };
}