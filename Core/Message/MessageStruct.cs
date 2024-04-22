using System.Text;

namespace SilhouetteDance.Core.Message;

public class MessageStruct : List<IMessageEntity>
{
    public long FromUin { get; set; }

    public long ToUin { get; set; }

    public long GroupUin { get; set; }

    public bool IsGroupMessage { get; set; }

    public List<object> Additional { get; private set; } = new();

    public DateTime Timestamp { get; set; } = DateTime.Now;

    public MessageStruct(IEnumerable<IMessageEntity> collection) : base(collection)
    {
    }

    public MessageStruct()
    {
    }

    public T GetEntity<T>() where T : IMessageEntity => this.OfType<T>().FirstOrDefault();

    public static MessageStruct operator ^(MessageStruct origin, MessageStruct target)
    {
        target.FromUin = origin.ToUin;
        target.ToUin = origin.FromUin;
        target.GroupUin = origin.GroupUin;
        target.IsGroupMessage = origin.IsGroupMessage;
        target.Additional = origin.Additional;
        target.Timestamp = origin.Timestamp;
        return target;
    }

    /// <summary>
    /// 调试用消息预览
    /// </summary>
    public string ToPreviewString()
    {
        var chainBuilder = new StringBuilder();

        chainBuilder.Append("[MessageStruct");
        if (GroupUin != 0) chainBuilder.Append($"({GroupUin})");
        chainBuilder.Append($"(from {FromUin} to {ToUin})");
        chainBuilder.Append("] ");
        foreach (var entity in this)
        {
            chainBuilder.Append(entity.ToPreviewString());
            if (this.Last() != entity) chainBuilder.Append(" | ");
        }

        return chainBuilder.ToString();
    }

    /// <summary>
    /// 用户用消息预览
    /// </summary>
    public string ToPreviewText()
    {
        var chainBuilder = new StringBuilder();
        foreach (var entity in this) chainBuilder.Append(entity.ToPreviewText());
        return chainBuilder.ToString();
    }
}