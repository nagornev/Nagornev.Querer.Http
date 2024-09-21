using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Nagornev.Querer.Http.Extensions;
using Nagornev.Querer.Http.Loggers;

namespace Nagornev.Querer.Http
{
    public abstract class QuererHttpResponseMessageHandler<TContentType> : QuererResponseHandler<HttpResponseMessage>
    {
        public TContentType Content { get; private set; }

        protected override void Handle(HttpResponseMessage response)
        {
            Invoker invoker = GetInvoker();

            IEnumerable<Handler> handlers = GetHandlers();

            invoker.Invoke(handlers, response);
        }

        private Invoker GetInvoker()
        {
            InvokerOptionsBuilder options = new InvokerOptionsBuilder((content) => Content = content);

            Configure(options);

            return new Invoker(options.Build());
        }

        private IEnumerable<Handler> GetHandlers()
        {
            Scheme scheme = GetScheme();

            IEnumerable<Scheme.Set> sets = SetScheme(scheme);

            foreach (Scheme.Set set in sets)
            {
                set.Configuration.Invoke();
            }

            return sets.Select(x => x.Handler);
        }

        private Scheme GetScheme()
        {
            PreviewHandler previewHandler = new PreviewHandler();
            ContentHandler contentHandler = new ContentHandler(content => Content = content);

            return new Scheme(new Scheme.Set(previewHandler, () => SetPreview(previewHandler)),
                              new Scheme.Set(contentHandler, () => SetContent(contentHandler)));
        }

        protected virtual void Configure(InvokerOptionsBuilder options)
        {
        }

        protected virtual void SetPreview(PreviewHandler handler)
        {
            handler.Set(response => (int)response.StatusCode >= 200 &&
                                    (int)response.StatusCode <= 299);
        }

        protected abstract void SetContent(ContentHandler handler);

        protected virtual IEnumerable<Scheme.Set> SetScheme(Scheme scheme)
        {
            return scheme.Default();
        }

        #region Handlers

        public abstract class Handler
        {
            internal abstract bool Handle(HttpResponseMessage response);
        }

        public sealed class PreviewHandler : Handler
        {
            private List<Func<HttpResponseMessage, bool>> _previews;

            internal PreviewHandler()
            {
                _previews = new List<Func<HttpResponseMessage, bool>>();
            }

            internal override bool Handle(HttpResponseMessage response)
            {
                foreach (Func<HttpResponseMessage, bool> preview in _previews)
                {
                    if (!preview.Invoke(response))
                        return false;
                }

                return true;
            }

            public PreviewHandler Set(Func<HttpResponseMessage, bool> preview)
            {
                _previews.Add(preview);

                return this;
            }
        }

        public sealed class ContentHandler : Handler
        {
            private Func<HttpResponseMessage, TContentType> _content;

            private Func<TContentType, bool> _confirmation;

            private Action<TContentType> _callback;

            internal ContentHandler(Action<TContentType> callback)
            {
                _callback = callback;
            }

            internal override bool Handle(HttpResponseMessage response)
            {
                TContentType content = _content.Invoke(response);

                _callback?.Invoke(content);

                return _confirmation?.Invoke(content) ?? content != null;
            }

            public ContentHandler Set(Func<HttpResponseMessage, TContentType> content)
            {
                _content = content;

                return this;
            }

            public ContentHandler Confirm(Func<TContentType, bool> confirmation)
            {
                _confirmation = confirmation;

                return this;
            }
        }

        #endregion

        #region Scheme

        public class Scheme
        {
            public Scheme(Set preview,
                          Set content)
            {
                Preview = preview;
                Content = content;
            }

            public Set Preview { get; private set; }

            public Set Content { get; private set; }

            public IEnumerable<Set> Configure(params Set[] sets)
            {
                return new HashSet<Set>(sets);
            }

            public IEnumerable<Set> Default()
            {
                return Configure(Preview, Content);
            }

            public class Set
            {
                public Set(Handler handler, Action configuration)
                {
                    Handler = handler;
                    Configuration = configuration;
                }

                internal Handler Handler { get; private set; }

                internal Action Configuration { get; private set; }
            }
        }

        #endregion

        #region Invoker

        internal class Invoker
        {
            private IInvokerOptions _options;

            public Invoker(IInvokerOptions options)
            {
                _options = options;
            }

