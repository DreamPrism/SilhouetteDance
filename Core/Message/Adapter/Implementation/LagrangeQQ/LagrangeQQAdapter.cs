using Lagrange.Core;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Event.EventArg;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SilhouetteDance.Core.Event.EventArgs;
using SilhouetteDance.Core.Event.EventData;
using SilhouetteDance.Utility;
using LogLevel = Lagrange.Core.Event.EventArg.LogLevel;

namespace SilhouetteDance.Core.Message.Adapter.Implementation.LagrangeQQ;

public class LagrangeQQAdapter : AdapterBase
{
    private readonly IConfiguration _config;
    private readonly ILogger _logger;
    private readonly BotContext _lagrange;
    private readonly MessageAdapter _msgAdapter;
    private readonly List<FriendRequestEvent> _friendRequests = new();

    public LagrangeQQAdapter(IConfiguration config, ILogger<MainApp> logger, IServiceProvider services)
    {
        _config = config;
        _logger = logger;
        _lagrange = BotManager.CreateBot(_config["Lagrange:DeviceInfoPath"] ?? "device.json",
            config["Lagrange:KeyStorePath"] ?? "keystore.json");
        _msgAdapter = new MessageAdapter(services.GetRequiredService<MarkdownRenderService>());
    }

    public override event EventHandler<MessageStruct> OnMessageReceived = delegate { };
    public override event EventHandler<FriendRequestEventArgs> OnFriendRequestReceived = delegate { };
    public override event EventHandler<GroupInvitationEventArgs> OnGroupInvitationReceived = delegate { };

    public override async Task<bool> SetFriendRequestAsync(uint sourceUin, RequestOperation op,
        CancellationToken cancellationToken = new())
    {
        var qRequest = _friendRequests.FirstOrDefault(r => r.SourceUin == sourceUin);
        if (qRequest != null)
            return await _lagrange.SetFriendRequest(qRequest, op == RequestOperation.Accept);
        return false;
    }

    public override async Task<bool> SetGroupInvitationAsync(GroupInvitationData invitation, RequestOperation op,
        CancellationToken cancellationToken = new())
    {
        var qRequest = (await _lagrange.FetchGroupRequests())?.FirstOrDefault(r =>
            r.GroupUin == invitation.GroupUin && r.InvitorMemberUin == invitation.InvitorUin);
        if (qRequest != null) return await _lagrange.SetGroupRequest(qRequest, op == RequestOperation.Accept);
        return false;
    }

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
        {
            _logger.LogInformation(@event.Chain.ToPreviewString());
            OnMessageReceived.Invoke(context, _msgAdapter.From(@event.Chain));
        };
        _lagrange.Invoker.OnFriendMessageReceived += (context, @event) =>
        {
            _logger.LogInformation(@event.Chain.ToPreviewString());
            OnMessageReceived.Invoke(context, _msgAdapter.From(@event.Chain));
        };
        _lagrange.Invoker.OnGroupInvitationReceived += (context, @event) =>
        {
            _logger.LogInformation(@event.EventMessage);
            OnGroupInvitationReceived.Invoke(context, new GroupInvitationEventArgs(@event.GroupUin, @event.InvitorUin));
        };
        _lagrange.Invoker.OnFriendRequestEvent += (context, @event) =>
        {
            _logger.LogInformation(@event.EventMessage);
            _friendRequests.Add(@event);
            OnFriendRequestReceived.Invoke(context, new FriendRequestEventArgs(@event.SourceUin, @event.Message));
        };

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