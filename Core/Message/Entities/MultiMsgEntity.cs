namespace SilhouetteDance.Core.Message.Entities;

public class MultiMsgEntity:MessageEntity
{
    public List<MessageStruct> Messages { get; set; } = new List<MessageStruct>();

    public MultiMsgEntity(params MessageStruct[] msgs) => Messages.AddRange(msgs);
}