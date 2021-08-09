using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using MoonSharp.Interpreter;

namespace EmbeddedScripts.Lua.Moonsharp
{
    public class MoonsharpRunner : ICodeRunner, IEvaluator
    {
        private Script _script;
        private readonly Container _container = new();

        public Task<T> EvaluateAsync<T>(string expression)
        {
            _script ??= new Script();

            try
            {
                _script.RegisterVariablesFromContainer(_container);
                
                var val = _script.DoString(expression);
                
                return Task.FromResult((T)val.ToObject());
            }
            catch (SyntaxErrorException e)
            {
                throw new ScriptSyntaxErrorException(e);
            }
            catch (ScriptRuntimeException e)
            {
                throw new ScriptRuntimeErrorException(e);
            }
            catch (Exception e) when (e is InternalErrorException or DynamicExpressionException)
            {
                throw new ScriptEngineErrorException(e);
            }
        }

        public Task<object> EvaluateAsync(string expression) => 
            EvaluateAsync<object>(expression);
        
        public Task<ICodeRunner> RunAsync(string code)
        {
            EvaluateAsync(code);
            
            return Task.FromResult(this as ICodeRunner);
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);
            
            return this;
        }
    }
}