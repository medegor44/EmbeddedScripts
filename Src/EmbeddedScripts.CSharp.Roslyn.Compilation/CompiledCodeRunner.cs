using System;
using System.Threading.Tasks;
using EmbeddedScripts.CSharp.Roslyn.Compilation.CodeGeneration;
using EmbeddedScripts.Shared;
using EmbeddedScripts.CSharp.Shared;

namespace EmbeddedScripts.CSharp.Roslyn.Compilation
{
    public class CompiledCodeRunner : ICodeRunner
    {
        public CompiledCodeRunner(string code) 
            : this(code, _ => CSharpCodeRunnerOptions.Default)
        {
        }

        public CompiledCodeRunner(string code, Func<CSharpCodeRunnerOptions, CSharpCodeRunnerOptions> opts)
        {
            Code = code;
            CodeGenerator = new();

            RunnerOptions = opts(CSharpCodeRunnerOptions.Default);
        }

        public CompiledCodeRunner AddOptions(Func<CSharpCodeRunnerOptions, CSharpCodeRunnerOptions> opts)
        {
            RunnerOptions = opts(RunnerOptions);
            return this;
        }

        public CompiledCodeRunner WithOptions(Func<CSharpCodeRunnerOptions, CSharpCodeRunnerOptions> opts)
        {
            RunnerOptions = opts(CSharpCodeRunnerOptions.Default);
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
        private CSharpCodeRunnerOptions RunnerOptions { get; set; }
        private CodeGeneratorForCompilation CodeGenerator { get; }
    }
}
