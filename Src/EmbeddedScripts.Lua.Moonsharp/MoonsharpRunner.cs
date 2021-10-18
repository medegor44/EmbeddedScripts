using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using MoonSharp.Interpreter;

namespace EmbeddedScripts.Lua.Moonsharp
{
    public class MoonsharpRunner : ICodeRunner, IEvaluator
    {
        private readonly Script _script = new();

        public Task<T> EvaluateAsync<T>(string expression)
        {
            try
            {
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
        
        public Task RunAsync(string code)
        {
            EvaluateAsync<object>(code);

            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _script.RegisterVariable(obj, alias);
            
            return this;
        }
    }
}