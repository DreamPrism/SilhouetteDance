using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SilhouetteDance.Core.Command;
using SilhouetteDance.Core.Message.Adapter;

namespace SilhouetteDance;

public sealed class LagrangeApp:IHost
{
    private readonly IHost _hostApp;

    public IServiceProvider Services => _hostApp.Services;
    private AdapterCollection Adapters { get; }
    private ILogger<LagrangeApp> Logger { get; }
    internal LagrangeApp(IHost host, AdapterCollection adapterCollection)
    {
        _hostApp = host;
        Adapters = adapterCollection;
        Logger = Services.GetRequiredService<ILogger<LagrangeApp>>();
    }public async Task StartAsync(CancellationToken cancellationToken = new())
    {
        Logger.LogInformation("SilhouetteDance started");
        await _hostApp.StartAsync(cancellationToken);

        foreach (var adapter in Adapters)
        {
            adapter.OnMessageReceived += async (_, @struct) =>
            {
                var result = await Services.GetRequiredService<CommandService>().InvokeCommand(@struct);
                if (result != null)
                {
                    result = @struct ^ result;
                    await adapter.SendMessageAsync(result, cancellationToken);
                }
            };
            await adapter.StartAsync(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = new())
    {
        Logger.LogInformation("SilhoutteDance stopped");
        await _hostApp.StopAsync(cancellationToken);

        foreach (var adapter in Adapters)
        {
            await adapter.StopAsync(cancellationToken);
        }
    }

    public void Dispose()
    {
        _hostApp.Dispose();
    }
}