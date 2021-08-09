using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace EmbeddedScripts.JS.ClearScriptV8
{
    public class ClearScriptV8Runner : ICodeRunner, IDisposable
    {
        private Container _container = new();
        private V8ScriptEngine _engine;

        public Task<object> EvaluateAsync(string expression) => 
            EvaluateAsync<object>(expression);
        
        public Task<T> EvaluateAsync<T>(string expression)
        {
            _engine ??= new V8ScriptEngine();
            _engine.AddHostObjectsFromContainer(_container);

            return Task.FromResult((T) _engine.Evaluate(expression));
        }

        public Task<ICodeRunner> RunAsync(string code)
        {
            _engine ??= new V8ScriptEngine();
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

            return Task.FromResult(this as ICodeRunner);
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);
            return this;
        }

        public void Dispose()
        {
            _engine?.Dispose();
        }
    }
}
