using RealmNexus.Models;

namespace RealmNexus.Logging;

public interface ILogger
{
    LogLevel MinimumLevel { get; set; }
    void LogDebug(string tag, string message);
    void LogInfo(string tag, string message);
    void LogWarning(string tag, string message);
    void LogError(string tag, string message);

    static event Action<string, LogLevel> OnLogger;
}

public class ConsoleLogger : ILogger
{
    private const string Reset = "\x1b[0m";
    private const string Gray = "\x1b[90m";
    private const string Green = "\x1b[32m";
    private const string Yellow = "\x1b[33m";
    private const string Red = "\x1b[31m";
    private const string Blue = "\x1b[34m";

    public static event Action<string, LogLevel> OnLogger;

    public LogLevel MinimumLevel { get; set; } = Config.Instance.LogLevel;

    private static string GetPrefix(string levelColor, string level, string tag)
    {
        if (string.IsNullOrEmpty(tag))
            return $"{levelColor}[{level}]{Reset}";
        return $"{levelColor}[{level}]{Reset} {Blue}[{tag}]{Reset}";
    }

    public void LogDebug(string tag, string message)
    {
        if (MinimumLevel <= LogLevel.Debug)
            OnLogger?.Invoke($"{GetPrefix(Gray, "DBG", tag)} {message}", LogLevel.Debug);
    }

    public void LogInfo(string tag, string message)
    {
        if (MinimumLevel <= LogLevel.Info)
            OnLogger?.Invoke($"{GetPrefix(Green, "INF", tag)} {message}", LogLevel.Info);
    }

    public void LogWarning(string tag, string message)
    {
        if (MinimumLevel <= LogLevel.Warning)
            OnLogger?.Invoke($"{GetPrefix(Yellow, "WRN", tag)} {message}", LogLevel.Warning);
    }

    public void LogError(string tag, string message)
    {
        if (MinimumLevel <= LogLevel.Error)
            OnLogger?.Invoke($"{GetPrefix(Red, "ERR", tag)} {message}", LogLevel.Error);
    }
}
