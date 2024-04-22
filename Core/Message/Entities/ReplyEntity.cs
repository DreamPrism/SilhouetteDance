namespace SilhouetteDance.Core.Message.Entities;

public class ReplyEntity : IMessageEntity
{
    public long TargetUin { get; set; }
    
    public ReplyEntity(long targetUin)
    {
        TargetUin = targetUin;
    }
    public string ToPreviewString() => $"[ReplyTo:{TargetUin}]";
    public string ToPreviewText() => $"[回复{TargetUin}]";
}