using System.Reflection;

namespace SilhouetteDance.Core.Command;

internal static class EnumConverter
{
    private static readonly Type[] TargetSignature = { typeof(string) };

    public static void RegisterConverter(CommandService service)
    {
        var field = service.GetType().GetField("Converters", BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException("Could not find Converters field");
        var converters = (Dictionary<Type, Func<string, object>>)(field.GetValue(service) ?? throw new InvalidOperationException("Converters field is null"));
        
        var assembly = Assembly.GetExecutingAssembly();
        var methods = assembly.GetTypes()
                .SelectMany(x => x.GetMethods()) // Get all methods
                .Where(x => x.ReturnParameter.ParameterType.IsSubclassOf(typeof(Enum))) // Filter out non-enum methods
                .Where(x => x.GetParameters().Select(y => y.ParameterType).SequenceEqual(TargetSignature)); // Filter out methods with invalid signatures

        foreach (var method in methods)
        {
            var target = method.ReturnParameter.ParameterType;
            Func<string, object> converter = s => method.Invoke(null, new object[] { s }) ?? throw new InvalidOperationException("Converter returned null");
            converters[target] = converter;
        }
    }
}