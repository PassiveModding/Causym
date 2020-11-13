using System;
using Disqord.Logging;

namespace Causym
{
    public sealed class Logger : ILogger
    {
        public Logger(LogSeverity minLogLevel = LogSeverity.Trace)
        {
            this.minLogLevel = minLogLevel;
        }
        public event EventHandler<LogEventArgs> Logged;

        private readonly object _lock = new object();
        private readonly LogSeverity minLogLevel;

        public void Log(string source, string message, LogSeverity severity, Exception exception = null)
        {
            Log(this, new LogEventArgs(source, severity, message, exception));
        }

        public void Log(object sender, LogEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (e.Severity < minLogLevel)
            {
                return;
            }

            lock (_lock)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = GetSeverityColor(e.Severity);
                Console.WriteLine(e);
                Console.ForegroundColor = oldColor;
            }

            var handlers = Logged?.GetInvocationList();
            if (handlers == null)
                return;

            for (var i = 0; i < handlers.Length; i++)
            {
                var handler = handlers[i] as EventHandler<LogEventArgs>;
                try
                {
                    handler(sender, e);
                }
                catch (Exception ex)
                {
                    lock (_lock)
                    {
                        var oldColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"An exception occurred in one of the logging handlers.\n{ex}");
                        Console.ForegroundColor = oldColor;
                    }
                }
            }
        }

        public static ConsoleColor GetSeverityColor(LogSeverity severity) => severity switch
        {
            LogSeverity.Critical => ConsoleColor.DarkRed,
            LogSeverity.Error => ConsoleColor.Red,
            LogSeverity.Warning => ConsoleColor.Yellow,
            LogSeverity.Information => ConsoleColor.Green,
            LogSeverity.Debug => ConsoleColor.Cyan,
            LogSeverity.Trace => ConsoleColor.White,
            _ => throw new ArgumentException("Unknown log level type.", nameof(severity)),
        };

        public void Dispose()
        { }
    }
}