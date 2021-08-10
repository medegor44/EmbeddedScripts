using System;
using MoonSharp.Interpreter;

namespace EmbeddedScripts.Lua.Moonsharp
{
    public static class MoonsharpScriptExtensions
    {
        public static void RegisterVariable<T>(this Script script, T obj, string alias)
        {
            var type = typeof(T);
            
            if (obj is not Delegate)
                UserData.RegisterType(type);
            
            script.Globals[alias] = obj;
        }
    }
}