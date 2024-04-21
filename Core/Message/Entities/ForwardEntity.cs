namespace SilhouetteDance.Core.Message.Entities;

public class ForwardEntity : MessageEntity
{
    public MessageStruct Message { get; set; }

    public ForwardEntity(MessageStruct message) => Message = message;
}