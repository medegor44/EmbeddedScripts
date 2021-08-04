using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using MoonSharp.Interpreter;

namespace EmbeddedScripts.Lua.Moonsharp
{
    public class MoonsharpRunner : ICodeRunner, IContinuable
    {
        private Script _script;
        private Container _container = new();

        private void ExecuteWithExceptionsHandling(Script script, string code)
        {
            try
            {
                script.DoString(code);
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
        }

        public Task RunAsync(string code)
        {
            _script = new Script()
                .RegisterVariablesFromContainer(_container);

            ExecuteWithExceptionsHandling(_script, code);

            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);
            return this;
        }

        public Task ContinueWithAsync(string code)
        {
            _script.RegisterVariablesFromContainer(_container);
            
            ExecuteWithExceptionsHandling(_script, code);
            
            return Task.CompletedTask;
        }
    }
}