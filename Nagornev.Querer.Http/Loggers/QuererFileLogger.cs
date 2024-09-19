using System;
using System.IO;

namespace Nagornev.Querer.Http.Loggers
{
    internal class QuererFileLogger : IQuererLogger
    {
        private enum LogType
        {
            Inform,
            Warn,
            Error
        }

        private string _path;

        public QuererFileLogger(string path)
        {
            _path = path;
        }

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
            Log(LogType.Error, message.Invoke(exception));
        }

        private void Log(LogType log, string message)
        {
            using (StreamWriter writer = new StreamWriter(_path, true))
            {
                writer.WriteLine($"[{DateTime.Now}] - [{log.ToString().ToUpper()}]: {message}");
            }
        }
    }
}
