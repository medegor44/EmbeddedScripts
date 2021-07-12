using System;
using System.Threading.Tasks;
using EmbeddedScripts.CSharp.Roslyn.Compilation.CodeGeneration;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.CSharp.Roslyn.Compilation
{
    public class CompiledCodeRunner : ICodeRunner
    {
        public CompiledCodeRunner() 
            : this(_ => CodeRunnerConfig.Default)
        {
        }

        public CompiledCodeRunner(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            CodeGenerator = new();

            RunnerConfig = configFunc(CodeRunnerConfig.Default);
        }

        public ICodeRunner AddConfig(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            RunnerConfig = configFunc(RunnerConfig);
            return this;
        }

        public async Task RunAsync(string code)
        {
            var container = RunnerConfig.Container;

            var generatedCode = CodeGenerator.GenerateCode(code, container);

            var compilation = new CodeCompiler(generatedCode, container).Compilation;
            var instance = new InstanceCreator(compilation).CreateInstanceOf(CodeGenerator.ClassName, container);

            await Task.Run(() =>
                instance.InvokeMethod(CodeGenerator.MethodName));
        }

        private CodeRunnerConfig RunnerConfig { get; set; }
        private CodeGeneratorForCompilation CodeGenerator { get; }
    }
}
