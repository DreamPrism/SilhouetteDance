namespace SilhouetteDance.Core.Event.EventArgs;

public class FriendRequestEventArgs:EventBase
{
    public uint SourceUin { get; }
    
    public string Message { get; }
    
    public FriendRequestEventArgs(uint sourceUin, string message)
    {
        SourceUin = sourceUin;
        Message = message;
        EventMessage = $"[{nameof(FriendRequestEventArgs)}]: {SourceUin}|{Message}";
    }
}