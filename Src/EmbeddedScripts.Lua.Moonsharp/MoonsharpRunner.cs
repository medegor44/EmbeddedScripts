using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using MoonSharp.Interpreter;

namespace EmbeddedScripts.Lua.Moonsharp
{
    public class MoonsharpRunner : ICodeRunner
    {
        private Script _script = new();
        
        public Task RunAsync(string code)
        {
            _script.DoString(code);
            
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