using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Nagornev.Querer.Http
{
    public class QuererHttpRequestsMessageCompiler : QuererRequestsCompiler<HttpRequestMessage>
    {
        private IEnumerable<QuererHttpRequestMessageCompiler> _compilers;

        public QuererHttpRequestsMessageCompiler()
        {
            _compilers = GetCompilers();
        }

        public QuererHttpRequestsMessageCompiler(params QuererHttpRequestMessageCompiler[] compilers)
            : this((IEnumerable<QuererHttpRequestMessageCompiler>)compilers)
        {
        }

        public QuererHttpRequestsMessageCompiler(IEnumerable<QuererHttpRequestMessageCompiler> compilers)
        {
            _compilers = compilers;
        }

        protected override IEnumerable<HttpRequestMessage> Compile()
        {
            return !(_compilers is null) || _compilers.Count() < 1 ?
                     Compile(_compilers) :
                     throw new ArgumentNullException("The compilers collection is null. Ovveride the 'GetCompilers' method or set compilers in constructor.");
        }

        protected virtual IEnumerable<QuererHttpRequestMessageCompiler> GetCompilers()
        {
            return default;
        }
    }
}
