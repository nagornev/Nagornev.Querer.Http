using System;
using System.Collections.Generic;

namespace Nagornev.Querer.Http.Loggers
{
    public class QuererConsoleLogger : IQuererLogger
    {
        private enum LogType
        {
            Inform,
            Warn,
            Error
        }

        private readonly Dictionary<LogType, ConsoleColor> _logColors = new Dictionary<LogType, ConsoleColor>()
        {
            { LogType.Inform, ConsoleColor.Green},
            { LogType.Warn, ConsoleColor.Yellow},
            { LogType.Error, ConsoleColor.Red},
        };

        public void Inform(string message)
        {
            Log(LogType.Inform, message);
        }

        public void Warn(string message)
        {
            Log(LogType.Warn, message);
        }

        public void Error<TExceptionType>(TExceptionType exception, Func<TExceptionType, string> message)
            where TExceptionType : Exception
        {
            Log(LogType.Error, $"{message.Invoke(exception)}\n{exception.StackTrace}");
        }

        private void Log(LogType log, string message)
        {
            Console.Write($"[{DateTime.Now}] - ");
            Console.ForegroundColor = _logColors[log];
            Console.Write($"[{log.ToString().ToUpper()}]");
            Console.ResetColor();
            Console.Write($": {message}\n");
        }
    }
}
