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
        public ScriptCodeRunner(string code) 
            : this(code, _ => CodeRunnerOptions.Default)
        {
        }

        public ScriptCodeRunner(string code, Func<CodeRunnerOptions, CodeRunnerOptions> opts)
        {
            Code = code;
            RunnerOptions = opts(RunnerOptions);
        }

        public ICodeRunner WithOptions(Func<CodeRunnerOptions, CodeRunnerOptions> opts)
        {
            RunnerOptions = opts(CodeRunnerOptions.Default);
            return this;
        }

        public ICodeRunner AddOptions(Func<CodeRunnerOptions, CodeRunnerOptions> opts)
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

        private CodeRunnerOptions RunnerOptions { get; set; } = CodeRunnerOptions.Default;
        private string Code { get; }
    }
}
