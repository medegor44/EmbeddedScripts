using System;
using System.Threading.Tasks;
using EmbeddedScripts.CSharp.Roslyn.Scripting.CodeGeneration;
using EmbeddedScripts.CSharp.Shared;
using EmbeddedScripts.Shared;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace EmbeddedScripts.CSharp.Roslyn.Scripting
{
    public class ScriptCodeRunner : ICodeRunner
    {
        public ScriptCodeRunner(string code) 
            : this(code, _ => CSharpCodeRunnerOptions.Default)
        {
        }

        public ScriptCodeRunner(string code, Func<CSharpCodeRunnerOptions, CSharpCodeRunnerOptions> opts)
        {
            Code = code;
            RunnerOptions = opts(RunnerOptions);
        }

        public ScriptCodeRunner WithOptions(Func<CSharpCodeRunnerOptions, CSharpCodeRunnerOptions> opts)
        {
            RunnerOptions = opts(CSharpCodeRunnerOptions.Default);
            return this;
        }

        public ScriptCodeRunner AddOptions(Func<CSharpCodeRunnerOptions, CSharpCodeRunnerOptions> opts)
        {
            RunnerOptions = opts(RunnerOptions);
            return this;
        }

        public async Task RunAsync() =>
            await CSharpScript.RunAsync(
                GenerateScriptCode(Code), 
                BuildEngineOptions(), 
                new Globals { Container = RunnerOptions.Container });

        private string GenerateScriptCode(string userCode) => 
            new CodeGeneratorForScripting()
                .GenerateCode(userCode, RunnerOptions.Container);

        private ScriptOptions BuildEngineOptions() =>
            ScriptOptions.Default.WithReferencesFromContainer(RunnerOptions.Container);

        private CSharpCodeRunnerOptions RunnerOptions { get; set; } = CSharpCodeRunnerOptions.Default;
        private string Code { get; }
    }
}
