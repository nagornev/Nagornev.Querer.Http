using System.Collections.Generic;
using System.Net.Http;

namespace Nagornev.Querer.Http
{
    public class QuererHttpRequestsMessageCompiler : QuererRequestsCompiler<HttpRequestMessage>
    {
        private IEnumerable<QuererHttpRequestMessageCompiler> _compilers;

        public QuererHttpRequestsMessageCompiler()
        {
        }

        public QuererHttpRequestsMessageCompiler(IEnumerable<QuererHttpRequestMessageCompiler> compilers)
        {
            _compilers = compilers;
        }

        public QuererHttpRequestsMessageCompiler(params QuererHttpRequestMessageCompiler[] compilers)
        {
            _compilers = compilers;
        }

        protected override IEnumerable<HttpRequestMessage> Compile()
        {
            return Compile(_compilers);
        }
    }
}
