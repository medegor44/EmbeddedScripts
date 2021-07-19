using System;
using ChakraHost.Hosting;

namespace EmbeddedScripts.JS.ChakraCore
{
    public class JsContext
    {
        private JavaScriptContext _context;

        public JsContext(JavaScriptContext context)
        {
            _context = context;
        }

        public JsScope Scope => 
            new (new JavaScriptContext.Scope(_context));

        public void Run(string script)
        {
            using (Scope)
            {
                try
                {
                    JavaScriptContext.RunScript(script);
                }
                catch (JavaScriptScriptException e)
                {
                    var errorMessage = new JsValue(this, e.Error)
                        .GetProperty("message")
                        .ToString();
                    throw new Exception(errorMessage);
                }
            }
        }
        
        public JsValue GlobalObject
        {
            get
            {
                using (Scope)
                    return new JsValue(this, JavaScriptValue.GlobalObject);
            }
        }
    }
}