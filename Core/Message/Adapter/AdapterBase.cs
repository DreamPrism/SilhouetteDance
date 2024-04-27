using SilhouetteDance.Core.Event.EventArgs;
using SilhouetteDance.Core.Event.EventData;

namespace SilhouetteDance.Core.Message.Adapter;

public abstract class AdapterBase
{
    public abstract event EventHandler<MessageStruct> OnMessageReceived;
#pragma warning disable CS0067
    public virtual event EventHandler<FriendRequestEventArgs> OnFriendRequestReceived;
    public virtual event EventHandler<GroupInvitationEventArgs> OnGroupInvitationReceived;
#pragma warning restore CS0067
    public abstract Task StartAsync(CancellationToken cancellationToken = new());
    public abstract Task StopAsync(CancellationToken cancellationToken = new());
    public abstract Task SendMessageAsync(MessageStruct message, CancellationToken cancellationToken = new());

    public virtual Task<bool> SetFriendRequestAsync(uint sourceUin, RequestOperation operation,
        CancellationToken cancellationToken = new())
    {
        return Task.FromResult(false);
    }

    public virtual Task<bool> SetGroupInvitationAsync(GroupInvitationData invitation, RequestOperation operation,
        CancellationToken cancellationToken = new())
    {
        return Task.FromResult(false);
    }
}