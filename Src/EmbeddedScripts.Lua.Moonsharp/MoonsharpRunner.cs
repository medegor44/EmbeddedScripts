using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using EmbeddedScripts.Shared.Exceptions;
using MoonSharp.Interpreter;

namespace EmbeddedScripts.Lua.Moonsharp
{
    public class MoonsharpRunner : ICodeRunner
    {
        private Script _script = new();
        
        public Task RunAsync(string code)
        {
            try
            {
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
            
            return Task.CompletedTask;
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            if (obj is not Delegate)
                UserData.RegisterType<T>();
            
            _script.Globals[alias] = obj;
            return this;
        }
    }
}