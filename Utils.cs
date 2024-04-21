using Lagrange.Core.Event.EventArg;

namespace SilhouetteDance;

public static class Utils
{
    private static void ChangeColorByTitle(this LogLevel level) => Console.ForegroundColor = level switch
    {
        LogLevel.Debug => ConsoleColor.White,
        LogLevel.Verbose => ConsoleColor.DarkGray,
        LogLevel.Information => ConsoleColor.Blue,
        LogLevel.Warning => ConsoleColor.Yellow,
        LogLevel.Fatal => ConsoleColor.Red,
        _ => Console.ForegroundColor
    };
    public static void Log(LogLevel level, string text)
    {
        ChangeColorByTitle(level);
        Console.WriteLine(text);
    }
}