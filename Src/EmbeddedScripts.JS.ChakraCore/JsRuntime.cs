using System;
using ChakraHost.Hosting;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class JsRuntime : IDisposable
    {
        private JavaScriptRuntime _runtime;

        public JsRuntime()
        {
            _runtime = JavaScriptRuntime.Create();
        }

        public JsContext CreateContext()
        {
            return new(_runtime.CreateContext());
        }
        
        public void Dispose()
        {
            _runtime.Dispose();
        }
    }
}