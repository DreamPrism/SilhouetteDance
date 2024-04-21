namespace SilhouetteDance.Core.Message;

public class MessageStruct : List<MessageEntity>
{
    public long FromUin { get; set; }
    
    public long ToUin { get; set; }
    
    public long GroupUin { get; set; }
    
    public bool IsGroupMessage { get; set; }

    public List<object> Additional { get; private set; } = new();
    
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public MessageStruct(IEnumerable<MessageEntity> collection) : base(collection) { }
    
    public MessageStruct() { }
    
    public T GetEntity<T>() where T : MessageEntity => this.OfType<T>().FirstOrDefault();
    
    public static MessageStruct operator^ (MessageStruct origin, MessageStruct target)
    {
        target.FromUin = origin.ToUin;
        target.ToUin = origin.FromUin;
        target.GroupUin = origin.GroupUin;
        target.IsGroupMessage = origin.IsGroupMessage;
        target.Additional = origin.Additional;
        target.Timestamp = origin.Timestamp;
        return target;
    }
}