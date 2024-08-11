using System;
using System.Collections.Generic;

namespace Nagornev.Querer.Http
{
    public class QuererHttpRequestsMessageCompilerBuilder
    {
        private List<QuererHttpRequestMessageCompiler> _compilers;

        private QuererHttpRequestsMessageCompilerBuilder()
        {
            _compilers = new List<QuererHttpRequestMessageCompiler>();
        }

        public QuererHttpRequestsMessageCompilerBuilder UseCompiler(Func<QuererHttpRequestMessageCompilerBuilder, QuererHttpRequestMessageCompiler> callback)
        {
            QuererHttpRequestMessageCompiler compiler = callback.Invoke(QuererHttpRequestMessageCompilerBuilder.Create());

            _compilers.Add(compiler);

            return this;
        }

        public QuererHttpRequestsMessageCompiler Build()
        {
            return new QuererHttpRequestsMessageCompiler(_compilers);
        }

        public static QuererHttpRequestsMessageCompilerBuilder Create()
        {
            return new QuererHttpRequestsMessageCompilerBuilder();
        }
    }
}
