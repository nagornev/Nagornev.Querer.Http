using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace Nagornev.Querer.Http
{
    public abstract class QuererHttpRequestMessageCompiler : QuererRequestCompiler<HttpRequestMessage>
    {
        protected override HttpRequestMessage Compile()
        {
            HttpRequestMessage request = new HttpRequestMessage();

            IEnumerable<Compiler> compilers = GetCompilers();

            foreach (Compiler compiler in compilers)
            {
                compiler.Compile(request);
            }

            return request;
        }

        private IEnumerable<Compiler> GetCompilers()
        {
            IEnumerable<Scheme.Set> sets = SetScheme(GetScheme());

            foreach (Scheme.Set set in sets)
            {
                set.Configuration.Invoke();
            }

            return sets.Select(x => x.Compiler);
        }

        private Scheme GetScheme()
        {
            PreviewCompilersBuilder preview = new PreviewCompilersBuilder();

            MethodCompiler methodCompiler = new MethodCompiler(preview);
            UrlCompiler urlCompiler = new UrlCompiler(preview);
            ContentCompiler contentCompiler = new ContentCompiler(preview);
            HeadersCompiler headersCompiler = new HeadersCompiler(preview);
            RequestCompiler requestCompiler = new RequestCompiler(preview);

            return new Scheme(new Scheme.Set(methodCompiler, () => SetMethod(methodCompiler)),
                              new Scheme.Set(urlCompiler, () => SetUrl(urlCompiler)),
                              new Scheme.Set(contentCompiler, () => SetContent(contentCompiler)),
                              new Scheme.Set(headersCompiler, () => SetHeaders(headersCompiler)),
                              new Scheme.Set(requestCompiler, () => SetRequest(requestCompiler)));
        }

        protected abstract void SetMethod(MethodCompiler compiler);

        protected abstract void SetUrl(UrlCompiler compiler);

        protected virtual void SetContent(ContentCompiler compiler) { }

        protected virtual void SetHeaders(HeadersCompiler compiler) { }

        protected virtual void SetRequest(RequestCompiler compiler) { }

        protected virtual IEnumerable<Scheme.Set> SetScheme(Scheme scheme)
        {
            return scheme.Default();
        }

        #region Compilers

        public abstract class Compiler
        {
            internal abstract void Compile(HttpRequestMessage request);
        }

        public sealed class MethodCompiler : Compiler
        {
            private event Action OnMethodChangedEvent;

            private HttpMethod _method;

            private PreviewCompilersBuilder _preview;

            internal MethodCompiler(PreviewCompilersBuilder preview)
            {
                _preview = preview;

                OnMethodChangedEvent += () => _preview.Method = _method;
            }

            public IPreviewCompilers Preview => _preview;

            internal override void Compile(HttpRequestMessage request)
            {
                request.Method = _method;
            }

            public void Set(HttpMethod method)
            {
                _method = method;

                OnMethodChangedEvent?.Invoke();
            }

            public void Set(string method)
            {
                Set(new HttpMethod(method));
            }
        }

        public sealed class UrlCompiler : Compiler
        {
            private event Action OnUrlChangedEvent;

            private Uri _url;

            private PreviewCompilersBuilder _preview;

            private UrlTool _tool;

            internal UrlCompiler(PreviewCompilersBuilder preview)
            {
                _preview = preview;
                _tool = new UrlTool();

                OnUrlChangedEvent += () => _preview.Url = _url;
            }

            public IPreviewCompilers Preview => _preview;

            public UrlTool Tool => _tool;

            internal override void Compile(HttpRequestMessage request)
            {
                request.RequestUri = _url;
            }

            public void Set(Uri url)
            {
                _url = url;

                OnUrlChangedEvent?.Invoke();
            }

            public void Set(string url)
            {
                Set(new Uri(url));
            }

            public void Set(UriBuilder builder)
            {
                Set(builder.Uri);
            }

            public void Set(Action<UriBuilder> callback)
            {
                UriBuilder builder = new UriBuilder();

                callback.Invoke(builder);

                Set(builder.Uri);
            }
        }

        public sealed class ContentCompiler : Compiler
        {
            private event Action OnContentChangedEvent;

            private const string _media = "application/json";

            private HttpContent _content;

            private PreviewCompilersBuilder _preview;

            private ContentTool _tool;

            internal ContentCompiler(PreviewCompilersBuilder preview)
            {
                _preview = preview;
                _tool = new ContentTool();

                OnContentChangedEvent += () => _preview.Content = _content;
            }

            public IPreviewCompilers Preview => _preview;

            public ContentTool Tool => _tool;

            internal override void Compile(HttpRequestMessage request)
            {
                request.Content = _content;
            }

            public void Set(HttpContent content)
            {
                _content = content;

                OnContentChangedEvent?.Invoke();
            }

            public void Set(StringContent content)
            {
                Set((HttpContent)content);
            }

            public void Set(ByteArrayContent content)
            {
                Set((HttpContent)content);
            }

            public void Set(string content, Encoding encoding, string media = _media)
            {
                Set(new StringContent(content, encoding, media));
            }

            public void Set(IEnumerable<KeyValuePair<string, string>> content)
            {
                Set(new FormUrlEncodedContent(content));
            }
        }

        public sealed class HeadersCompiler : Compiler
        {
            private event Action OnHeadersChangedEvent;

            private List<KeyValuePair<string, string>> _headers;

            private PreviewCompilersBuilder _preview;

            internal HeadersCompiler(PreviewCompilersBuilder preview)
            {
                _headers = new List<KeyValuePair<string, string>>();

                _preview = preview;

                OnHeadersChangedEvent += () => _preview.Headers = _headers;
            }

            public IPreviewCompilers Preview => _preview;

            internal override void Compile(HttpRequestMessage request)
            {
                foreach (KeyValuePair<string, string> header in _headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            public HeadersCompiler Set(IEnumerable<KeyValuePair<string, string>> headers)
            {
                _headers = headers.ToList();

                OnHeadersChangedEvent?.Invoke();

                return this;
            }

            public HeadersCompiler Add(IEnumerable<KeyValuePair<string, string>> headers)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    _headers.Add(header);
                }

                OnHeadersChangedEvent?.Invoke();

                return this;
            }

            public HeadersCompiler Add(KeyValuePair<string, string> header)
            {
                _headers.Add(header);

                OnHeadersChangedEvent?.Invoke();

                return this;
            }

            public HeadersCompiler Add(string key, string value)
            {
                _headers.Add(new KeyValuePair<string, string>(key, value));

                OnHeadersChangedEvent?.Invoke();

                return this;
            }
        }

        public sealed class RequestCompiler : Compiler
        {
            private Action<HttpRequestMessage> _callback;

            private PreviewCompilersBuilder _preview;

            internal RequestCompiler(PreviewCompilersBuilder container)
            {
                _preview = container;
            }

            public IPreviewCompilers Preview => _preview;

            internal override void Compile(HttpRequestMessage request)
            {
                _callback?.Invoke(request);
            }

            public void Set(Action<HttpRequestMessage> callback)
            {
                _callback = callback;
            }
        }

        #endregion

        #region Scheme

        public class Scheme
        {
            internal Scheme(Set method,
                            Set url,
                            Set content,
                            Set headers,
                            Set request)
            {
                Method = method;
                Url = url;
                Content = content;
                Headers = headers;
                Request = request;
            }

            public Set Method { get; private set; }

            public Set Url { get; private set; }

            public Set Content { get; private set; }

            public Set Headers { get; private set; }

            public Set Request { get; private set; }

            public IEnumerable<Set> Configure(params Set[] sets)
            {
                return new HashSet<Set>(sets);
            }

            public IEnumerable<Set> Default()
            {
                return Configure(Method, Url, Content, Headers, Request);
            }

            public class Set
            {
                internal Set(Compiler compiler, Action configuration)
                {
                    Compiler = compiler;
                    Configuration = configuration;
                }

                internal Compiler Compiler { get; private set; }

                internal Action Configuration { get; private set; }
            }
        }

        #endregion

        #region Preview

        public class PreviewCompilersBuilder : IPreviewCompilers
        {
            internal PreviewCompilersBuilder()
            {
            }

            public HttpMethod Method { get; set; }

            public Uri Url { get; set; }

            public HttpContent Content { get; set; }

            public IEnumerable<KeyValuePair<string, string>> Headers { get; set; }

        }

        public interface IPreviewCompilers
        {
            HttpMethod Method { get; }

            Uri Url { get; }

            HttpContent Content { get; }

            IEnumerable<KeyValuePair<string, string>> Headers { get; }
        }

        #endregion

        #region Tools

        public interface ITool
        {
        }

        public class UrlTool : ITool
        {
            internal UrlTool()
            {
            }

            /// <summary>
            /// Build query string (without "?").
            /// </summary>
            /// <param name="queries"></param>
            /// <returns></returns>
            public string GetQuery(IEnumerable<KeyValuePair<string, string>> queries)
            {
                StringBuilder query = new StringBuilder();

                for (int i = 0; i < queries.Count(); i++)
                {
                    query.Append(i == queries.Count() - 1 ?
                                    $"{queries.ElementAt(i).Key}={queries.ElementAt(i).Value}" :
                                    $"{queries.ElementAt(i).Key}={queries.ElementAt(i).Value}&");
                }

                return query.ToString();
            }

            /// <summary>
            /// Encode a URL string.
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public string UrlEncode(string value)
            {
                return HttpUtility.UrlEncode(value);
            }
        }

        public class ContentTool : ITool
        {
            internal ContentTool()
            {
            }

            /// <summary>
            /// Serialization method for protobuf-net object.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public byte[] Protobuff(object obj)
            {
                MemoryStream stream;

                using (stream = new MemoryStream())
                    Serializer.Serialize(stream, obj);

                return stream.ToArray();
            }

            /// <summary>
            /// Serialization method for Newtonsoft.Json object.
            /// </summary>
            /// <param name="obj"></param>
            /// <param name="settings"></param>
            /// <returns></returns>
            public string Json(object obj, JsonSerializerSettings settings = null)
            {
                return JsonConvert.SerializeObject(obj, settings);
            }
        }

        #endregion
    }
}
