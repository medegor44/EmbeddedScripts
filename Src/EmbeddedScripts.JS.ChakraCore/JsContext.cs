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
            using (Scope)
            {
                GlobalObject = new JsValue(this, JavaScriptValue.GlobalObject);
            }
        }

        public JsScope Scope =>
            new (new (_context));

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

                    throw errorName switch
                    {
                        "SyntaxError" => new ScriptSyntaxErrorException(stringifyError, e),
                        Constants.HostError => CallbackException,
                        _ => new ScriptRuntimeErrorException(stringifyError, e)
                    };
                }
                catch (Exception e) when (e is
                    JavaScriptUsageException or
                    JavaScriptEngineException or
                    JavaScriptFatalException)
                {
                    throw new ScriptEngineErrorException(e.Message, e);
                }
                finally
                {
                    CallbackException = null;
                }
            }
        }

        public JsValue GlobalObject { get; }
        internal Exception CallbackException { get; set; }
    }
}