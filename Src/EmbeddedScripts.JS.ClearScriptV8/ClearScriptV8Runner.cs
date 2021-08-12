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
        private readonly V8ScriptEngine _engine = new();

        public Task<T> EvaluateAsync<T>(string expression)
        {
            try
            {
                var val = (T)_engine.Evaluate(expression);
                return Task.FromResult(val);

            }
            catch (ScriptEngineException e)
            {
                if (e.Message.StartsWith("SyntaxError"))
                    throw new ScriptSyntaxErrorException(e.Message, e);

                var tripleInnerException = e.InnerException?.InnerException?.InnerException;
                throw tripleInnerException ?? new ScriptRuntimeErrorException(e.Message, e);
            }
        }

        public Task<ICodeRunner> RunAsync(string code)
        {
            EvaluateAsync<object>(code);

            return Task.FromResult(this as ICodeRunner);
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _engine.AddHostObject(obj, alias);
            return this;
        }

        public void Dispose() =>
            _engine?.Dispose();
    }
}
