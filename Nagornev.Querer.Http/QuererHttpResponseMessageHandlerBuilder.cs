﻿using System;
using System.Collections.Generic;

namespace Nagornev.Querer.Http
{
    public class QuererHttpResponseMessageHandlerBuilder<TContentType>
    {
        private Action<QuererHttpResponseMessageHandler<TContentType>.PreviewHandler> _preview;

        private Action<QuererHttpResponseMessageHandler<TContentType>.ContentHandler> _content;

        private Func<QuererHttpResponseMessageHandler<TContentType>.Scheme, IEnumerable<QuererHttpResponseMessageHandler<TContentType>.Scheme.Set>> _scheme;

        private QuererHttpResponseMessageHandlerBuilder()
        {
        }

        public static QuererHttpResponseMessageHandlerBuilder<TContentType> Create()
        {
            return new QuererHttpResponseMessageHandlerBuilder<TContentType>();
        }

        public QuererHttpResponseMessageHandlerBuilder<TContentType> UsePreview(Action<QuererHttpResponseMessageHandler<TContentType>.PreviewHandler> handler)
        {
            _preview = handler;

            return this;
        }

        public QuererHttpResponseMessageHandlerBuilder<TContentType> UseContent(Action<QuererHttpResponseMessageHandler<TContentType>.ContentHandler> handler)
        {
            _content = handler;

            return this;
        }

        public QuererHttpResponseMessageHandlerBuilder<TContentType> UseScheme(Func<QuererHttpResponseMessageHandler<TContentType>.Scheme, IEnumerable<QuererHttpResponseMessageHandler<TContentType>.Scheme.Set>> scheme)
        {
            _scheme = scheme;

            return this;
        }

        public QuererHttpResponseMessageHandler<TContentType> Build()
        {
            if (_content is null)
                throw new ArgumentNullException(string.Empty, "The content can`t be null.");

            return new QuererHttpResponseMessageHandlerAction(_preview, _content, _scheme);
        }

        private class QuererHttpResponseMessageHandlerAction : QuererHttpResponseMessageHandler<TContentType>
        {
            private Action<PreviewHandler> _preview;

            private Action<ContentHandler> _content;

            private Func<Scheme, IEnumerable<Scheme.Set>> _scheme;

            public QuererHttpResponseMessageHandlerAction(Action<PreviewHandler> preview,
                                                          Action<ContentHandler> content,
                                                          Func<Scheme, IEnumerable<Scheme.Set>> scheme)
            {
                _preview = preview;
                _content = content;
                _scheme = scheme;
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
                if (_scheme is null)
                    return base.SetScheme(scheme);
                else
                    return _scheme.Invoke(scheme);
            }
        }

    }
}