using System.Text.Json;
using Lagrange.Core;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Interface.Api;
using Microsoft.Extensions.Configuration;

namespace SilhouetteDance.Core.Message.Adapter.Implementation.LagrangeQQ;

public static class BotManager
{
    public static BotContext CreateBot(string devicePath, string keystorePath,IConfiguration config)
    {
        var device = JsonSerializer.Deserialize<BotDeviceInfo>(File.ReadAllText(devicePath)) ?? new BotDeviceInfo();
        var keystore = JsonSerializer.Deserialize<BotKeystore>(File.ReadAllText(keystorePath)) ?? new BotKeystore();
        
        return BotFactory.Create(new BotConfig
        {
            UseIPv6Network = false,
            GetOptimumServer = true,
            Protocol = Protocols.Linux,
            AutoReconnect = true,
            CustomSignProvider = new LagrangeSignProvider(config)
        }, device, keystore);
    }

    public static void UpdateBotKeystore(BotContext context, string keystorePath) => 
            File.WriteAllText(keystorePath, JsonSerializer.Serialize(context.UpdateKeystore()));
}