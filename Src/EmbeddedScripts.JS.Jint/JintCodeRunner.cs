using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using Esprima;
using Jint;
using Jint.Native;
using Jint.Runtime;

namespace EmbeddedScripts.JS.Jint
{
    public class JintCodeRunner : ICodeRunner, IEvaluator
    {
        private readonly Container _container = new();
        private Options _jintOptions = new();
        private Engine _engine;

        private object ToNumber(JsValue jsNum)
        {
            var num = jsNum.AsNumber();
            
            if (Math.Abs(num - (int)num) < double.Epsilon)
                return (int)num;
            
            return num;
        }
        
        public JintCodeRunner AddEngineOptions(Func<Options, Options> optionsFunc)
        {
            _jintOptions = optionsFunc(_jintOptions);
            
            return this;
        }

        public Task<T> EvaluateAsync<T>(string expression)
        {
            _engine ??= new Engine(_jintOptions);

            try
            {
                _engine.SetValuesFromContainer(_container);
                var val = _engine.Evaluate(expression);

                if (val.Type == Types.Number)
                    return Task.FromResult((T)ToNumber(val));
                
                return Task.FromResult((T)val.ToObject());
            }
            catch (JavaScriptException e)
            {
                throw new ScriptRuntimeErrorException(e.Error.ToString(), e);
            }
            catch (ParserException e)
            {
                throw new ScriptSyntaxErrorException(e.Message, e);
            }
            catch (Exception e) when (e is
                MemoryLimitExceededException or
                ExecutionCanceledException or
                RecursionDepthOverflowException or
                JintException or
                StatementsCountOverflowException)
            {
                throw new ScriptEngineErrorException(e.Message, e);
            }
        }
        
        public Task<ICodeRunner> RunAsync(string code)
        {
            _engine ??= new Engine(_jintOptions);

            EvaluateAsync<object>(code);

            return Task.FromResult(this as ICodeRunner);
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);
            
            return this;
        }
    }
}
