using System;
using System.Threading.Tasks;
using EmbeddedScripts.Shared;
using Jint;

namespace EmbeddedScripts.JS.Jint
{
    public class JintCodeRunner : ICodeRunner
    {
        private Container container = new();
        private Options jintOptions = new();

        public JintCodeRunner AddEngineOptions(Func<Options, Options> optionsFunc)
        {
            jintOptions = optionsFunc(jintOptions);
            return this;
        }
        public ICodeRunner Register<T>(T obj, string alias)
        {
            container.Register(obj, alias);
            return this;
        }

        public async Task RunAsync(string code) =>
            await Task.Run(() => 
                new Engine(jintOptions)
                    .SetValuesFromContainer(container)
                    .Execute(code));
    }
}
