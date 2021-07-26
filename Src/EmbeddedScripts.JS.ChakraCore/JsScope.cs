using System;
using ChakraHost.Hosting;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class JsScope : IDisposable
    {
        private JavaScriptContext.Scope _scope;

        public JsScope(JavaScriptContext.Scope scope)
        {
            _scope = scope;
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}