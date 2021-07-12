using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using Microsoft.ClearScript.V8;

namespace EmbeddedScripts.JS.ClearScriptV8
{
    public class ClearScriptV8Runner : ICodeRunner
    {
        private Container container = new();

        public async Task RunAsync(string code)
        {
            using var engine = new V8ScriptEngine();
            engine.AddHostObjectsFromContainer(container);

            await Task.Run(() => engine.Execute(code));
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            container.Register(obj, alias);
            return this;
        }
    }
}
