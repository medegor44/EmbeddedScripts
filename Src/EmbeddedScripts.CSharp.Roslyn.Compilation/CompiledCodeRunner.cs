using System.Threading.Tasks;
using EmbeddedScripts.CSharp.Roslyn.Compilation.CodeGeneration;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.CSharp.Roslyn.Compilation
{
    public class CompiledCodeRunner : ICodeRunner
    {
        private Container container = new();
        private CodeGeneratorForCompilation codeGenerator = new();

        public async Task RunAsync(string code)
        {
            var generatedCode = codeGenerator.GenerateCode(code, container);

            var compilation = new CodeCompiler(generatedCode, container).Compilation;
            var instance = new InstanceCreator(compilation).CreateInstanceOf(codeGenerator.ClassName, container);

            await Task.Run(() =>
                instance.InvokeMethod(codeGenerator.MethodName));
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            container.Register(obj, alias);
            return this;
        }
    }
}
