namespace SilhouetteDance.Core.Message.Entities;

public class MentionEntity : MessageEntity
{
    public long TargetUin { get; set; }
    
    public MentionEntity(long targetUin)
    {
        TargetUin = targetUin;
    }
}