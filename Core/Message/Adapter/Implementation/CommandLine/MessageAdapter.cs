using System.Text;
using SilhouetteDance.Core.Message.Entities;

namespace SilhouetteDance.Core.Message.Adapter.Implementation.CommandLine;

public class MessageAdapter : MessageAdapter<string>
{
    public override MessageStruct From(string message)
    {
        var from = new MessageStruct
        {
            FromUin = 0,
            GroupUin = 0,
            IsGroupMessage = false
        };
        from.Add(new TextEntity(message));
        return from;
    }

    public override string To(MessageStruct message) => message.ToPreviewText();
}