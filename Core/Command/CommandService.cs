using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SilhouetteDance.Core.Command.Attributes;
using SilhouetteDance.Core.Message;
using SilhouetteDance.Function;
using SilhouetteDance.Utility;
using TextEntity = SilhouetteDance.Core.Message.Entities.TextEntity;

namespace SilhouetteDance.Core.Command;

internal class CommandService
{
    private readonly PriorityDictionary<string, (Type, MethodInfo, ParameterInfo[]), int> _stringCommands = new();
    private readonly PriorityDictionary<Regex, (Type, MethodInfo, ParameterInfo[]), int> _regexCommands = new();
    private readonly List<(Type, MethodInfo, ParameterInfo[])> _constCommands = new();

    private readonly IConfiguration _configuration;
    private readonly ServiceProvider _serviceProvider;

    private readonly int _maxStringCommandLength;

    private static readonly Dictionary<Type, Func<string, object>> Converters = new()
    {
        { typeof(string), x => x },
        { typeof(int), x => int.Parse(x) },
        { typeof(long), x => long.Parse(x) },
        { typeof(float), x => float.Parse(x) },
        { typeof(double), x => double.Parse(x) },
        { typeof(bool), x => bool.Parse(x) },
        { typeof(DateTime), x => DateTime.Parse(x) },
        { typeof(TimeSpan), x => TimeSpan.Parse(x) },
        { typeof(Guid), x => Guid.Parse(x) },
        { typeof(decimal), x => decimal.Parse(x) },
        { typeof(char), x => x[0] },
        { typeof(byte), x => byte.Parse(x) },
        { typeof(sbyte), x => sbyte.Parse(x) },
        { typeof(short), x => short.Parse(x) },
        { typeof(ushort), x => ushort.Parse(x) },
        { typeof(uint), x => uint.Parse(x) },
        { typeof(ulong), x => ulong.Parse(x) },
        { typeof(object), x => x },
    };

