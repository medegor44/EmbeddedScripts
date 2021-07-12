using System.Linq;
using EmbeddedScripts.Shared;
using Microsoft.ClearScript.V8;

namespace EmbeddedScripts.JS.ClearScriptV8
{
    public static class V8ScriptEngineExtensions
    {
        internal static V8ScriptEngine AddHostObjectByAlias(this V8ScriptEngine engine, Container container,
            string alias)
        {
            var type = container.GetTypeByAlias(alias);
            var instance = container.Resolve(alias);

            if (type.IsPrimitive || type == typeof(string))
                engine.Script[alias] = instance;
            else
                engine.AddHostObject(alias, instance);

            return engine;
        }

        internal static V8ScriptEngine AddHostObjectsFromContainer(this V8ScriptEngine engine, Container container) =>
            container.VariableAliases
                .Aggregate(engine, (current, alias) => 
                    current.AddHostObjectByAlias(container, alias));
    }
}