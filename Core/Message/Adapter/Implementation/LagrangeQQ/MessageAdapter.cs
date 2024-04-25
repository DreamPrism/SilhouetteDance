using Lagrange.Core.Message;
using SilhouetteDance.Core.Message.Entities;
using SilhouetteDance.Utility;
using RawEntity = Lagrange.Core.Message.Entity.TextEntity;

namespace SilhouetteDance.Core.Message.Adapter.Implementation.LagrangeQQ;

public class MessageAdapter : MessageAdapter<MessageChain>
{
    private PlaywrightService _playwright;
    public MessageAdapter(PlaywrightService playwright) => _playwright = playwright;

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
                case Lagrange.Core.Message.Entity.MarkdownEntity markdown:
                    from.Add(new MarkdownEntity(new MarkdownData { Content = markdown.Data.Content }));
                    break;
                case Lagrange.Core.Message.Entity.MultiMsgEntity multiMsg:
                    from.Add(new MultiMsgEntity(multiMsg.Chains.Select(From).ToArray()));
                    break;
            }
        }

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
                case MarkdownEntity markdown:
                    var imgs = _playwright.RenderMarkdownToImage(markdown.Data.Content).Result;
                    foreach (var img in imgs)
                        builder.Image(img);
                    break;
                case MultiMsgEntity multiMsg:
                    builder.MultiMsg(null, multiMsg.Messages.Select(To).ToArray());
                    break;
            }
        }

        return builder.Build();
    }
}