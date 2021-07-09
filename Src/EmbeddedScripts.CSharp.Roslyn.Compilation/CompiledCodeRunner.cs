using System;
using System.Threading.Tasks;
using EmbeddedScripts.CSharp.Roslyn.Compilation.CodeGeneration;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.CSharp.Roslyn.Compilation
{
    public class CompiledCodeRunner : ICodeRunner
    {
        public CompiledCodeRunner(string code) 
            : this(code, _ => CodeRunnerOptions.Default)
        {
        }

        public CompiledCodeRunner(string code, Func<CodeRunnerOptions, CodeRunnerOptions> opts)
        {
            Code = code;
            CodeGenerator = new();

            RunnerOptions = opts(CodeRunnerOptions.Default);
        }

        public ICodeRunner AddOptions(Func<CodeRunnerOptions, CodeRunnerOptions> opts)
        {
            RunnerOptions = opts(RunnerOptions);
            return this;
        }

        public ICodeRunner WithOptions(Func<CodeRunnerOptions, CodeRunnerOptions> opts)
        {
            RunnerOptions = opts(CodeRunnerOptions.Default);
            return this;
        }

        public async Task RunAsync()
        {
            var container = RunnerOptions.Container;

            var code = CodeGenerator.GenerateCode(Code, container);

            var compilation = new CodeCompiler(code, container).Compilation;
            var instance = new InstanceCreator(compilation).CreateInstanceOf(CodeGenerator.ClassName, container);

            await Task.Run(() =>
                instance.InvokeMethod(CodeGenerator.MethodName));
        }

        private string Code { get; }
        private CodeRunnerOptions RunnerOptions { get; set; }
        private CodeGeneratorForCompilation CodeGenerator { get; }
    }
}
