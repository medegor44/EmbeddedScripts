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
        public ScriptCodeRunner() 
            : this(_ => CodeRunnerConfig.Default)
        {
        }

        public ScriptCodeRunner(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            RunnerConfig = configFunc(RunnerConfig);
        }

        public ICodeRunner AddConfig(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            RunnerConfig = configFunc(RunnerConfig);
            return this;
        }

        public ScriptCodeRunner AddEngineOptions(Func<ScriptOptions, ScriptOptions> optionsFunc)
        {
            RoslynOptions = optionsFunc(RoslynOptions);
            return this;
        }

        public async Task RunAsync(string code) =>
            await CSharpScript.RunAsync(
                GenerateScriptCode(code), 
                BuildEngineOptions(), 
                new Globals { Container = RunnerConfig.Container });

        private string GenerateScriptCode(string userCode) => 
            new CodeGeneratorForScripting()
                .GenerateCode(userCode, RunnerConfig.Container);

        private ScriptOptions BuildEngineOptions() =>
            RoslynOptions.WithReferencesFromContainer(RunnerConfig.Container);

        private CodeRunnerConfig RunnerConfig { get; set; } = CodeRunnerConfig.Default;
        private ScriptOptions RoslynOptions { get; set; } = ScriptOptions.Default;
    }
}
