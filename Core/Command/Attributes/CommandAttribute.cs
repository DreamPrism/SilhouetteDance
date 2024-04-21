using System.Text.RegularExpressions;

namespace SilhouetteDance.Core.Command.Attributes;

[AttributeUsage(AttributeTargets.Method)]
internal class CommandAttribute : Attribute
{
    public CommandOptions Option { get; set; }
    public string StringInvoke { get; }
    public Regex RegexInvoke { get; }
    public int Priority { get; }
    
    public CommandAttribute(string invoke, int priority = 0, CommandOptions option = CommandOptions.StartWith)
    {
        Option = option;
        StringInvoke = invoke;
        Priority = priority;
        
        if (option == CommandOptions.Regex) RegexInvoke = new Regex(invoke, RegexOptions.Compiled);
    }
}