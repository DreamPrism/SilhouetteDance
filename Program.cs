// See https://aka.ms/new-console-template for more information

using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface;
using Lagrange.Core.Common.Interface.Api;
using Lagrange.Core.Event.EventArg;
using Lagrange.Core.Message;
using Lagrange.Core.Message.Entity;
using Microsoft.Extensions.Hosting;
using SilhouetteDance.Configuration;

namespace SilhouetteDance;

internal class Program
{
    public static void Main(string[] args)
    {
        GCSettings.LatencyMode = GCLatencyMode.Batch;

        AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
        {
            var exception = (Exception)eventArgs.ExceptionObject;
            Console.WriteLine(exception);
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) => { Console.WriteLine("Process exit"); };
        AppDomain.CurrentDomain.FirstChanceException+= (_, e) => { Console.WriteLine(e.Exception); };

        var hostBuilder = new LagrangeAppBuilder(args)
            .ConfigureConfiguration("appsettings.json", false, true);
        hostBuilder.Build().Run();
        
        /*
            SilhouetteDanceConfig.Init();
            var config = SilhouetteDanceConfig.Instance;

            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            BotDeviceInfo _deviceInfo;
            BotKeystore _keyStore;
            var deviceInfoPath = config.DeviceInfoPath ?? Path.Combine(config.BotFilePath, "device.json");
            var KeyStorePath = config.KeyStorePath ?? Path.Combine(config.BotFilePath, "keystore.json");

            if (File.Exists(deviceInfoPath))
                _deviceInfo = JsonSerializer.Deserialize<BotDeviceInfo>(await File.ReadAllTextAsync(deviceInfoPath));
            else
                await File.WriteAllTextAsync(deviceInfoPath,
                    JsonSerializer.Serialize(_deviceInfo = BotDeviceInfo.GenerateInfo()));

            if (File.Exists(KeyStorePath))
                _keyStore = JsonSerializer.Deserialize<BotKeystore>(await File.ReadAllTextAsync(KeyStorePath),
                    new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve });
            else
                await File.WriteAllTextAsync(KeyStorePath, JsonSerializer.Serialize(_keyStore = new BotKeystore()));

            var bot = BotFactory.Create(new BotConfig
            {
                UseIPv6Network = false,
                GetOptimumServer = true,
                AutoReconnect = true,
                Protocol = Protocols.Linux,
                CustomSignProvider = new LagrangeSignProvider(),
            }, _deviceInfo, _keyStore);

            bot.Invoker.OnBotLogEvent += (_, e) => Utils.Log(e.Level, e.EventMessage);

            await bot.LoginByPassword();
            */
    }
}