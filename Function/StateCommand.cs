using System.Diagnostics;
using System.Text;
using Lagrange.Core;
using SilhouetteDance.Core.Command.Attributes;
using SilhouetteDance.Core.Message;
using SilhouetteDance.Core.Message.Entities;

namespace SilhouetteDance.Function;

public class StateCommand : FunctionBase
{
    private static readonly DateTime StartTime = DateTime.Now;

    public StateCommand(ResContext resContext) : base(resContext)
    {
    }

    [Command("state")]
    public static async Task<MessageStruct> GetState()
    {
        var process = Process.GetCurrentProcess();
        var startTime = DateTime.Now;
        var startCpuTime = process.TotalProcessorTime;
        await Task.Delay(1000);
        var endTime = DateTime.Now;
        var endCpuTime = process.TotalProcessorTime;
        var cpuUsedTime = (endCpuTime - startCpuTime).TotalMilliseconds;
        var totalTime = (endTime - startTime).TotalMilliseconds;
        var cpuUsage = cpuUsedTime / totalTime * 100;
        var processRamUsage = process.WorkingSet64 / (1024 * 1024.0);

        var builder = new StringBuilder()
            .AppendLine("当前状态:")
            .AppendLine($".NET版本: {Environment.Version}")
            .AppendLine($"已运行时间: {FormatTimeSpan(DateTime.Now - StartTime)}")
            .AppendLine($"CPU: {cpuUsage:F1}%")
            .AppendLine($"RAM: {processRamUsage:F1}MB");

        return new MessageStruct
        {
            new TextEntity(builder.ToString())
        };
    }

    private static string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
        {
            var totalDays = (int)timeSpan.TotalDays;
            var remainingHours = timeSpan.Hours;
            var remainingMinutes = timeSpan.Minutes;
            var remainingSeconds = timeSpan.Seconds;
            return $"{totalDays}天{remainingHours}时{remainingMinutes}分{remainingSeconds}秒";
        }

        if (timeSpan.TotalHours >= 1)
        {
            var totalHours = (int)timeSpan.TotalHours;
            var remainingMinutes = timeSpan.Minutes;
            var remainingSeconds = timeSpan.Seconds;
            return $"{totalHours}时{remainingMinutes}分{remainingSeconds}秒";
        }

        if (timeSpan.TotalMinutes >= 1)
        {
            var totalMinutes = (int)timeSpan.TotalMinutes;
            var remainingSeconds = timeSpan.Seconds;
            return $"{totalMinutes}分{remainingSeconds}秒";
        }

        return $"{timeSpan.TotalSeconds:F1}秒";
    }
}