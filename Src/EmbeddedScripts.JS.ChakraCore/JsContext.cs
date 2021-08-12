using System;
using ChakraHost.Hosting;
using EmbeddedScripts.Shared.Exceptions;

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
            new(new(_context));

        public JsValue Evaluate(string expression)
        {
            using (Scope)
            {
                try
                {
                    return new JsValue(this, JavaScriptContext.RunScript(expression));
                }
                catch (JavaScriptScriptException e)
                {
                    var error = new JsValue(this, e.Error);
                    var errorName = error.GetProperty("name").ToString();

                    var stringifyError = error.ToString();

                    if (errorName == "SyntaxError")
                        throw new ScriptSyntaxErrorException(stringifyError, e);

                    if (CallbackException == null)
                        throw new ScriptRuntimeErrorException(stringifyError, e);

                    var ex = CallbackException;
                    CallbackException = null;
                    throw ex;
                }
                catch (Exception e) when (e is
                    JavaScriptUsageException or
                    JavaScriptEngineException or
                    JavaScriptFatalException)
                {
                    throw new ScriptEngineErrorException(e.Message, e);
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

        internal Exception CallbackException { get; set; }
    }
}