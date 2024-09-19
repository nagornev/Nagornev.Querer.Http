using System;
using System.Collections.Generic;

namespace Nagornev.Querer.Http
{
    public class QuererHttpResponsesMessageHandlerBuilder<TContentType>
    {
        private Action<QuererHttpResponsesMessageHandler<TContentType>.InvokerOptionsBuilder> _configure;

        private Action<QuererHttpResponsesMessageHandler<TContentType>.PreviewHandler> _preview;

        private Action<QuererHttpResponsesMessageHandler<TContentType>.ContentHandler> _content;

        private Func<QuererHttpResponsesMessageHandler<TContentType>.Scheme, IEnumerable<QuererHttpResponsesMessageHandler<TContentType>.Scheme.Set>> _scheme;

        private QuererHttpResponsesMessageHandlerBuilder()
        {
        }

        public static QuererHttpResponsesMessageHandlerBuilder<TContentType> Create()
        {
            return new QuererHttpResponsesMessageHandlerBuilder<TContentType>();
        }

        public QuererHttpResponsesMessageHandlerBuilder<TContentType> UseConfigure(Action<QuererHttpResponsesMessageHandler<TContentType>.InvokerOptionsBuilder> configure)
        {
            _configure = configure;

            return this;
        }

        public QuererHttpResponsesMessageHandlerBuilder<TContentType> UsePreview(Action<QuererHttpResponsesMessageHandler<TContentType>.PreviewHandler> handler)
        {
            _preview = handler;

            return this;
        }

        public QuererHttpResponsesMessageHandlerBuilder<TContentType> UseContent(Action<QuererHttpResponsesMessageHandler<TContentType>.ContentHandler> handler)
        {
            _content = handler;

            return this;
        }

        public QuererHttpResponsesMessageHandlerBuilder<TContentType> UseScheme(Func<QuererHttpResponsesMessageHandler<TContentType>.Scheme, IEnumerable<QuererHttpResponsesMessageHandler<TContentType>.Scheme.Set>> scheme)
        {
            _scheme = scheme;

            return this;
        }


        public QuererHttpResponsesMessageHandler<TContentType> Build()
        {
            if (_content is null)
                throw new ArgumentNullException(string.Empty, "The content can`t be null.");

            return new QuererHttpResponsesMessageHandlerAction(_configure,
                                                               _preview,
                                                               _content,
                                                               _scheme);
        }

        private class QuererHttpResponsesMessageHandlerAction : QuererHttpResponsesMessageHandler<TContentType>
        {
            private Action<InvokerOptionsBuilder> _configure;

            private Action<PreviewHandler> _preview;

            private Action<ContentHandler> _content;

            private Func<Scheme, IEnumerable<Scheme.Set>> _scheme;

            public QuererHttpResponsesMessageHandlerAction(Action<InvokerOptionsBuilder> configure,
                                                           Action<PreviewHandler> preview,
                                                           Action<ContentHandler> content,
                                                           Func<Scheme, IEnumerable<Scheme.Set>> scheme)
            {
                _configure = configure;
                _preview = preview;
                _content = content;
                _scheme = scheme;
            }

            protected override void Configure(InvokerOptionsBuilder options)
            {
                if (_configure is null)
                    base.Configure(options);
                else
                    _configure.Invoke(options);
            }

            protected override void SetPreview(PreviewHandler handler)
            {
                if (_preview is null)
                    base.SetPreview(handler);
                else
                    _preview.Invoke(handler);
            }

            protected override void SetContent(ContentHandler handler)
            {
                _content.Invoke(handler);
            }

            protected override IEnumerable<Scheme.Set> SetScheme(Scheme scheme)
            {
                if (scheme is null)
                    return base.SetScheme(scheme);
                else
                    return _scheme.Invoke(scheme);
            }
        }
    }
}

