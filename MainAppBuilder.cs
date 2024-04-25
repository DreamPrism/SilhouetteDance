using Lagrange.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SilhouetteDance.Core;
using SilhouetteDance.Core.Command;
using SilhouetteDance.Core.Dynamic;
using SilhouetteDance.Core.Message.Adapter;
using SilhouetteDance.Utility;

namespace SilhouetteDance;
public sealed class MainAppBuilder
{
    private IServiceCollection Services => _hostAppBuilder.Services;
    
    private ConfigurationManager Configuration => _hostAppBuilder.Configuration;
    
    private readonly HostApplicationBuilder _hostAppBuilder;
    
    private readonly AdapterCollection _adapters;

    public MainAppBuilder(string[] args)
    {
        _hostAppBuilder = new HostApplicationBuilder(args);
        _adapters = new AdapterCollection(Services);
        StandardComponent();
    }
    
    public MainAppBuilder AddAdapter<TAdapter>() where TAdapter : AdapterBase
    {
        var adapter = ActivatorUtilities.CreateInstance<TAdapter>(Services.BuildServiceProvider());
        _adapters.Add(adapter);
        return this;
    }
    
    public MainAppBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
    {
        configureDelegate(Services);
        return this;
    }
    
    public MainAppBuilder ConfigureConfiguration(string path, bool optional = false, bool reloadOnChange = false)
    {
        Configuration.AddJsonFile(path, optional, reloadOnChange);
        return this;
    }

    public MainApp Build()
    {
        StandardComponent();
        return new MainApp(_hostAppBuilder.Build(), _adapters);
    }
    
    private void StandardComponent() => Services
        .AddSingleton<AssemblyService>()
        .AddSingleton<ResContext>()  // Resources Context
        .AddSingleton<MetadataContext>()
        .AddSingleton<PlaywrightService>()
        .AddHostedService<MetadataService>()
        .AddSingleton(new CommandService(Configuration, Services)); // Command Manager
}