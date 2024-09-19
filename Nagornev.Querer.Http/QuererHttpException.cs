using System;

namespace Nagornev.Querer.Http
{
    public class QuererHttpException : QuererException
    {
        public QuererHttpException()
        {
        }

        public QuererHttpException(string message) 
            : base(message)
        {
        }

        public QuererHttpException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
