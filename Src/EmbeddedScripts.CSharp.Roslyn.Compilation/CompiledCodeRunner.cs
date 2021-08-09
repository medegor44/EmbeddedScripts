using System.Threading.Tasks;
using EmbeddedScripts.CSharp.Roslyn.Compilation.CodeGeneration;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.CSharp.Roslyn.Compilation
{
    public class CompiledCodeRunner : ICodeRunner
    {
        private readonly Container _container = new();
        private readonly CodeGeneratorForCompilation _codeGenerator = new();

        public Task<ICodeRunner> RunAsync(string code)
        {
            var generatedCode = _codeGenerator.GenerateCode(code, _container);

            var compilation = new CodeCompiler(generatedCode, _container).Compilation;
            var instance = new InstanceCreator(compilation).CreateInstanceOf(_codeGenerator.ClassName, _container);
            instance.InvokeMethod(_codeGenerator.MethodName);
            
            return Task.FromResult(this as ICodeRunner);
        }

        public ICodeRunner Register<T>(T obj, string alias)
        {
            _container.Register(obj, alias);
            return this;
        }
    }
}