    public CommandService(IConfiguration configuration, IServiceCollection serviceCollection)
    {
        _configuration = configuration;
        _serviceProvider = serviceCollection.BuildServiceProvider();

        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(FunctionBase))).ToArray();
        foreach (var type in types)
        {
            var methods = type.GetMethods();
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute == null) continue;

                switch (attribute.Option) // make lint happy
                {
                    case CommandOptions.Regex:
                        if (attribute.RegexInvoke == null) continue;
                        _regexCommands[(attribute.RegexInvoke, attribute.Priority)] =
                            (type, method, method.GetParameters());
                        break;
                    case CommandOptions.Everything:
                        _constCommands.Add((type, method, method.GetParameters()));
                        break;
                    case CommandOptions.StartWith:
                    default:
                        _stringCommands[(attribute.StringInvoke, attribute.Priority)] =
                            (type, method, method.GetParameters());
                        break;
                }

                _maxStringCommandLength = Math.Max(_maxStringCommandLength, attribute.StringInvoke.Length);
            }
        }

        ReorderParameters(_stringCommands);
        ReorderParameters(_regexCommands);

        EnumConverter.RegisterConverter(this);
    }

    private static void ReorderParameters<T>(PriorityDictionary<T, (Type, MethodInfo, ParameterInfo[]), int> dictionary)
        where T : notnull
    {
        foreach (var (key, parameterInfos) in dictionary)
        {
            var newParameters = new ParameterInfo[parameterInfos.Item3.Length];
            var defaultParameters = new List<ParameterInfo>();
            var paramAlloc = new bool[parameterInfos.Item3.Length];

            foreach (var paramInfo in parameterInfos.Item3)
            {
                var attribute = paramInfo.GetCustomAttribute<ParameterAttribute>();
                if (attribute == null)
                {
                    defaultParameters.Add(paramInfo);
                    continue;
                }

                newParameters[attribute.Order] = paramInfo;
            }

            var index = 0;
            foreach (var defaultParameter in defaultParameters)
            {
                while (paramAlloc[index]) index++; // find a empty slot

                newParameters[index] = defaultParameter;
                paramAlloc[index] = true; // mark as allocated
            }

            dictionary[(key, dictionary.GetPriority(key))] =
                (parameterInfos.Item1, parameterInfos.Item2, newParameters);
        }
    }

    public async Task<MessageStruct> InvokeCommand(MessageStruct msg)
    {
        var prefix = _configuration["Generic:CommandPrefix"] ?? "/";
        var msgRaw = msg.GetEntity<TextEntity>()?.Text ?? "";
        if (!msgRaw.StartsWith(prefix)) return null;

        msgRaw = msgRaw[prefix.Length..]; // remove prefix
        var msgSplit = msgRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var matchTarget = msgSplit.Length > 1
            ? string.Join(' ', msgSplit.Take(_maxStringCommandLength)).Trim()
            : msgSplit[0];
        var matchPrefixLength = matchTarget.Count(x => x == ' ');

        (Type, MethodInfo, ParameterInfo[])? matched = null;
        try
        {
            matched = _stringCommands.StartwithPriority(matchTarget, out var pattern);
            matchPrefixLength = pattern.Count(x => x == ' ') + 1;
        }
        catch (KeyNotFoundException)
        {
            matched = _regexCommands.FirstOrDefault(x => x.Key.IsMatch(msgRaw)).Value;
        }
        catch
        {
            // ignored
        }

        if (matched is not { Item1: not null, Item2: not null, Item3: not null }) return null;

        var (type, method, parameters) = matched.Value;
        var args = new object[parameters.Length];

        for (int i = 0, j = 0; j < parameters.Length; i++, j++)
        {
            if (j - parameters.Count(x => x.GetCustomAttribute<MetadataAttribute>() != null) >=
                msgSplit.Length - matchPrefixLength)
            {
                if (parameters[j].HasDefaultValue)
                {
                    args[j] = parameters[j].DefaultValue;
                    continue;
                }

                return new MessageStruct
                {
                    new TextEntity(
                        $"指令\"{matchTarget}\"需要{parameters.Length}个参数,但只传入了{msgSplit.Length - matchPrefixLength}个")
                };
            }

            if (parameters[j].GetCustomAttribute<MetadataAttribute>() != null)
            {
                args[j] = parameters[j].GetCustomAttribute<MetadataAttribute>()?.Type switch
                {
                    MetadataAttribute.MetadataType.Timestamp => msg.Timestamp,
                    MetadataAttribute.MetadataType.Uin => msg.FromUin,
                    MetadataAttribute.MetadataType.MessageStruct => msg,
                    _ => null
                };
                i--; // do not count this parameter into token count
                continue;
            }

            var composite =
                Composite(i,
                    parameters); // Item1 for Index of CompositeParam, Item2 for Param Count After the attribute
            if (composite == null) // the parameter is not a composite
            {
                var rawToken = msgSplit[i + matchPrefixLength];
                var parsedToken = Converters[parameters[j].ParameterType](rawToken);
                args[j] = parsedToken;
            }
            else // the parameter is a composite
            {
                var parameter = parameters[composite.Value.Item1]; // must be list or array
                var tokenCount = msgSplit.Length - matchPrefixLength - composite.Value.Item1;
                var tokens = msgSplit.Skip(i + matchPrefixLength).Take(tokenCount).ToArray();
                i += tokenCount - 1;

                var collectionType = parameter.ParameterType.GetElementType() ?? type;
                var collection = parameter.ParameterType.IsArray
                    ? Array.CreateInstance(collectionType, tokenCount)
                    : Activator.CreateInstance(parameter.ParameterType);

                for (var k = 0; k < tokenCount; k++)
                {
                    var parsedToken = Converters[collectionType](tokens[k]);
                    switch (collection)
                    {
                        case Array array:
                            array.SetValue(parsedToken, k);
                            break;
                        case IList list:
                            list.Add(parsedToken);
                            break;
                    }
                }

                args[j] = collection;
            }
        }

        try
        {
            foreach (var (constType, constMethod, _) in _constCommands)
            {
                var instance = ActivatorUtilities.CreateInstance(_serviceProvider, constType);
                if (constMethod.ReturnType == typeof(Task) &&
                    constMethod.Invoke(instance, new object[] { matchTarget }) is Task task)
                {
                    await task;
                }
                else
                {
                    constMethod.Invoke(instance, new object[] { matchTarget });
                }
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            var instance = ActivatorUtilities.CreateInstance(_serviceProvider, type);
            var result = method.ReturnType == typeof(MessageStruct)
                ? method.Invoke(instance, args) as MessageStruct
                : await (method.Invoke(instance, args) as Task<MessageStruct>)!;
            return result;
        }
        catch (Exception)
        {
            return new MessageStruct
                { new TextEntity($"指令【{matchTarget}】执行失败") };
        }
    }

    /// <summary>
    /// To determine whether a parameter is a composite parameter, and return the index of the composite parameter and the parameter count after the parameter of composite attribute
    /// </summary>
    /// <param name="index"></param>
    /// <param name="parameters"></param>
    /// <returns>(param count before attribute/position of composite attribute, param count after attributed parameter)</returns>
    private static (int, int)? Composite(int index, IReadOnlyList<ParameterInfo> parameters)
    {
        if (index >= parameters.Count) return null;
        if (parameters.Count(x => x.GetCustomAttribute<CompositeAttribute>() != null) > 1)
        {
            throw new InvalidOperationException("Only one Composite Attribute is allowed");
        }

        var composite = parameters[index].GetCustomAttribute<CompositeAttribute>();
        if (composite == null) return null;
        return (index, parameters.Count - index - 1);
    }
}