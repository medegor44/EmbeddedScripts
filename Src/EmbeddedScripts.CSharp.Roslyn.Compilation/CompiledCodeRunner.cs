using System;
using System.Threading.Tasks;
using EmbeddedScripts.CSharp.Roslyn.Compilation.CodeGeneration;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.CSharp.Roslyn.Compilation
{
    public class CompiledCodeRunner : ICodeRunner
    {
        public CompiledCodeRunner(string code) 
            : this(code, _ => CodeRunnerConfig.Default)
        {
        }

        public CompiledCodeRunner(string code, Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            Code = code;
            CodeGenerator = new();

            RunnerConfig = configFunc(CodeRunnerConfig.Default);
        }

        public ICodeRunner AddConfig(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            RunnerConfig = configFunc(RunnerConfig);
            return this;
        }

        public ICodeRunner WithConfig(Func<CodeRunnerConfig, CodeRunnerConfig> configFunc)
        {
            RunnerConfig = configFunc(CodeRunnerConfig.Default);
            return this;
        }

        public async Task RunAsync()
        {
            var container = RunnerConfig.Container;

            var code = CodeGenerator.GenerateCode(Code, container);

            var compilation = new CodeCompiler(code, container).Compilation;
            var instance = new InstanceCreator(compilation).CreateInstanceOf(CodeGenerator.ClassName, container);

            await Task.Run(() =>
                instance.InvokeMethod(CodeGenerator.MethodName));
        }

        private string Code { get; }
        private CodeRunnerConfig RunnerConfig { get; set; }
        private CodeGeneratorForCompilation CodeGenerator { get; }
    }
}
