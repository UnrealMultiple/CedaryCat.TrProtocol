namespace Dimensions;

public enum LogLevel
{
    INFO,
    WARNING,
    ERROR,
    DEBUG
}

public static class Logger
{

    public static void Log(string type, LogLevel level, string message)
    {
        var now = DateTime.Now;
        string timestamp = now.ToString("MM-dd HH:mm:ss");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(timestamp);

        Console.ResetColor();
        Console.Write(" [");
        switch (level)
        {
            case LogLevel.INFO:
                Console.ForegroundColor = ConsoleColor.White;
                break;
            case LogLevel.WARNING:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case LogLevel.ERROR:
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case LogLevel.DEBUG:
                Console.ResetColor();
                break;
        }
        Console.Write(level);
        Console.ResetColor();
        Console.Write("] ");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(type);
        Console.ResetColor();

        Console.Write(" | ");
        Console.WriteLine(message);
    }
}


