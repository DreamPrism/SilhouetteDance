namespace SilhouetteDance.Core.Message;

/// <summary>
/// Defined as a base class for all entities for Message
/// </summary>
public interface IMessageEntity
{
    public string ToPreviewString();
    public string ToPreviewText() => "[暂不支持该消息类型]";
}