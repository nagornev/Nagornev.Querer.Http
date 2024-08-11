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
            IEnumerable<Handler> handlers = GetHandlers();

            foreach (Handler handler in handlers)
            {
                if (!handler.Handle(response))
                    throw new Exception($"{handler.GetType().FullName}. Invalid handling.");
            }
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
    }
}