            /// <summary>
            /// Start handling the response.
            /// </summary>
            /// <param name="handlers"></param>
            /// <param name="response"></param>
            /// <exception cref="InvalidOperationException"></exception>
            public void Invoke(IEnumerable<Handler> handlers, HttpResponseMessage response)
            {
                foreach (Handler handler in handlers)
                {
                    _options.Logger?.Inform($"Start of handling the '{handler.GetType().Name}' handler ({response.RequestMessage.RequestUri}).");

                    if (!Handle(handler, response, out Exception exception))
                    {
                        var failure = exception is null ?
                                        new InvalidOperationException($"Failed handling by the '{handler.GetType().Name}' handler.") :
                                        exception;

                        _options.Logger?.Error(failure, ex => exception is null? 
                                                                $"{ex.GetType().Name}: {ex.Message}":
                                                                $"{ex.GetType().Name}: Failed handling by the '{handler.GetType().Name}' handler.");

                        _options.Failure(response, failure);

                        return;
                    }

                    _options.Logger?.Inform($"Successful handling by the '{handler.GetType().Name}' handler ({response.RequestMessage.RequestUri}).");
                }
            }

            private bool Handle(Handler handler, HttpResponseMessage response, out Exception catchedException)
            {
                bool result = false;

                try
                {
                    result = handler.Handle(response);
                    catchedException = default;
                }
                catch (Exception exception)
                {
                    catchedException = exception;
                }

                return result;
            }
        }

        internal interface IInvokerOptions
        {
            IQuererLogger Logger { get; }

            void Failure<T>(HttpResponseMessage response, T exception) 
                where T : Exception;
        }

        internal class InvokerOptions : IInvokerOptions
        {
            public IDictionary<Type, Action<HttpResponseMessage, Exception>> _failures { get; private set; }


            public InvokerOptions(IDictionary<Type, Action<HttpResponseMessage, Exception>> failures, 
                                  IQuererLogger logger)
            {
                _failures = failures;
                Logger = logger;
            }

            public IQuererLogger Logger { get; private set; }

            public void Failure<T>(HttpResponseMessage response, T exception) 
                where T : Exception
            {
                Type catchedFailure = exception.GetType();

                if (_failures.TryGetValue(catchedFailure, out Action<HttpResponseMessage, Exception> direct) |
                    _failures.TryGetValue((fail) => catchedFailure.IsSubclassOf(fail.Key), out Action<HttpResponseMessage, Exception> indirect))
                {
                    (direct ?? indirect).Invoke(response, exception);
                    return;
                }

                throw exception;
            }
        }

        public class InvokerOptionsBuilder
        {
            public class FailureBuilder
            {
                private readonly Action<TContentType> _content;

                private IDictionary<Type, Action<HttpResponseMessage, Exception>> _failures;

                internal FailureBuilder(Action<TContentType> content)
                {
                    _content = content;
                    _failures = new Dictionary<Type, Action<HttpResponseMessage, Exception>>();
                }

                public FailureBuilder AddFailure<T>(Action<HttpResponseMessage, T> failure)
                    where T : Exception
                {
                    _failures.Add(typeof(T), (Action<HttpResponseMessage, Exception>)failure);

                    return this;
                }

                public FailureBuilder AddFailure<T>(Func<HttpResponseMessage, T, TContentType> failure)
                    where T : Exception
                {
                    _failures.Add(typeof(T), (response, exception) => 
                    {
                        TContentType content = failure.Invoke(response, (T)exception);

                        _content.Invoke(content);
                    });

                    return this;
                }

                public IDictionary<Type, Action<HttpResponseMessage, Exception>> Build()
                {
                    return _failures;
                }
            }

            private FailureBuilder _failureBuilder;

            private QuererLoggerBuilder _loggerBuilder;

            internal InvokerOptionsBuilder(Action<TContentType> content)
            {
                _failureBuilder = new FailureBuilder(content);
                _loggerBuilder = new QuererLoggerBuilder();
            }

            public InvokerOptionsBuilder SetFailure(Action<FailureBuilder> options)
            {
                options.Invoke(_failureBuilder);

                return this;
            }

            public InvokerOptionsBuilder SetLogger(Action<QuererLoggerBuilder> options)
            {
                options.Invoke(_loggerBuilder);

                return this;
            }

            internal IInvokerOptions Build()
            {
                return new InvokerOptions(_failureBuilder.Build(),
                                          _loggerBuilder.Build());
            }
        }

        #endregion
    }
}
