using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace EmbeddedScripts.JS.ClearScriptV8
{
    public class ClearScriptV8Runner : ICodeRunner, IEvaluator, IDisposable
    {
        private readonly Container _container = new();
        private V8ScriptEngine _engine;

        public Task<T> EvaluateAsync<T>(string expression)
        {
            _engine ??= new V8ScriptEngine();

            try
            {
                _engine.AddHostObjectsFromContainer(_container);
                var val = (T)_engine.Evaluate(expression);
                return Task.FromResult(val);

            }
            catch (ScriptEngineException e)
            {
                if (e.Message.StartsWith("SyntaxError"))
                    throw new ScriptSyntaxErrorException(e);

                var tripleInnerException = e.InnerException?.InnerException?.InnerException;
                throw tripleInnerException ?? new ScriptRuntimeErrorException(e);
            }
        }

        public Task<ICodeRunner> RunAsync(string code)
        {
            EvaluateAsync<object>(code);

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
