using System.Text.Json;
using System.Text.Json.Serialization;
using ProtoBuf;

namespace SilhouetteDance.Core.Message.Entities;

public class MarkdownEntity : IMessageEntity
{
    public MarkdownData Data { get; set; }
    
    internal MarkdownEntity() => Data = new MarkdownData();
    
    public MarkdownEntity(MarkdownData data) => Data = data;
    
    public MarkdownEntity(string data) => Data = JsonSerializer.Deserialize<MarkdownData>(data) ?? throw new Exception();

    public string ToPreviewString() => $"[Markdown] {Data.Content[..Math.Min(Data.Content.Length, 5)]}...";
    public string ToPreviewText()=>$"[Markdown] {Data.Content[..Math.Min(Data.Content.Length, 10)]}...";
}

public class MarkdownData
{
    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}