// See https://aka.ms/new-console-template for more information

using System.Runtime;
using Microsoft.Extensions.Hosting;
using SilhouetteDance.Core.Message.Adapter.Implementation.CommandLine;
using SilhouetteDance.Core.Message.Adapter.Implementation.LagrangeQQ;

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

        var hostBuilder = new MainAppBuilder(args)
            .ConfigureConfiguration("appsettings.json", false, true)
            .AddAdapter<LagrangeQQAdapter>()
            .AddAdapter<ConsoleAdapter>();
        hostBuilder.Build().Run();
    }
}