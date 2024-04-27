using System.ComponentModel.DataAnnotations;
using Lagrange.Core;
using SilhouetteDance.Core.Command.Attributes;
using SilhouetteDance.Core.Event.EventData;
using SilhouetteDance.Core.Message;
using SilhouetteDance.Core.Message.Adapter;
using SilhouetteDance.Core.Message.Entities;

namespace SilhouetteDance.Function;

public class HandleFriendRequest : FunctionBase
{
    public HandleFriendRequest(ResContext resContext) : base(resContext)
    {
    }

    [Command("fr accept")]
    public static async Task<MessageStruct> AcceptRequest(uint sourceUin,
        [Metadata(MetadataAttribute.MetadataType.Adapter)]
        AdapterBase adapter)
    {
        var result = await adapter.SetFriendRequestAsync(sourceUin, RequestOperation.Accept);
        return new MessageStruct
        {
            new TextEntity($"{(result ? "成功" : "未能")}同意{sourceUin}的好友请求")
        };
    }

    [Command("fr reject")]
    public static async Task<MessageStruct> RejectRequest(uint sourceUin,
        [Metadata(MetadataAttribute.MetadataType.Adapter)] AdapterBase adapter)
    {
        var result = await adapter.SetFriendRequestAsync(sourceUin, RequestOperation.Reject);
        return new MessageStruct
        {
            new TextEntity($"{(result ? "成功" : "未能")}拒绝{sourceUin}的好友请求")
        };
    }
}