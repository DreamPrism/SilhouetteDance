namespace SilhouetteDance.Core.Message.Adapter;

public abstract class MessageAdapter<T> where T : class
{
    public abstract MessageStruct From(T message);
    
    public abstract T To(MessageStruct message);
}