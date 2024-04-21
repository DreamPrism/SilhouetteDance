using System.Reflection;
using System.Text.RegularExpressions;
using SilhouetteDance.Core.Command.Attributes;
using SilhouetteDance.Function;

namespace SilhouetteDance.Core;

/// <summary>
/// Metadata Context, access to this context to get the runtime metadata such as MessageReceived
/// </summary>
public class MetadataContext
{
    
    public DateTime StartTime { get; } = DateTime.Now;
    
    public Dictionary<string, CommandMetadata> CommandMetadatas { get; set; }
    
    public MetadataContext()
    {
        CommandMetadatas = new Dictionary<string, CommandMetadata>();
        
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(FunctionBase))).ToArray();
        foreach (var type in types)
        {
            foreach (var method in type.GetMethods())
            {
                var attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute == null) continue;
                
                switch (attribute.Option) // make lint happy
                {
                    case CommandOptions.Regex:
                        if (attribute.RegexInvoke == null) continue;
                        CommandMetadatas[attribute.RegexInvoke.ToString()] = new CommandMetadata(type, attribute.RegexInvoke.ToString());
                        break;
                    case CommandOptions.StartWith:
                        CommandMetadatas[attribute.StringInvoke] = new CommandMetadata(type, attribute.StringInvoke);
                        break;
                }
            }
        }
    }

    public void Count(string command)
    {
        CommandMetadatas[command].Count++;
    }
    
    public void Count(Regex command)
    {
        CommandMetadatas[command.ToString()].Count++;
    }
}

public class CommandMetadata
{
    public CommandMetadata(Type module, string command)
    {
        Module = module;
        Command = command;
    }

    public Type Module { get; set; }
    public string Command { get; set; }
    public int Count { get; set; }
}