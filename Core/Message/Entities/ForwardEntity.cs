namespace SilhouetteDance.Core.Message.Entities;

public class ForwardEntity : IMessageEntity
{
    public MessageStruct Message { get; set; }

    public ForwardEntity(MessageStruct message) => Message = message;
    public string ToPreviewString() => $"[Forward]:{{{Message.ToPreviewString()}}}";
    public string ToPreviewText() => "[转发消息]";
}