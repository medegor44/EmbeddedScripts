using EmbeddedScripts.CSharp.Shared.CodeGeneration;
using EmbeddedScripts.Shared;

namespace EmbeddedScripts.CSharp.Roslyn.Scripting.CodeGeneration
{
    public class CodeGeneratorForScripting
    {
        public string GenerateCode(string code, Container container)
        {
            var declaration = ResolvingCodeGenerator.GenerateVariablesDeclaration(container);
            var initialization = ResolvingCodeGenerator.GenerateVariablesInitialization(container, "Container");

            return $@"
{declaration}
{initialization}
{code}
";
        }

        private ResolvingCodeGenerator ResolvingCodeGenerator { get; } = new();
    }
}