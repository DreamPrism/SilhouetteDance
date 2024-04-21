namespace SilhouetteDance.Core.Message.Entities;

public class ReplyEntity : MessageEntity
{
    public long TargetUin { get; set; }
    
    public ReplyEntity(long targetUin)
    {
        TargetUin = targetUin;
    }
}