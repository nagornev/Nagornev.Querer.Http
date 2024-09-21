using System;
using System.Collections.Generic;

namespace Nagornev.Querer.Http.Loggers
{
    public class QuererLoggerBuilder
    {
        private List<IQuererLogger> _loggers;

        internal QuererLoggerBuilder()
        {
            _loggers = new List<IQuererLogger>();
        }

        public QuererLoggerBuilder AddConsole()
        {
            _loggers.Add(new QuererConsoleLogger());

            return this;
        }

        public QuererLoggerBuilder AddFile(string path)
        {
            _loggers.Add(new QuererFileLogger(path));

            return this;
        }

        public QuererLoggerBuilder AddLogger(IQuererLogger logger)
        {
            _loggers.Add(logger);

            return this;
        }

        public QuererLoggerBuilder AddLoggers(params IQuererLogger[] loggers)
        {
            _loggers.AddRange(loggers);

            return this;
        }

        public QuererLoggerBuilder AddCallback(Action<string> inform,
                                               Action<string> warn,
                                               Action<Exception, Func<Exception, string>> error)
        {
            _loggers.Add(new QuererCallbackLogger(inform, warn, error));

            return this;
        }

        internal IQuererLogger Build()
        {
            return new QuererLoggers(_loggers);
        }
    }
}
