using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using MoonSharp.Interpreter;

namespace EmbeddedScripts.Lua.Moonsharp
{
    public class MoonsharpRunner : ICodeRunner
    {
        private Script _script;
        private Container _container = new();

        public Task<object> EvaluateAsync(string expression)
        {
            _script ??= new Script();
            
            _script.RegisterVariablesFromContainer(_container);
            var val = _script.DoString(expression);
            
            return Task.FromResult(val.ToObject());
        }
        
        public Task<ICodeRunner> RunAsync(string code)
        {
            _script ??= new Script();

            try
            {
                _script.RegisterVariablesFromContainer(_container);
                _script.DoString(code);
            }
            catch (SyntaxErrorException e)
            {
                throw new ScriptSyntaxErrorException(e);
            }
            catch (Exception e) when (e is InternalErrorException or DynamicExpressionException)
            {
                throw new ScriptEngineErrorException(e);
            }
            catch (ScriptRuntimeException e)
            {
                throw new ScriptRuntimeErrorException(e);
            }

            return Task.FromResult(this as ICodeRunner);
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);
            return this;
        }
    }
}