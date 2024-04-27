namespace SilhouetteDance.Core.Event.EventArgs;

public class GroupInvitationEventArgs : EventBase
{
    public uint GroupUin { get; }
    
    public uint InvitorUin { get; }
    
    internal GroupInvitationEventArgs(uint groupUin, uint invitorUin)
    {
        GroupUin = groupUin;
        InvitorUin = invitorUin;
        EventMessage = $"[{nameof(GroupInvitationEventArgs)}]: {GroupUin} from {InvitorUin}";
    }
}