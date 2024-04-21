using Lagrange.Core.Message;
using SilhouetteDance.Core.Message.Entities;
using RawEntity = Lagrange.Core.Message.Entity.TextEntity;

namespace SilhouetteDance.Core.Message.Adapter.Implementation.LagrangeQQ;

public class MessageAdapter : MessageAdapter<MessageChain>
{
    public override MessageStruct From(MessageChain message)
    {
        var from = new MessageStruct
        {
            FromUin = message.FriendUin,
            GroupUin = message.GroupUin ?? 0,
            IsGroupMessage = message.GroupUin != null,
        };
        foreach (var entity in message)
        {
            switch (entity)
            {
                case RawEntity text:
                    from.Add(new TextEntity(text.Text));
                    break;
                case Lagrange.Core.Message.Entity.ImageEntity image:
                    from.Add(new ImageEntity(image.ImageUrl, image.PictureSize));
                    break;
                case Lagrange.Core.Message.Entity.MentionEntity mention:
                    from.Add(new MentionEntity(mention.Uin));
                    break;
                case Lagrange.Core.Message.Entity.MultiMsgEntity multiMsg:
                    from.Add(new MultiMsgEntity(multiMsg.Chains.Select(From).ToArray()));
                    break;
            }
        }

        from.Add(new TextEntity(message.GetEntity<RawEntity>()?.Text ?? ""));
        return from;
    }

    public override MessageChain To(MessageStruct message)
    {
        var builder = message.IsGroupMessage
            ? MessageBuilder.Group((uint)message.GroupUin)
            : MessageBuilder.Friend((uint)message.ToUin);

        foreach (var entity in message)
        {
            switch (entity)
            {
                case ImageEntity image:
                    if (image.Data != null) builder.Image(image.Data);
                    break;
                case TextEntity text:
                    builder.Text(text.Text);
                    break;
                case MentionEntity mention:
                    builder.Mention((uint)mention.TargetUin);
                    break;
                case MultiMsgEntity multiMsg:
                    builder.MultiMsg(null, multiMsg.Messages.Select(To).ToArray());
                    break;
            }
        }

        return builder.Build();
    }
}