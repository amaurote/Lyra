using static System.Enum;

namespace Lyra;

public static class Logger
{
    private static readonly string LogFilePath = Path.Combine(LyraDataDirectory.GetDataDirectory(), "log.txt");
    private const long MaxLogFileSize = 5 * 1024 * 1024; // 5 MB max log size

    private static readonly Lock Lock = new();

    private static LogStrategy _currentStrategy = LogStrategy.File;
    private static bool _debugMode;

    private static string _lastLoggedMessage = string.Empty;

    public static void Log(string message, LogLevel level = LogLevel.Info, bool excludeTimestamp = false)
    {
        TryParse<LogLevelInternal>(level.ToString(), out var logInternal);
        LogInternal(message, logInternal, excludeTimestamp, false);
    }

    public static void LogDebug(string message, bool preventRepeat = false, bool excludeTimestamp = false)
    {
        if (_debugMode)
            LogInternal(message, LogLevelInternal.Debug, preventRepeat, excludeTimestamp);
    }

    private static void LogInternal(string message, LogLevelInternal level, bool preventRepeat, bool excludeTimestamp)
    {
        if (_currentStrategy == LogStrategy.Disabled)
            return;

        lock (Lock) // Ensure thread safety
        {
            if (preventRepeat && _lastLoggedMessage == message)
                return;
            else
                _lastLoggedMessage = message;

            var logEntry = excludeTimestamp
                ? $"[{level}] {message}"
                : $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            
            if (_currentStrategy is LogStrategy.Console or LogStrategy.Both)
            {
                Console.ForegroundColor = level switch
                {
                    LogLevelInternal.Info => ConsoleColor.White,
                    LogLevelInternal.Warn => ConsoleColor.Yellow,
                    LogLevelInternal.Error => ConsoleColor.Red,
                    LogLevelInternal.Debug => ConsoleColor.DarkGreen,
                    _ => ConsoleColor.Gray
                };

                Console.WriteLine(logEntry);
                Console.ResetColor();
            }

            if (_currentStrategy is LogStrategy.File or LogStrategy.Both)
            {
                try
                {
                    if (File.Exists(LogFilePath) && new FileInfo(LogFilePath).Length > MaxLogFileSize)
                    {
                        File.WriteAllText(LogFilePath, string.Empty); // Truncate if too large
                    }

                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[Logger] Failed to write to log: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }
    }

    public static void ClearLog()
    {
        lock (Lock)
        {
            try
            {
                File.WriteAllText(LogFilePath, string.Empty);
                Console.WriteLine("[Logger] Log file cleared.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logger] Failed to clear log: {ex.Message}");
            }
        }
    }

    public static void SetLogStrategy(LogStrategy strategy)
    {
        _currentStrategy = strategy;
    }

    public static void SetLogDebugMode(bool debugMode)
    {
        _debugMode = debugMode;
    }

    public enum LogStrategy
    {
        Disabled,
        Console,
        File,
        Both
    }

    public enum LogLevel
    {
        Info,
        Warn,
        Error
    }

    private enum LogLevelInternal
    {
        Info,
        Warn,
        Error,
        Debug
    }
}