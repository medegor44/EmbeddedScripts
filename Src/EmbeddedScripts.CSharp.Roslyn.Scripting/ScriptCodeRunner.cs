using System;
using System.Threading.Tasks;
using EmbeddedScripts.CSharp.Roslyn.Scripting.CodeGeneration;
using EmbeddedScripts.Shared;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace EmbeddedScripts.CSharp.Roslyn.Scripting
{
    public class ScriptCodeRunner : ICodeRunner
    {
        private Container container = new();
        private ScriptOptions roslynOptions = ScriptOptions.Default;

        public ScriptCodeRunner AddEngineOptions(Func<ScriptOptions, ScriptOptions> optionsFunc)
        {
            roslynOptions = optionsFunc(roslynOptions);
            return this;
        }

        public async Task RunAsync(string code) =>
            await CSharpScript.RunAsync(
                GenerateScriptCode(code), 
                BuildEngineOptions(), 
                new Globals { Container = container });

        public ICodeRunner Register<T>(T obj, string alias)
        {
            container.Register(obj, alias);
            return this;
        }

        private string GenerateScriptCode(string userCode) => 
            new CodeGeneratorForScripting()
                .GenerateCode(userCode, container);

        private ScriptOptions BuildEngineOptions() =>
            roslynOptions.WithReferencesFromContainer(container);
    }
}
