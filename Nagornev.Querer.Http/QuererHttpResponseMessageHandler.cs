using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

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

            public ContentHandler SetContent(Func<HttpResponseMessage, TContentType> content)
            {
                _content = content;

                return this;
            }

            public ContentHandler SetConfirmation(Func<TContentType, bool> confirmation)
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

        public class Invoker
        {
            private IInvokerOptions _options;

            public Invoker(IInvokerOptions options)
            {
                _options = options;
            }

            public void Invoke(IEnumerable<Handler> handlers, HttpResponseMessage response)
            {
                foreach (Handler handler in handlers)
                {
                    _options.Logger?.Inform($"The handler '{handler.GetType().Name}' started handling response ({response.RequestMessage.RequestUri}).");

                    if (!Handle(handler, response, out Exception exception))
                    {
                        var failure = new QuererHttpExceptionHandling(handler.GetType(), exception);

                        _options.Logger?.Error(failure, ex =>
                        {
                            string message = $"Failure handling by the '{ex.Name}' handler ({response.RequestMessage.RequestUri}).";

                            return ex.InnerException == null ?
                                        message :
                                        $"{message} {ex.InnerException.GetType().Name}: {ex.InnerException.Message}";
                        });

                        switch (_options.Failure)
                        {
                            case null:
                                throw failure;

                            default:
                                _options.Failure(response, failure);
                                return;
                        }
                    }

                    _options.Logger?.Inform($"The handler '{handler.GetType().Name}' completed handling response ({response.RequestMessage.RequestUri}).");
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

        public interface IInvokerOptions
        {
            IQuererLogger Logger { get; }

            Action<HttpResponseMessage, QuererHttpExceptionHandling> Failure { get; } 
        }

        private class InvokerOptions : IInvokerOptions
        {
            public InvokerOptions(IQuererLogger logger,
                                  Action<HttpResponseMessage, QuererHttpExceptionHandling> failure)
            {
                Logger = logger;
                Failure = failure;
            }

            public IQuererLogger Logger { get; private set; }

            public Action<HttpResponseMessage, QuererHttpExceptionHandling> Failure { get; private set; }

        }

        public class InvokerOptionsBuilder 
        {
            private readonly Action<TContentType> _content;

            private Action<HttpResponseMessage, QuererHttpExceptionHandling> _failure;

            private IQuererLogger _logger;

            internal InvokerOptionsBuilder(Action<TContentType> content)
            {
                _content = content;
            }

            public InvokerOptionsBuilder SetFailure(Func<HttpResponseMessage, QuererHttpExceptionHandling, TContentType> failure)
            {
                _failure = (response, exception) =>
                {
                    TContentType content = failure.Invoke(response, exception);
                    _content.Invoke(content);
                };

                return this;
            }

            public InvokerOptionsBuilder SetLogger(IQuererLogger logger)
            {
                _logger = logger;

                return this;
            }

            internal IInvokerOptions Build()
            {
                return new InvokerOptions(_logger, _failure);
            }
        }

        #endregion
    }
}
