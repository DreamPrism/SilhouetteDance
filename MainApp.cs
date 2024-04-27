using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SilhouetteDance.Core.Command;
using SilhouetteDance.Core.Event.EventData;
using SilhouetteDance.Core.Message;
using SilhouetteDance.Core.Message.Adapter;
using SilhouetteDance.Core.Message.Entities;

namespace SilhouetteDance;

public sealed class MainApp : IHost
{
    private readonly IHost _hostApp;

    public IServiceProvider Services => _hostApp.Services;
    private AdapterCollection Adapters { get; }
    private ILogger<MainApp> Logger { get; }

    private IConfiguration Configuration { get; }

    internal MainApp(IHost host, IConfiguration configuration, AdapterCollection adapterCollection)
    {
        _hostApp = host;
        Configuration = configuration;
        Adapters = adapterCollection;
        Logger = Services.GetRequiredService<ILogger<MainApp>>();
    }

    public async Task StartAsync(CancellationToken cancellationToken = new())
    {
        Logger.LogInformation("SilhouetteDance started");
        await _hostApp.StartAsync(cancellationToken);

        foreach (var adapter in Adapters)
        {
            adapter.OnMessageReceived += async (_, @struct) =>
            {
                var result = await Services.GetRequiredService<CommandService>().InvokeCommand(@struct,adapter);
                if (result != null)
                {
                    result = @struct ^ result;
                    await adapter.SendMessageAsync(result, cancellationToken);
                }
            };
            adapter.OnFriendRequestReceived += async (_, args) =>
            {
                var superUsers = Configuration.GetSection("Generic:SuperUsers").Get<List<uint>>();
                foreach (var msg in superUsers.Select(admin => new MessageStruct
                             { ToUin = admin, IsGroupMessage = false }))
                {
                    msg.Add(new TextEntity($"[好友请求]来自:{args.SourceUin} " +
                                           $"附加消息:{(string.IsNullOrEmpty(args.Message) ? "无" : args.Message)}"));
                    await adapter.SendMessageAsync(msg, cancellationToken);
                }
            };
            adapter.OnGroupInvitationReceived += async (_, args) =>
            {
                var superUsers = JsonSerializer.Deserialize<HashSet<uint>>(Configuration["Generic:SuperUsers"] ?? "[]");
                var data = new GroupInvitationData { GroupUin = args.GroupUin, InvitorUin = args.InvitorUin };
                if (superUsers.Contains(args.InvitorUin))
                    await adapter.SetGroupInvitationAsync(data, RequestOperation.Accept, cancellationToken);
                else
                    await adapter.SetGroupInvitationAsync(data, RequestOperation.Reject, cancellationToken);
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