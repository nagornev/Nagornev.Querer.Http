using System;

namespace Nagornev.Querer.Http.Loggers
{
    public class QuererCallbackLogger : IQuererLogger
    {
        private Action<string> _inform;

        private Action<string> _warn;

        private Action<Exception, Func<Exception, string>> _error;

        public QuererCallbackLogger(Action<string> inform, Action<string> warn, Action<Exception, Func<Exception, string>> error) 
        {
            _inform = inform;
            _warn = warn;
            _error = error;
        }

        public void Inform(string message)
        {
            _inform.Invoke(message);
        }

        public void Warn(string message)
        {
            _warn.Invoke(message);
        }

        public void Error<TExceptionType>(TExceptionType exception, Func<TExceptionType, string> message) 
            where TExceptionType : Exception
        {
            _error.Invoke(exception, (Func<Exception, string>)message);
        }
    }
}
