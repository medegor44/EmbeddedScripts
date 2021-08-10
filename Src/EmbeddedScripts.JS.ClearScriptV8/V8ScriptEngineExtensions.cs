using Microsoft.ClearScript.V8;

namespace EmbeddedScripts.JS.ClearScriptV8
{
    public static class V8ScriptEngineExtensions
    {
        public static void AddHostObject<T>(this V8ScriptEngine engine, T instance, string alias)
        {
            var type = typeof(T);
            
            if (type.IsPrimitive || type == typeof(string))
                engine.Script[alias] = instance;
            else
                engine.AddHostObject(alias, instance);
        }
    }
}