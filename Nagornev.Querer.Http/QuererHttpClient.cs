using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nagornev.Querer.Http
{
    public class QuererHttpClient : QuererClient<HttpRequestMessage, HttpResponseMessage>
    {
        private readonly HttpClient _httpClient;

        public QuererHttpClient()
            : this(new HttpClient())
        {
        }

        public QuererHttpClient(HttpMessageHandler handler)
            : this(new HttpClient(handler))
        {
        }

        public QuererHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<HttpResponseMessage> SendAsync(QuererRequestCompiler<HttpRequestMessage> compiler)
        {
            HttpRequestMessage request = Compile(compiler);

            return await _httpClient.SendAsync(request);
        }

        public override async Task<IEnumerable<HttpResponseMessage>> SendAsync(QuererRequestsCompiler<HttpRequestMessage> compiler)
        {
            List<HttpResponseMessage> responses = new List<HttpResponseMessage>();

            IEnumerable<HttpRequestMessage> requests = Compile(compiler);

            foreach (HttpRequestMessage request in requests)
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                responses.Add(response);
            }

            return responses;
        }

        public override async Task SendAsync(QuererRequestCompiler<HttpRequestMessage> compiler, QuererResponseHandler<HttpResponseMessage> handler)
        {
            HttpResponseMessage response = await SendAsync(compiler);

            Handle(handler, response);
        }


        public override async Task SendAsync(QuererRequestsCompiler<HttpRequestMessage> compiler, QuererResponsesHandler<HttpResponseMessage> handler)
        {
            IEnumerable<HttpResponseMessage> responses = await SendAsync(compiler);

            Handle(handler, responses);
        }
    }
}
