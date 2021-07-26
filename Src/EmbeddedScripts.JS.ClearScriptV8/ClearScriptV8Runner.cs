using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace EmbeddedScripts.JS.ClearScriptV8
{
    public class ClearScriptV8Runner : ICodeRunner
    {
        private Container _container = new();

        public Task RunAsync(string code)
        {
            using var engine = new V8ScriptEngine();
            engine.AddHostObjectsFromContainer(_container);

            try
            {
                engine.Execute(code);
            }
            catch (ScriptEngineException e)
            {
                if (e.Message.StartsWith("SyntaxError"))
                    throw new ScriptSyntaxErrorException(e);

                var tripleInnerException = e.InnerException?.InnerException?.InnerException; 
                throw tripleInnerException ?? new ScriptRuntimeErrorException(e);
            }

            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);
            return this;
        }
    }
}
