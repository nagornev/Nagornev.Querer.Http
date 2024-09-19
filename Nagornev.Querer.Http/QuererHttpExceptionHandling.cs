using System;

namespace Nagornev.Querer.Http
{
    public class QuererHttpExceptionHandling : QuererHttpException
    {
        private const string _message = "Unsuccessful processing by the '{0}' handler.";

        public QuererHttpExceptionHandling(Type handler)
            : this(handler, default)
        {
        }

        public QuererHttpExceptionHandling(Type handler, Exception innerException) 
            : base(string.Format(_message, handler.Name), innerException)
        {
            Handler = handler;
        }

        public Type Handler { get; private set; }

        public string Name => Handler.Name;
    }
}
