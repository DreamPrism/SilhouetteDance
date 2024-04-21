using System.Text.Json;
using System.Text.Json.Serialization;
using ProtoBuf;

namespace SilhouetteDance.Core.Message.Entities;

public class MarkdownEntity : MessageEntity
{
    public MarkdownData Data { get; set; }
    
    internal MarkdownEntity() => Data = new MarkdownData();
    
    public MarkdownEntity(MarkdownData data) => Data = data;
    
    public MarkdownEntity(string data) => Data = JsonSerializer.Deserialize<MarkdownData>(data) ?? throw new Exception();
    
}

public class MarkdownData
{
    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}