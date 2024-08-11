using System;
using System.Collections.Generic;
using static Nagornev.Querer.Http.QuererHttpRequestMessageCompiler;

namespace Nagornev.Querer.Http
{
    public class QuererHttpRequestMessageCompilerBuilder
    {
        private Action<MethodCompiler> _method;

        private Action<UrlCompiler> _url;

        private Action<ContentCompiler> _content;

        private Action<HeadersCompiler> _headers;

        private Action<RequestCompiler> _request;

        private Func<Scheme, IEnumerable<Scheme.Set>> _scheme;

        private QuererHttpRequestMessageCompilerBuilder()
        {
        }

        #region Builder

        public QuererHttpRequestMessageCompilerBuilder UseMethod(Action<MethodCompiler> compiler)
        {
            _method = compiler;

            return this;
        }

        public QuererHttpRequestMessageCompilerBuilder UseUrl(Action<UrlCompiler> compiler)
        {
            _url = compiler;

            return this;
        }

        public QuererHttpRequestMessageCompilerBuilder UseContent(Action<ContentCompiler> compiler)
        {
            _content = compiler;

            return this;
        }

        public QuererHttpRequestMessageCompilerBuilder UseHeaders(Action<HeadersCompiler> compiler)
        {
            _headers = compiler;

            return this;
        }

        public QuererHttpRequestMessageCompilerBuilder UseRequest(Action<RequestCompiler> compiler)
        {
            _request = compiler;

            return this;
        }

        public QuererHttpRequestMessageCompilerBuilder UseScheme(Func<Scheme, IEnumerable<Scheme.Set>> scheme)
        {
            _scheme = scheme;

            return this;
        }

        public QuererHttpRequestMessageCompiler Build()
        {
            if (_method is null ||
                _url is null)
                throw new ArgumentNullException(string.Empty, "The method or URL can`t be null.");

            return new QuererHttpRequestMessageCompilerAction(_method, _url, _content, _headers, _request, _scheme);
        }

        #endregion

        public static QuererHttpRequestMessageCompilerBuilder Create()
        {
            return new QuererHttpRequestMessageCompilerBuilder();
        }

        private class QuererHttpRequestMessageCompilerAction : QuererHttpRequestMessageCompiler
        {
            private Action<MethodCompiler> _method;

            private Action<UrlCompiler> _url;

            private Action<ContentCompiler> _content;

            private Action<HeadersCompiler> _headers;

            private Action<RequestCompiler> _request;

            private Func<Scheme, IEnumerable<Scheme.Set>> _scheme;

            public QuererHttpRequestMessageCompilerAction(Action<MethodCompiler> method,
                                                          Action<UrlCompiler> url,
                                                          Action<ContentCompiler> content,
                                                          Action<HeadersCompiler> headers,
                                                          Action<RequestCompiler> request,
                                                          Func<Scheme, IEnumerable<Scheme.Set>> scheme)
            {
                _method = method;
                _url = url;
                _content = content;
                _headers = headers;
                _request = request;
                _scheme = scheme;
            }

            protected override void SetMethod(MethodCompiler compiler)
            {
                _method.Invoke(compiler);
            }

            protected override void SetUrl(UrlCompiler compiler)
            {
                _url.Invoke(compiler);
            }

            protected override void SetContent(ContentCompiler compiler)
            {
                _content?.Invoke(compiler);
            }

            protected override void SetHeaders(HeadersCompiler compiler)
            {
                _headers?.Invoke(compiler);
            }

            protected override void SetRequest(RequestCompiler compiler)
            {
                _request?.Invoke(compiler);
            }

            protected override IEnumerable<Scheme.Set> SetScheme(Scheme scheme)
            {
                return _scheme is null ? base.SetScheme(scheme) : _scheme.Invoke(scheme);
            }
        }
    }
}
