using System;
using System.Linq;
using EmbeddedScripts.Shared;
using MoonSharp.Interpreter;

namespace EmbeddedScripts.Lua.Moonsharp
{
    public static class MoonsharpScriptExtensions
    {
        private static Script RegisterVariableFromContainer(this Script script, string alias, Container container)
        {
            var obj = container.Resolve(alias);
            var type = container.GetTypeByAlias(alias);
            
            if (obj is not Delegate)
                UserData.RegisterType(type);
            
            script.Globals[alias] = obj;
            return script;
        }

        public static Script RegisterVariablesFromContainer(this Script script, Container container) =>
            container.VariableAliases.Aggregate(script, (currentScript, alias) =>
                currentScript.RegisterVariableFromContainer(alias, container));
    }
}