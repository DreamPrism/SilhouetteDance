using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SilhouetteDance.Core.Message.Adapter.Implementation.CommandLine;

public class ConsoleAdapter : AdapterBase
{
    public override event EventHandler<MessageStruct> OnMessageReceived = delegate { };
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly MessageAdapter _msgAdapter = new();

    public ConsoleAdapter(IConfiguration config, ILogger<LagrangeApp> logger)
    {
        _config = config;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken = new())
    {
        _logger.LogInformation("Console Adapter started.");
        await Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await Console.In.ReadLineAsync();
                OnMessageReceived.Invoke(null, _msgAdapter.From(line));
            }
        }, cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken = new()) => Task.CompletedTask;

    public override async Task SendMessageAsync(MessageStruct message,
        CancellationToken cancellationToken = new())
    {
        var originalColor = Console.ForegroundColor;
        if (int.TryParse(_config["Console:Color"] ?? "11", out var color))
            Console.ForegroundColor = (ConsoleColor)color;
        await Console.Out.WriteLineAsync(message.ToPreviewText());
        Console.ForegroundColor = originalColor;
    }
}