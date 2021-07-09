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
            : this(code, _ => CodeRunnerConfig.Default)
        {
        }

        public ScriptCodeRunner(string code, Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            Code = code;
            RunnerConfig = configFunc(RunnerConfig);
        }

        public ICodeRunner WithConfig(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            RunnerConfig = configFunc(CodeRunnerConfig.Default);
            return this;
        }

        public ICodeRunner AddConfig(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            RunnerConfig = configFunc(RunnerConfig);
            return this;
        }

        public async Task RunAsync() =>
            await CSharpScript.RunAsync(
                GenerateScriptCode(Code), 
                BuildEngineOptions(), 
                new Globals { Container = RunnerConfig.Container });

        private string GenerateScriptCode(string userCode) => 
            new CodeGeneratorForScripting()
                .GenerateCode(userCode, RunnerConfig.Container);

        private ScriptOptions BuildEngineOptions() =>
            ScriptOptions.Default.WithReferencesFromContainer(RunnerConfig.Container);

        private CodeRunnerConfig RunnerConfig { get; set; } = CodeRunnerConfig.Default;
        private string Code { get; }
    }
}
