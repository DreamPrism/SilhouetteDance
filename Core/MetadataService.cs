using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SilhouetteDance.Core;

public class MetadataService : BackgroundService
{
    private readonly IServiceProvider _provider;

    public MetadataService(IServiceProvider provider)
    {
        _provider = provider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _provider.GetRequiredService<MetadataContext>(); // initialize Metadata
        
        return Task.CompletedTask;
    }
}