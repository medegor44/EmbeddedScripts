using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace EmbeddedScripts.JS.ClearScriptV8
{
    public class ClearScriptV8Runner : ICodeRunner, IContinuable, IDisposable
    {
        private Container _container = new();
        private V8ScriptEngine _engine;

        public Task RunAsync(string code)
        {
            _engine = new V8ScriptEngine();
            _engine.AddHostObjectsFromContainer(_container);

            try
            {
                _engine.Execute(code);
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

        public Task ContinueWithAsync(string code)
        {
            _engine.AddHostObjectsFromContainer(_container);
            _engine.Execute(code);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _engine?.Dispose();
        }
    }
}
