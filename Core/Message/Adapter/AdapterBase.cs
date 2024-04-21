namespace SilhouetteDance.Core.Message.Adapter;

public abstract class AdapterBase
{
    public abstract event EventHandler<MessageStruct> OnMessageReceived;
    
    public abstract Task StartAsync(CancellationToken cancellationToken = new());
    public abstract Task StopAsync(CancellationToken cancellationToken = new());
    public abstract Task SendMessageAsync(MessageStruct message, CancellationToken cancellationToken = new());
}