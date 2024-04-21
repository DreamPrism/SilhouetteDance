using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LogLevel = Lagrange.Core.Event.EventArg.LogLevel;

namespace SilhouetteDance.Core.Message.Adapter.Implementation.LagrangeQQ;

public class LagrangeQQAdapter : AdapterBase
{
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly BotContext _lagrange;
    private readonly MessageAdapter _msgAdapter = new();

    public LagrangeQQAdapter(IConfiguration config, ILogger<LagrangeApp> logger)
    {
        _config = config;
        _logger = logger;
        _lagrange = BotManager.CreateBot(_config["Lagrange:DeviceInfoPath"] ?? "device.json",
            config["Lagrange:KeyStorePath"] ?? "keystore.json");
    }

    public override event EventHandler<MessageStruct> OnMessageReceived = delegate { };

    public override async Task StartAsync(CancellationToken cancellationToken = new())
    {
        #region Basic Event Handlers

        _lagrange.Invoker.OnBotOnlineEvent += (context, _) =>
            BotManager.UpdateBotKeystore(context, _config["Lagrange:KeyStorePath"] ?? "keystore.json");
        _lagrange.Invoker.OnBotLogEvent += (_, @event) =>
        {
            switch (@event.Level)
            {
                case LogLevel.Debug:
                    _logger.LogDebug(@event.EventMessage);
                    break;
                case LogLevel.Verbose:
                    _logger.LogTrace(@event.EventMessage);
                    break;
                case LogLevel.Information:
                    _logger.LogInformation(@event.EventMessage);
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(@event.EventMessage);
                    break;
                case LogLevel.Exception:
                    _logger.LogError(@event.EventMessage);
                    break;
                case LogLevel.Fatal:
                    _logger.LogCritical(@event.EventMessage);
                    break;
                default:
                    _logger.LogDebug(@event.EventMessage);
                    break;
            }
        };
        _lagrange.Invoker.OnBotCaptchaEvent += (context, @event) =>
        {
            _logger.LogInformation($"Captcha Url: {@event.Url}");
            _logger.LogInformation("Please input ticket:");
            var ticket = Console.ReadLine();
            _logger.LogInformation("Please input randStr:");
            var randStr = Console.ReadLine();
            if (ticket is not null && randStr is not null)
                context.SubmitCaptcha(ticket, randStr);
        };

        #endregion

        _lagrange.Invoker.OnGroupMessageReceived += (context, @event) =>
            OnMessageReceived.Invoke(context, _msgAdapter.From(@event.Chain));
        _lagrange.Invoker.OnFriendMessageReceived += (context, @event) =>
            OnMessageReceived.Invoke(context, _msgAdapter.From(@event.Chain));

        var success = false;
        if (string.IsNullOrEmpty(_config["Lagrange:Password"]))
        {
            if (await _lagrange.FetchQrCode() is { } qrCode)
            {
                _logger.LogInformation("Logging via QrCode...");
                await File.WriteAllBytesAsync("qrcode.png", qrCode.QrCode, cancellationToken);
                await _lagrange.LoginByQrCode();
                _logger.LogInformation("Please scan qrcode.png to login.");
                success = true;
            }
        }
        else
        {
            success = await _lagrange.LoginByPassword();
        }

        _logger.LogInformation(success
            ? $"LagrangeQQ Adapter Started: {_lagrange.UpdateKeystore().Uin}"
            : $"LagrangeQQ Adapter Failed: {_lagrange.UpdateKeystore().Uin}");
    }

    public override Task StopAsync(CancellationToken cancellationToken = new())
    {
        _lagrange.Dispose();
        return Task.CompletedTask;
    }

    public override Task SendMessageAsync(MessageStruct message, CancellationToken cancellationToken = new()) =>
        _lagrange.SendMessage(_msgAdapter.To(message));
}