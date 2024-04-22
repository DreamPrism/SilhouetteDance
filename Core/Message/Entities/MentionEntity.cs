namespace SilhouetteDance.Core.Message.Entities;

public class MentionEntity : IMessageEntity
{
    public long TargetUin { get; set; }
    
    public MentionEntity(long targetUin)
    {
        TargetUin = targetUin;
    }

    public string ToPreviewString() => $"[@{TargetUin}]";
    public string ToPreviewText() => $"@{TargetUin}";
}