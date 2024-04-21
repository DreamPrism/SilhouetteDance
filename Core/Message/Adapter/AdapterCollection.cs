using Microsoft.Extensions.DependencyInjection;
namespace SilhouetteDance.Core.Message.Adapter;

public class AdapterCollection : List<AdapterBase>
{
    public IServiceCollection Services { get; }

    public AdapterCollection(IServiceCollection services) => Services = services;
}