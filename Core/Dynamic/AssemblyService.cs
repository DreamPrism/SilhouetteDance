using System.Reflection;
using Lagrange.Core;
using SilhouetteDance.Core.Command.Attributes;
using SilhouetteDance.Function;

namespace SilhouetteDance.Core.Dynamic;

/// <summary>
/// Dynamically load the dll plugin
/// </summary>
public class AssemblyService
{
    private readonly Dictionary<string, Assembly> _loaded = new();
    private readonly ResContext _resContext;

    public AssemblyService(ResContext resContext) => _resContext = resContext;

    public void UnloadAssembly(string key) => _loaded.Remove(key);

    public void LoadAssembly(string key)
    {
        var target = _resContext[$"Plugins:{key}"];
        var assembly = Assembly.Load(target);
        _loaded[key] = assembly;
    }

    public Type[] GetFuncModules(out List<MethodInfo> methods)
    {
        var types = new List<Type>();
        var funcTypes = new List<Type>();
        
        foreach (var (_, assembly) in _loaded)
        {
            var func = assembly.GetTypes().FirstOrDefault(t => t.IsSubclassOf(typeof(FunctionBase)));
            if (func != null)
            {
                funcTypes.Add(func);
                types.AddRange(assembly.GetTypes());
            }
        }
        methods = new List<MethodInfo>();
        foreach (var type in funcTypes)
        { 
            var method = type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.GetCustomAttribute<CommandAttribute>() != null);
            methods.AddRange(method);
        }
        return types.ToArray();
    }
}