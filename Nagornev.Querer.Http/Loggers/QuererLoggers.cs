using System;
using System.Collections.Generic;

namespace Nagornev.Querer.Http.Loggers
{
    public class QuererLoggers : IQuererLogger
    {
        private readonly IReadOnlyCollection<IQuererLogger> _loggers;

        public QuererLoggers(params IQuererLogger[] loggers)
        {
            _loggers = loggers;
        }

        public void Inform(string message)
        {
            foreach (IQuererLogger logger in _loggers)
            {
                logger.Inform(message);
            }
        }

        public void Warn(string message)
        {
            foreach (IQuererLogger logger in _loggers)
            {
                logger.Warn(message);
            }
        }

        public void Error<TExceptionType>(TExceptionType exception, Func<TExceptionType, string> message)
            where TExceptionType : Exception
        {
            foreach (IQuererLogger logger in _loggers)
            {
                logger.Error(exception, message);
            }
        }
    }
}
