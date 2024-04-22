namespace SilhouetteDance.Core.Message.Entities;

public class MultiMsgEntity : IMessageEntity
{
    public List<MessageStruct> Messages { get; set; } = new List<MessageStruct>();

    public MultiMsgEntity(params MessageStruct[] msgs) => Messages.AddRange(msgs);
    public string ToPreviewString() => $"[MultiMsgEntity] {Messages.Count} chains";
    public string ToPreviewText() => "[聊天记录]";
}